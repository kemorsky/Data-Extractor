import "./status-tab.css";
import { Chart as ChartJS, CategoryScale,
  LinearScale,
  BarElement, ArcElement, 
  Title, Tooltip, Legend } from 'chart.js';
import { Bar, Doughnut } from 'react-chartjs-2';
import { getUniqueProperties } from '../../../utils/get-unique-properties';
import type { LocationData } from '../../../utils/types';

ChartJS.register(CategoryScale,
  LinearScale,
  BarElement,
  ArcElement, Title, Tooltip, Legend);

interface StatusGraphProps {
  locations: LocationData[] | undefined;
}

interface CountyStatusGraphProps {
  county: string;
  statuses: string[];
  statusCounts: Record<string, number>;
}

export default function StatusGraph(props: StatusGraphProps) {
  const { locations } = props;
  
  const parentLocations = [
      ...new Set(
        (locations ?? [])
          .filter(location =>
            location.keywords.includes("LocTypeHold") 
            // || 
            // location.keywords.includes("LocTypeCity")
          )
          .map(location => location.name)
      ),
      ].sort();

  console.log(parentLocations);
  
  const statuses = getUniqueProperties(locations, "status")
    .filter(status => status !== "None")
    .sort();

  const statusByCounty = locations?.reduce((acc, location) => {
    const county = location.parentLocation;
    const status = location.status;

    if (!county || !status || !parentLocations.includes(county)) return acc;

      if (!acc[county]) {
        acc[county] = {};
      }

      acc[county][status] = (acc[county][status] ?? 0) + 1;

      return acc;
  }, {} as Record<string, Record<string, number>>);

  console.log(statusByCounty);

  const colors = [
    'rgb(0, 94, 31)',
    'rgb(118, 151, 0)',
    'rgb(43, 255, 43)',
    'rgb(156, 157, 255)',
    'rgb(192, 192, 192)',
    'rgb(150, 0, 0)',
  ];

  const datasets = statuses.map((status, i) => ({
    label: status,
    data: parentLocations.map(
      county => statusByCounty?.[county]?.[status] ?? 0
    ),
    backgroundColor: colors[i],
    barPercentage: 0.5,
  }));

  const data = {
    labels: parentLocations,
    datasets,
  };

  const maxCount = Math.max(
    ...datasets.flatMap(dataset => dataset.data as number[])
  );

  const options = {
    responsive: true,
    plugins: {
        legend: {
            position: 'top' as const,
        },
        title: {
            display: true,
            text: 'LD completion status by county',
        },
    },
    scales: {
      y: {
        beginAtZero: true,
        min: 0,
        max: maxCount + 5,
        ticks: {
          stepSize: 5,
        },
      },
    },
  };

  function CountyStatusGraph({
    county,
    statuses,
    statusCounts,
  }: CountyStatusGraphProps) {
    const data = {
      labels: statuses,
      datasets: [
        {
          label: county,
          data: statuses.map(status => statusCounts[status] ?? 0),
          backgroundColor: statuses.map((_, i) => colors[i]),
        },
      ],
    };

    const countyOptions = {
      responsive: true,
      plugins: {
          legend: {
              position: 'top' as const,
          },
          title: {
              display: true,
              text: `LD completion status for ${county}`,
          },
      },
    };

    return <div className="graphTab__county-graph">
              <Doughnut data={data} options={countyOptions} />
            </div>;
  }

    return (
        <div className="graphTab">
          <Bar className="graphTab__province-graph" data={data} options={options} />

          <section className="graphTab__county-container">
          {/* Individual county graphs */}
            {parentLocations.map(county => (
              <CountyStatusGraph
                key={county}
                county={county}
                statuses={statuses}
                statusCounts={statusByCounty?.[county] ?? {}}
              />
            ))}
          </section>
        </div>
    )
};


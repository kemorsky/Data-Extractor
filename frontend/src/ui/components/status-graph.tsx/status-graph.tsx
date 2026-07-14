import { useQuery } from '@tanstack/react-query';
import { Chart as ChartJS, CategoryScale,
  LinearScale,
  BarElement,
  Title, Tooltip, Legend } from 'chart.js';
import { Bar } from 'react-chartjs-2';
import { locationsQueryOptions } from '../../../queries/locationQueryOptions';

ChartJS.register(CategoryScale,
  LinearScale,
  BarElement,
  Title, Tooltip, Legend);

interface StatusGraphProps {
    statuses: string[];
    parentLocations: string[];
}

export default function StatusGraph(props: StatusGraphProps) {
    const { data: filterResults } = useQuery(locationsQueryOptions());

    const statusByCounty = filterResults?.reduce((acc, location) => {
    const county = location.parentLocation;
    const status = location.status;

        if (!county || !status || !props.parentLocations.includes(county)) return acc;

        if (!acc[county]) {
        acc[county] = {};
        }

        acc[county][status] = (acc[county][status] ?? 0) + 1;

        return acc;
    }, {} as Record<string, Record<string, number>>);

  const colors = [
    'rgb(0, 94, 31)',
    'rgb(118, 151, 0)',
    'rgb(43, 255, 43)',
    'rgb(156, 157, 255)',
    'rgb(192, 192, 192)',
    'rgb(150, 0, 0)',
  ];

  const datasets = props.statuses.map((status, i) => ({
    label: status,
    data: props.parentLocations.map(
      county => statusByCounty?.[county]?.[status] ?? 0
    ),
    backgroundColor: colors[i],
  }));

  const data = {
    labels: props.parentLocations,
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

    return (
        <div style={{ width: "800px", height: "400px" }}>
          <Bar data={data} options={options} />
        </div>
    )
}
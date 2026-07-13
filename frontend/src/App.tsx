import { useNavigate, useParams } from 'react-router'
import viteLogo from './assets/vite.svg'
import './App.css'
import { useQuery } from '@tanstack/react-query'
import { locationFilterQueryOptions, locationsQueryOptions } from './queries/locationQueryOptions'
import { useState } from 'react'

type LocationFilters = {
  statuses: string[];
  types: string[];
  parentLocations: string[];
};

type CheckboxProps = {
  title: string,
  options: string[];
  selected: string[];
  onToggle: (value: string) => void;
};

export default function App() {
  const navigate = useNavigate();
  // const { status, locationType, parentLocation } = useParams();
  const [filters, setFilters] = useState<LocationFilters>({
    statuses: [],
    types: [],
    parentLocations: [],
  });

  const status = "Finalized";
  const locationType = "Ayleid";
  const parentLocation = "County Bruma";

  const { data: locations } = useQuery(locationsQueryOptions());
  const { data: filterResults } = useQuery(locationFilterQueryOptions(status, locationType, parentLocation));

  const filteredLocations = locations?.filter(location =>
    location.locationType?.includes("Ayleid")
  );

  console.log(filteredLocations);
  console.log(filterResults);

  const toggleFilter = (
    category: keyof LocationFilters,
    value: string
  ) => {
    setFilters(prev => {
      const values = prev[category];

      return {
        ...prev,
        [category]: values.includes(value)
          ? values.filter(v => v !== value)
          : [...values, value],
      };
    });
  };

  const handleClickName = (name: string) => {
    navigate(`/location/${encodeURIComponent(name)}`)
  }

  const handleClick = (
    status: string, 
    locationType: string, 
    parentLocation: string) => {
      const data = locationFilterQueryOptions(status, locationType, parentLocation);
      console.log(data);
    
  }

  return (
    <main id="center">
      <section className="filter">
        <section>
          <button onClick={() => handleClick(status, locationType, parentLocation)}>Filter</button>
          <span>County</span>
          <fieldset>
            <legend></legend>
          </fieldset>
          <article>
            <input type="checkbox" />
            <span>Bruma</span>
          </article>
          <input type="radio" />
        </section>
      </section>
      <div className="hero">
        <img src={viteLogo} className="vite" alt="Vite logo" />
        {filteredLocations?.map((location) => (
          <div 
            key={location.id} 
            onClick={() => {handleClickName(location.name ?? "")}}
          >
            <h2>{location.name}</h2>
            <p>{location.parentLocation}</p>
            <p>{location.locationType}</p>
            <p>{location.status}</p>
            <p>{location.relatedQuestName}</p>
            <p>{location.relatedQuestUrl}</p>
          </div>
        ))}
      </div>
    </main>
  )
}
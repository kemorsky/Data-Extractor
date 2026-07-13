import { useNavigate } from 'react-router'
import './App.css'
import { useQuery } from '@tanstack/react-query'
import { locationFilterQueryOptions, locationsQueryOptions } from './queries/locationQueryOptions'
import { useState } from 'react'
import CheckboxGroup from './ui/components/checkbox-group/checkbox-group'
import LocationCard from './ui/components/location-card/location-card'

type LocationFilters = {
  statuses: string[];
  locationTypes: string[];
  parentLocations: string[];
};

export default function App() {
  const navigate = useNavigate();
  // const { status, locationType, parentLocation } = useParams();

   const [filters, setFilters] = useState<LocationFilters>({
    statuses: [],
    locationTypes: [],
    parentLocations: [],
  });

  const { data: locations } = useQuery(locationsQueryOptions());
  const { data: filterResults } = useQuery(locationFilterQueryOptions(filters.statuses, filters.locationTypes, filters.parentLocations));
  
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
    navigate(`/${encodeURIComponent(name)}`)
  }

  return (
    <main id="center">
      <section className="main__wrapper">
        <section className="filter__options">
          <CheckboxGroup 
            title="Parent Location"
            options={["County Anvil", "County Bruma", "County Chorrol"]}
            selected={filters.parentLocations}
            onToggle={(value) => toggleFilter("parentLocations", value)}
          />
          <CheckboxGroup 
            title="Type"
            options={["Ayleid", "Fort", "Fort Ext.", "Fort Imp.", "Cave", "Mine"]}
            selected={filters.locationTypes}
            onToggle={(value) => toggleFilter("locationTypes", value)}
          />
          <CheckboxGroup 
            title="Status"
            options={["Not started", "Redo", "First passed", "Finalized"]}
            selected={filters.statuses}
            onToggle={(value) => toggleFilter("statuses", value)}
          />
        </section>
      
        <div className="hero">
          {filterResults?.map((location) => (
            <LocationCard 
              key={location.id} 
              location={location} 
              handleClickName={handleClickName}
            />
          ))}
        </div>
      </section>
    </main>
  )
}
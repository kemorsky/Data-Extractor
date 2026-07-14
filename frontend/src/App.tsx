import { useNavigate } from 'react-router'
import './App.css'
import { useQuery } from '@tanstack/react-query'
import { locationFilterQueryOptions, locationsQueryOptions } from './queries/locationQueryOptions'
import { useState } from 'react'
import { getUniqueProperties } from './utils/get-unique-properties'
import CheckboxGroup from './ui/components/checkbox-group/checkbox-group'
import LocationCard from './ui/components/location-card/location-card'
import StatusGraph from './ui/components/status-graph.tsx/status-graph'

type LocationFilters = {
  statuses: string[];
  locationTypes: string[];
  parentLocations: string[];
  inhabitants: string[];
};

export default function App() {
  const navigate = useNavigate();
  // const { status, locationType, parentLocation } = useParams();
   
  const [filters, setFilters] = useState<LocationFilters>({
    statuses: [],
    locationTypes: [],
    parentLocations: [],
    inhabitants: []
  });

  const { data: locations } = useQuery(locationsQueryOptions());
  const { data: filterResults } = useQuery(locationFilterQueryOptions(filters.statuses, filters.locationTypes, filters.parentLocations, filters.inhabitants));

  const parentLocations = getUniqueProperties(locations, "parentLocation")
    .filter(location => location.includes("County") || location.includes("Imperial Seat"))
    .sort((a, b) => a.localeCompare(b));

  const locationTypes = getUniqueProperties(locations, "locationType")
    .filter(status => status !== "None");
  
  const statuses = getUniqueProperties(locations, "status")
    .filter(status => status !== "None")
    .sort((a, b) => a.localeCompare(b));

  const inhabitants = getUniqueProperties(locations, "inhabitants")
    .filter(status => status !== "None");

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
    navigate(`/locations/${encodeURIComponent(name)}`)
  };

  return (
    <main id="center">
      <section className="main__wrapper">
        <section className="filter__options">
          <CheckboxGroup 
            key={1}
            title="Parent Location"
            options={parentLocations}
            selected={filters.parentLocations}
            onToggle={(value) => toggleFilter("parentLocations", value)}
          />
          <CheckboxGroup 
            key={2}
            title="Type"
            options={locationTypes}
            selected={filters.locationTypes}
            onToggle={(value) => toggleFilter("locationTypes", value)}
          />
          <CheckboxGroup 
            key={3}
            title="Status"
            options={statuses}
            selected={filters.statuses}
            onToggle={(value) => toggleFilter("statuses", value)}
          />
          <CheckboxGroup 
            key={4}
            title="Inhabitants"
            options={inhabitants}
            selected={filters.inhabitants}
            onToggle={(value) => toggleFilter("inhabitants", value)}
          />
        </section>
      
        <div className="hero">
          <StatusGraph parentLocations={parentLocations} statuses={statuses} />

          <section className="location-card__container">
            {filterResults?.map((location) => (
              <LocationCard 
                key={location.id} 
                location={location} 
                handleClickName={handleClickName}
              />
            ))}
          </section>
        </div>
      </section>
    </main>
  )
}
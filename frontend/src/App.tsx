import './App.css'
import styles from './index.module.css';
import "./assets/fonts/Webfonts/Balgruf.woff";
import { useState } from 'react'
import StatusGraph from './ui/components/status-graph/status-graph'
import type { LocationFilters } from './utils/types'
import LocationDrawer from './ui/components/drawer/drawer'
import Filters from './ui/blocks/filter/filters';
import LocationsTab from './ui/blocks/locations/locations-tab';
import { useQuery } from '@tanstack/react-query';
import { locationsQueryOptions, locationFilterQueryOptions } from './queries/locationQueryOptions';
import { Tabs } from '@base-ui/react/tabs';
import { useSearchParams } from 'react-router';

export default function App() {
  const [ searchParams, setSearchParams ] = useSearchParams();

  const [ filters, setFilters ] = useState<LocationFilters>({
    statuses: searchParams.get("statuses")?.split(",") ?? [],
    keywords: searchParams.get("keywords")?.split(",") ?? [],
    locationCategories: searchParams.get("locationCategories")?.split(",") ?? [],
    locationTypes: searchParams.get("locationTypes")?.split(",") ?? [],
    parentLocationsCities: searchParams.get("parentLocationsCities")?.split(",") ?? [],
    parentLocations: searchParams.get("parentLocations")?.split(",") ?? [],
    inhabitants: searchParams.get("inhabitants")?.split(",") ?? []
  });

  const { data: locations } = useQuery(locationsQueryOptions());
  const { data: filterResults, isLoading, error } = useQuery(locationFilterQueryOptions(
      filters.statuses, 
      filters.locationCategories, 
      filters.locationTypes,
      filters.parentLocations, 
      filters.inhabitants,
  ));
  
  return (
    <main id="center">
      <section className="main__wrapper">
        <Filters 
          filters={filters} 
          setFilters={setFilters} 
          searchParams={searchParams} 
          setSearchParams={setSearchParams}
        />
      
        <Tabs.Root className={styles.Root} defaultValue="overview">
          <Tabs.List className={styles.List}>
            <Tabs.Tab className={styles.Tab} value="locations">
              Locations
            </Tabs.Tab>
            <Tabs.Tab className={styles.Tab} value="graphs">
              Graphs
            </Tabs.Tab>
            <Tabs.Indicator className={styles.Indicator} />
          </Tabs.List>
          <div className={styles.PanelViewport}>
            <Tabs.Panel className={styles.Panel} value="locations">
              <LocationsTab 
                error={error}
                isLoading={isLoading} 
                locations={locations} 
                filterResults={filterResults} 
                searchParams={searchParams}
                setSearchParams={setSearchParams}
              />
              
            </Tabs.Panel>
            <Tabs.Panel className={styles.Panel} value="graphs">
              <StatusGraph locations={locations} />
            </Tabs.Panel>
          </div>
        </Tabs.Root>        
        
        <LocationDrawer />

      </section>
    </main>
  )
}
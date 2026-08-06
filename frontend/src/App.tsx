import './App.css'
import styles from './index.module.css';
import "./assets/fonts/Webfonts/Balgruf.woff";
import { useMemo } from 'react'
import StatusTab from './ui/components/status-graph/status-tab'
import type { LocationFilters } from './utils/types'
import LocationDrawer from './ui/components/drawer/drawer'
import Filters from './ui/blocks/filter/filters';
import { LocationsTab } from './ui/blocks/locations/locations-tab';
import { useQuery } from '@tanstack/react-query';
import { locationsQueryOptions, locationFilterQueryOptions } from './queries/locationQueryOptions';
import { Tabs } from '@base-ui/react/tabs';
import { useLocation, useSearchParams } from 'react-router';
import Footer from './ui/components/shared/footer';
import Navbar from './ui/components/shared/navbar';

export default function App() {
  const [_, setSearchParams] = useSearchParams();

  const location = useLocation();
  console.log(_);

  const listLocation = location.state?.backgroundLocation ?? location;

  const searchParams = useMemo(() => 
    new URLSearchParams(listLocation.search),
    [listLocation.search]
  );

  const filters = useMemo<LocationFilters>(() =>({
    statuses: searchParams.get("statuses")?.split(",") ?? [],
    hasAQuest: searchParams.get("hasQuest") === "true",
    keywords: searchParams.get("keywords")?.split(",") ?? [],
    locationCategories: searchParams.get("locationCategories")?.split(",") ?? [],
    locationTypes: searchParams.get("locationTypes")?.split(",") ?? [],
    parentLocationsCities: searchParams.get("parentLocationsCities")?.split(",") ?? [],
    parentLocations: searchParams.get("parentLocations")?.split(",") ?? [],
    inhabitants: searchParams.get("inhabitants")?.split(",") ?? [],
  }), [searchParams]);

  const { data: locations } = useQuery(locationsQueryOptions());
  const { data: filterResults, isLoading, error } = useQuery(locationFilterQueryOptions(
      filters.statuses, 
      filters.hasAQuest,
      filters.locationCategories, 
      filters.locationTypes,
      filters.parentLocations, 
      filters.inhabitants,
  ));
  
  return (
    <main id="center">
      <Navbar />
      <section className="main__wrapper">
        <Filters 
          locations={locations}
          filterResults={filterResults}
          filters={filters} 
          // setFilters={setFilters} 
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
            <Tabs.Panel key={"locations"} className={styles.Panel} value="locations">
              <LocationsTab 
                error={error}
                isLoading={isLoading} 
                locations={locations} 
                filterResults={filterResults} 
                searchParams={searchParams}
                setSearchParams={setSearchParams}
              />
              
            </Tabs.Panel>
            <Tabs.Panel key={"graphs"} className={styles.Panel} value="graphs">
              <StatusTab locations={locations} />
            </Tabs.Panel>
          </div>
        </Tabs.Root>        

        <LocationDrawer />
      </section>
      <Footer />
    </main>
  )
}
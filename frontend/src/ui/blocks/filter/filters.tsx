import "./filters.css";
import styles from "./filters.module.css";
import { useMemo } from "react"; // , useState
import CheckboxGroup from "../../components/checkbox-group/checkbox-group";
import { getUniqueProperties } from "../../../utils/get-unique-properties";
import type { LocationData, LocationFilters } from "../../../utils/types";
import { getObjectCount } from "../../../utils/get-object-count";
import Quest from "../../../assets/location-icons/Quest-Door.svg"
import { Drawer } from "@base-ui/react";
import Hamburger from "../../../assets/icons/hamburger.svg"
import X from "../../../assets/icons/cross.svg"
import HideFilters from '../../../assets/icons/hide-filters.svg';

interface FilterProps {
    locations: NoInfer<LocationData[]> | undefined
    filterResults: NoInfer<LocationData[]> | undefined
    filters: LocationFilters;
    searchParams: URLSearchParams;
    setSearchParams: (value: React.SetStateAction<URLSearchParams>) => void;
}

export default function Filters(props: FilterProps) {
    const { locations, filterResults, filters, searchParams, setSearchParams } = props;

    // const [ activeFilter, setActiveFilter ] = useState<FilterId | null>(null);

    const allFilters = Object.entries(filters).flatMap(([category, values]) => {
        if (!Array.isArray(values)) {
            return values ? [{
                category: category as keyof LocationFilters,
                value: "Has Quest"
            }] : [];
        }

        return values.map(value => ({
            category: category as Exclude<keyof LocationFilters, "hasQuest">,
            value
        }))
        .sort()
    });
    
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
    
    const parentLocationsCities = [
        ...new Set(
            (locations ?? [])
                .filter(location => 
                    location.keywords.includes("LocTypeCity")
                )
                .map(location => location.name)
        ),
    ].sort();

    const getOptions = <K extends keyof LocationData>(
        key: K,
        invalid: string[] = ["None"]
        ): string[] => {
            const rawOptions = getUniqueProperties(locations, key);

            const flatOptions = rawOptions.flat().map(String);

            const uniqueOptions = Array.from(new Set(flatOptions));

            return uniqueOptions
                .filter(value => value && !invalid.includes(value))
                .sort();
        };

    const locationCategories = getOptions("locationCategory");
    const locationTypes = getOptions("locationType");
    const statuses = getOptions("status");
    const inhabitants = getOptions("inhabitants", [
        "None",
        "",
        "N/A",
    ]);

    // const hasQuestCount = (locations ?? []).filter(location => location.hasQuest).length;
    const categoryCount = useMemo(() => getObjectCount(filterResults ?? [], "locationCategory"), [filterResults]);
    const typeCount = useMemo(() => getObjectCount(filterResults ?? [], "locationType"), [filterResults]);
    const parentLocationCount = useMemo(() => getObjectCount(filterResults ?? [], "parentLocation"), [filterResults]);
    const statusCount = useMemo(() => getObjectCount(filterResults ?? [], "status"), [filterResults]);
    const inhabitantsCount = useMemo(() => getObjectCount(filterResults ?? [], "inhabitants"), [filterResults]);

    const toggleFilter = (
        category: Exclude<keyof LocationFilters, "hasAQuest">,
        value: string
    ) => {
        const values = filters[category];

        const next = {
            ...filters,
            [category]: Array.isArray(values) 
                ? values.includes(value)
                    ? values.filter(v => v !== value)
                    : [...values, value]
                : values,
            };
        
        const params = new URLSearchParams(searchParams);
        params.set("page", "1");

        Object.entries(next).forEach(([key, values]) => {
            if (Array.isArray(values)) {
                if (values.length === 0) {
                    params.delete(key);
                } else {
                    params.set(key, values.join(","));
                }
            } else {
                if (values) {
                    params.set(key, "true");
                } else {
                    params.delete(key);
                }
            }
        });

        setSearchParams(params);
    };

    const toggleHasQuest = () => {
        const next = {
            ...filters,
            hasQuest: !filters.hasQuest,
        };

        const params = new URLSearchParams(searchParams);
        params.set("page", "1");

        if (next.hasQuest === true) {
            params.set("hasQuest", "true");
        } else {
            params.delete("hasQuest");
        }

        setSearchParams(params);
    };

    const handleClearFilters = () => {
        const params = new URLSearchParams();
        params.set("page", "1");
        setSearchParams(params);
    };

    return (
        <Drawer.Root swipeDirection="left" defaultOpen={true} modal={false} disablePointerDismissal>
            
            <section className="filter__tags">
                <Drawer.Trigger className={styles.ButtonOpenFilter}>
                    <img src={Hamburger} alt="filter button icon" width={28} />
                </Drawer.Trigger>

                <section className="filter__tags-container">
                    {allFilters.map(({category, value}) => (
                        <button 
                            className="filter__tags-tag" key={`${category}-${value}`}
                            onClick={() => {
                                if (category === "hasQuest") {
                                    toggleHasQuest();
                                } else {
                                    toggleFilter(category, value);
                                }
                            }}>
                            {value} <img src={X} alt="delete filter icon" width={18} />
                        </button>
                    ))}

                    {allFilters.length > 0 &&
                        <button 
                            className="filter__tags-clear-btn"
                            onClick={() => handleClearFilters()}>
                            Clear Filters
                        </button>
                    }
                </section>
            </section>
            {/* <Drawer.Trigger className={styles.Button}>Open drawer</Drawer.Trigger>        */}
            <Drawer.Portal>         
                {/* <Drawer.Backdrop className={styles.Backdrop} />          */}
                <Drawer.Viewport className={styles.Viewport}>
                    <Drawer.Popup className={styles.Popup}>   
                
                        <Drawer.Content className={styles.Content}>
                            <section className={styles.ButtonCloseContainer}>
                                <Drawer.Close className={styles.ButtonClose}>
                                    <img src={HideFilters} width={28} height={28}/>
                                </Drawer.Close> 
                            </section>             
                            
                            <section className="quest-checkbox">
                                <label className="quest-checkbox__option">
                                    <input
                                        type="checkbox"
                                        className="quest-checkbox__option__checkbox"
                                        checked={filters.hasQuest === null ? false : filters.hasQuest}
                                        onChange={toggleHasQuest}
                                    />
                                    <section style={{height: 30, width: 30, display: "flex", alignItems: "center", justifyContent: "center"}}>
                                        <img width={20} src={Quest} alt="Quest anchor icon" />
                                    </section> 
                                    <span className="quest-checkbox__option-text">
                                        Has Quest
                                    </span>
                                </label>
                            </section>
                            <CheckboxGroup 
                                key={1}
                                title="Location Category"
                                options={locationCategories}
                                counts={categoryCount}
                                selected={filters.locationCategories}
                                onToggle={(value) => toggleFilter("locationCategories", value)}
                            />
                            <CheckboxGroup 
                                key={2}
                                title="City"
                                options={parentLocationsCities}
                                counts={parentLocationCount}
                                selected={filters.parentLocations}
                                onToggle={(value) => toggleFilter("parentLocations", value)}
                            />
                            <CheckboxGroup 
                                key={3}
                                title="County"
                                options={parentLocations}
                                counts={parentLocationCount}
                                selected={filters.parentLocations}
                                onToggle={(value) => toggleFilter("parentLocations", value)}
                            />
                            <CheckboxGroup 
                                key={4}
                                title="Location Type"
                                options={locationTypes}
                                counts={typeCount}
                                selected={filters.locationTypes}
                                onToggle={(value) => toggleFilter("locationTypes", value)}
                            />
                            <CheckboxGroup 
                                key={5}
                                title="Status"
                                options={statuses}
                                counts={statusCount}
                                selected={filters.statuses}
                                onToggle={(value) => toggleFilter("statuses", value)}
                            />
                            <CheckboxGroup 
                                key={6}
                                title="Inhabitants"
                                options={inhabitants}
                                counts={inhabitantsCount}
                                selected={filters.inhabitants}
                                onToggle={(value) => toggleFilter("inhabitants", value)}
                            />             
                        </Drawer.Content>           
                    </Drawer.Popup>      
                </Drawer.Viewport>       
            </Drawer.Portal>     
        </Drawer.Root>
        
    )
};
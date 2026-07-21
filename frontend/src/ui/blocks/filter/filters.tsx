import "./filters.css";
import CheckboxGroup from "../../components/checkbox-group/checkbox-group";
import { getUniqueProperties } from "../../../utils/get-unique-properties";
import type { LocationData, LocationFilters } from "../../../utils/types";
import { getObjectCount } from "../../../utils/get-object-count";

interface FilterProps {
    locations: NoInfer<LocationData[]> | undefined
    filterResults: NoInfer<LocationData[]> | undefined
    filters: LocationFilters;
    setFilters: (value: React.SetStateAction<LocationFilters>) => void;
    searchParams: URLSearchParams;
    setSearchParams: (value: React.SetStateAction<URLSearchParams>) => void;
}

export default function Filters(props: FilterProps) {
    const { locations, filterResults, filters, setFilters, searchParams, setSearchParams } = props;

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
        ) =>
        getUniqueProperties(locations, key)
            .filter(value => !invalid.includes(value as string))
            .sort();

    const locationCategories = getOptions("locationCategory");
    const locationTypes = getOptions("locationType");
    const statuses = getOptions("status");
    const inhabitants = getOptions("inhabitants", [
        "None",
        "",
        "N/A",
    ]);

    const categoryCount = getObjectCount(filterResults ?? [], "locationCategory");
    const typeCount = getObjectCount(filterResults ?? [], "locationType");
    const parentLocationCount = getObjectCount(filterResults ?? [], "parentLocation");
    const statusCount = getObjectCount(filterResults ?? [], "status");
    const inhabitantsCount = getObjectCount(filterResults ?? [], "inhabitants");

    const toggleFilter = (
        category: keyof LocationFilters,
        value: string
    ) => {
        const values = filters[category];

        const next = {
            ...filters,
            [category]: values.includes(value)
                ? values.filter(v => v !== value)
                : [...values, value],
            };

        setFilters(next);

        const params = new URLSearchParams(searchParams);

        params.set("page", "1");

        Object.entries(next).forEach(([key, values]) => {
            if (values.length === 0) {
                params.delete(key);
            } else {
                params.set(key, values.join(","));
            }
        });

        setSearchParams(params);
    };

    const handleClearFilters = () => {
        setFilters({
            statuses: [],
            keywords: [],
            locationCategories: [],
            locationTypes: [],
            parentLocationsCities: [],
            parentLocations: [],
            inhabitants: []
        });

        const params = new URLSearchParams();

        params.set("page", "1");

        setSearchParams(params);
    };

    return (
        <section className="filter__options">
            <legend className="filter__options-title">Filters</legend>
            <button onClick={() => handleClearFilters()}>Clear Filters</button>
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
        </section>
    )
};
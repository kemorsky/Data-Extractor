import "./filters.css";
import CheckboxGroup from "../../components/checkbox-group/checkbox-group";
import { useQuery } from "@tanstack/react-query";
import { locationsQueryOptions } from "../../../queries/locationQueryOptions";
import { getUniqueProperties } from "../../../utils/get-unique-properties";
import type { LocationFilters } from "../../../utils/types";

interface FilterProps {
    filters: LocationFilters;
    setFilters: (value: React.SetStateAction<LocationFilters>) => void
}

export default function Filters(props: FilterProps) {
    const { filters, setFilters } = props;

    const { data: locations } = useQuery(locationsQueryOptions());

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

    const locationCategories = getUniqueProperties(locations, "locationCategory")
        .filter(category => category !== "None")
        .sort();

    const locationTypes = getUniqueProperties(locations, "locationType")
        .filter(type => type !== "None")
        .sort((a, b) => a.localeCompare(b));
    
    const statuses = getUniqueProperties(locations, "status")
        .filter(status => status !== "None")
        .sort((a, b) => a.localeCompare(b));

    const inhabitants = getUniqueProperties(locations, "inhabitants")
        .filter(inhabitants => inhabitants !== "None"  
            && inhabitants !== "" 
            && inhabitants !== "N/A"
        )
        .sort((a, b) => a.localeCompare(b));

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

    return (
        <section className="filter__options">
            <legend className="filter__options-title">Filters</legend>
            <CheckboxGroup 
                key={1}
                title="Location Category"
                options={locationCategories}
                selected={filters.locationCategories}
                onToggle={(value) => toggleFilter("locationCategories", value)}
            />
            <CheckboxGroup 
                key={2}
                title="City"
                options={parentLocationsCities}
                selected={filters.parentLocations}
                onToggle={(value) => toggleFilter("parentLocations", value)}
            />
            <CheckboxGroup 
                key={3}
                title="County"
                options={parentLocations}
                selected={filters.parentLocations}
                onToggle={(value) => toggleFilter("parentLocations", value)}
            />
            <CheckboxGroup 
                key={4}
                title="Location Type"
                options={locationTypes}
                selected={filters.locationTypes}
                onToggle={(value) => toggleFilter("locationTypes", value)}
            />
            <CheckboxGroup 
                key={5}
                title="Status"
                options={statuses}
                selected={filters.statuses}
                onToggle={(value) => toggleFilter("statuses", value)}
            />
            <CheckboxGroup 
                key={6}
                title="Inhabitants"
                options={inhabitants}
                selected={filters.inhabitants}
                onToggle={(value) => toggleFilter("inhabitants", value)}
            />
        </section>
    )
};
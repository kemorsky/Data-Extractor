import "./filters.css";
import CheckboxGroup from "../../components/checkbox-group/checkbox-group";
import { getUniqueProperties } from "../../../utils/get-unique-properties";
import type { LocationData, LocationFilters } from "../../../utils/types";
import { getObjectCount } from "../../../utils/get-object-count";
import Quest from "../../../assets/Quest-Door.svg"

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

    const allFilters = Object.entries(filters).flatMap(([category, values]) => {
        if (!Array.isArray(values)) {
            return values ? [{
                category: category as keyof LocationFilters,
                value: "Has Quest"
            }] : [];
        }

        return values.map(value => ({
            category: category as Exclude<keyof LocationFilters, "hasAQuest">,
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

    // const hasQuestCount = (locations ?? []).filter(location => location.hasQuest).length;
    const categoryCount = getObjectCount(filterResults ?? [], "locationCategory");
    const typeCount = getObjectCount(filterResults ?? [], "locationType");
    const parentLocationCount = getObjectCount(filterResults ?? [], "parentLocation");
    const statusCount = getObjectCount(filterResults ?? [], "status");
    const inhabitantsCount = getObjectCount(filterResults ?? [], "inhabitants");

    const toggleFilter = (
        category: Exclude<keyof LocationFilters, "hasAQuest">,
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
            hasAQuest: !filters.hasAQuest,
        };

        setFilters(next);

        const params = new URLSearchParams(searchParams);
        params.set("page", "1");

        if (next.hasAQuest === true) {
            params.set("hasQuest", "true");
        } else {
            params.delete("hasQuest");
        }

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
            inhabitants: [],
            hasAQuest: false
        });

        const params = new URLSearchParams();
        params.set("page", "1");
        setSearchParams(params);
    };

    return (
        <section className="filter__options">
            <article className="filter__options__header">
                <legend className="filter__options__header-title">Filters</legend>
                <button onClick={() => handleClearFilters()}>Clear Filters</button>
            </article>
            <section className="filter__tags">
                {allFilters.map(({category, value}) => (
                    <span className="filter__tags-tag" key={`${category}-${value}`}>
                        {value}
                        <button className="filter__tags-tag__delete-btn" 
                                onClick={() => {
                                    if (category === "hasAQuest") {
                                        toggleHasQuest();
                                    } else {
                                        toggleFilter(category, value);
                                    }
                                }}>
                            X
                        </button>
                    </span>
                ))}
            </section>
            <section className="quest-checkbox">
                <label className="quest-checkbox__option">
                    <input
                        type="checkbox"
                        className="quest-checkbox__option__checkbox"
                        checked={filters.hasAQuest === null ? false : filters.hasAQuest}
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
        </section>
    )
};
import "./locations-tab.css";
import { useNavigate } from "react-router";
import type { LocationData } from "../../../utils/types";
import LocationCard from "../../components/location-card/location-card";
interface LocationTabProps {
    isLoading: boolean;
    locations: LocationData[] | undefined;
    filterResults: LocationData[] | undefined;
}
export default function LocationsTab(props: LocationTabProps) {
    const { isLoading, locations, filterResults } = props;
    const navigate = useNavigate();
    
    const handleClickName = (name: string) => {
        navigate(`/locations/${encodeURIComponent(name)}`, {
        state: { drawer: true }
        })
    };

    const childrenByParent = new Map<string, LocationData[]>();
    
    for (const loc of locations ?? []) {
        if (!loc.parentLocation) continue;

        if (!childrenByParent.has(loc.parentLocation)) {
            childrenByParent.set(loc.parentLocation, []);
        }

        childrenByParent.get(loc.parentLocation)!.push(loc);
    };

    return (
        <div className="hero">
            {isLoading && <h2>Loading data...</h2>}
            <h2>Locations</h2>
            <section className="location-card__container">
                {filterResults?.map((location) => (
                    <LocationCard 
                        key={location.id} 
                        location={location}
                        childrenByParent={childrenByParent}
                        handleClickName={handleClickName}
                    />
                ))}
            </section>
        </div>
    )
};
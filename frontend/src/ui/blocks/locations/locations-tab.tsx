import "./locations-tab.css";
import { useNavigate } from "react-router";
import type { LocationData } from "../../../utils/types";
import LocationCard from "../../components/location-card/location-card";

interface LocationTabProps {
    error: Error | null;
    isLoading: boolean;
    locations: LocationData[] | undefined;
    filterResults: LocationData[] | undefined;
    searchParams: URLSearchParams;
    setSearchParams: (params: URLSearchParams) => void
}

export default function LocationsTab(props: LocationTabProps) {
    const { isLoading, locations, filterResults, error, searchParams, setSearchParams } = props;

    // const filteredResults = filterResults?.filter(location => 
    //     location.name !== "" &&
    //     location.name !== "None"
    // );

    const page = Number(searchParams.get("page") ?? "1");

    const setPage = (newPage: number) => {
        const params = new URLSearchParams(searchParams);

        params.set("page", newPage.toString());

        setSearchParams(params);
    };

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

    const pageSize = 30;
    const totalPages = Math.ceil(
      (filterResults?.length ?? 0) / pageSize
    );

    const pageResults = filterResults?.slice(
      (page - 1) * pageSize,
      page * pageSize
    );

    return (
        <div className="hero">
            <h2>Locations</h2>
            {isLoading && <h2>Loading data...</h2>}
            {error && <h2>{error.message}</h2>}
            
            <section className="location-card__container">
                {pageResults?.map((location) => (
                    <LocationCard 
                        key={location.id} 
                        location={location}
                        childrenByParent={childrenByParent}
                        handleClickName={handleClickName}
                    />
                ))}
            </section>
            <div className="pagination">
    
                <button
                    disabled={page === 1}
                    onClick={() => setPage(page - 1)}
                >
                    Previous
                </button>

                <span>
                    Page {page} of {totalPages}
                </span>

                <button
                    disabled={page === totalPages}
                    onClick={() => setPage(page + 1)}
                >
                    Next
                </button>

                <button
                    disabled={page === totalPages}
                    onClick={() => setPage(totalPages)}
                >
                    Last
                </button>
            </div>
        </div>
    )
};
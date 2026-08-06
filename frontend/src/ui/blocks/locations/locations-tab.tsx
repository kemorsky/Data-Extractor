import "./locations-tab.css";
import { memo, useMemo, useCallback } from "react";
import { useLocation, useNavigate } from "react-router";
import type { LocationData } from "../../../utils/types";
import LocationCard from "../../components/location-card/location-card";
import ChevronLeft from "../../../assets/icons/chevron-left.svg";
import ChevronRight from "../../../assets/icons/chevron-right.svg";
import ChevronDoubleLeft from "../../../assets/icons/chevron-double-left.svg";
import ChevronDoubleRight from "../../../assets/icons/chevron-double-right.svg";

interface LocationTabProps {
    error: Error | null;
    isLoading: boolean;
    locations: LocationData[] | undefined;
    filterResults: LocationData[] | undefined;
    searchParams: URLSearchParams;
    setSearchParams: (params: URLSearchParams) => void
}

export const LocationsTab = memo(function LocationsTab (props: LocationTabProps) {
    const { isLoading, locations, filterResults, searchParams, error, setSearchParams } = props;

    const navigate = useNavigate();
    const location = useLocation();

    const page = Number(searchParams.get("page") ?? "1");

    const setPage = useCallback((newPage: number) => {
        const params = new URLSearchParams(searchParams);

        params.set("page", newPage.toString());

        setSearchParams(params);
    }, [searchParams, setSearchParams]);

    function slugify(value: string) {
        return value
            .toLowerCase()
            .trim()
            .replace(/['']/g, "")
            .replace(/\s+/g, "-");
    }
    
    const handleClickName = (name: string) => {
        navigate(`/locations/${slugify(name)}`, {
            state: { 
                drawer: true,
                backgroundLocation: location.state?.backgroundLocation ?? location,
            },
        });
    };

    const childrenByParent = useMemo(() => {
        const map = new Map<string, LocationData[]>();
    
        for (const loc of locations ?? []) {
            if (!loc.parentLocation) continue;

            if (!map.has(loc.parentLocation)) {
                map.set(loc.parentLocation, []);
            }

            map.get(loc.parentLocation)!.push(loc);
        };

        return map;
    }, [locations]);

    const pageSize = 30;
    const totalPages = Math.ceil(
      (filterResults?.length ?? 0) / pageSize
    );

    const pageResults = useMemo(() => {
        return filterResults?.slice(
            (page - 1) * pageSize,
            page * pageSize
        );
    }, [filterResults, page]);

    return (
        <div className="hero">
            
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
                    className="pagination__first-btn"
                    disabled={page === 1}
                    onClick={() => setPage(page === 1 ? 1 : 1)}
                >
                    <img src={ChevronDoubleLeft} width={20} height={20} />
                </button>
    
                <button
                    className="pagination__previous-btn"
                    disabled={page === 1}
                    onClick={() => setPage(page - 1)}
                >
                    <img src={ChevronLeft} width={20} height={20} />
                </button>

                <select
                    className="pagination__select"
                    value={page}
                    onChange={(e) => setPage(Number(e.target.value))}
                >
                    {[...Array(totalPages)].map((_, index) => (
                        <option key={index + 1} value={index + 1}>
                            {index + 1}
                        </option>
                    ))}
                </select>

                <span className="pagination__pages-text">
                    of {totalPages}
                </span>

                <button
                    className="pagination__next-btn"
                    disabled={page === totalPages}
                    onClick={() => setPage(page + 1)}
                >
                    <img src={ChevronRight} width={20} height={20} />
                </button>

                <button
                    className="pagination__last-btn"
                    disabled={page === totalPages}
                    onClick={() => setPage(totalPages)}
                >
                    <img src={ChevronDoubleRight} width={20} height={20} />
                </button>
            </div>
        </div>
    )
});
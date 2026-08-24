import "./locations-tab.css";
import { memo, useMemo, useCallback, useState } from "react";
import { useLocation, useNavigate } from "react-router";
import type { LocationData, LocationFilters } from "../../../utils/types";
import LocationCard from "../../components/location-card/location-card";
import ChevronLeft from "../../../assets/icons/chevron-left.svg";
import ChevronRight from "../../../assets/icons/chevron-right.svg";
import ChevronDoubleLeft from "../../../assets/icons/chevron-double-left.svg";
import ChevronDoubleRight from "../../../assets/icons/chevron-double-right.svg";
import Search from "../../../assets/icons/search.svg";

interface LocationTabProps {
    error: Error | null;
    isLoading: boolean;
    filters: LocationFilters;
    filterResults: LocationData[] | undefined;
    searchParams: URLSearchParams;
    setSearchParams: (params: URLSearchParams) => void
}

export const LocationsTab = memo(function LocationsTab (props: LocationTabProps) {
    const { isLoading, filters, filterResults, searchParams, error, setSearchParams } = props;
    const [ isTable, setIsTable ] = useState(false);
    const [searchInput, setSearchInput] = useState(filters.query ?? "");

    const pageSizes = [20, 25, 35, 50, 100];
    const [ numberPerPage, setNumberPerPage ] = useState(pageSizes[2]);

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

    const totalPages = Math.ceil(
      (filterResults?.length ?? 0) / numberPerPage
    );

    const pageResults = useMemo(() => {
        return filterResults?.slice(
            (page - 1) * numberPerPage,
            page * numberPerPage
        );
    }, [filterResults, page, numberPerPage]);

    const handleSearchSubmit = (e?: React.SyntheticEvent) => {
        e?.preventDefault();

        const params = new URLSearchParams(searchParams);
        
        if (searchInput.trim()) {
            params.set("query", searchInput.trim());
        } else {
            params.delete("query");
        }

        params.set("page", "1");
        setSearchParams(params);
    }

    const handleClearSearch = () => {
        const params = new URLSearchParams(searchParams);
        params.set("page", "1");

        params.delete("query");
        
        setSearchInput("");
        setSearchParams(params);
    }

    return (
        <div className="hero">
            
            {isLoading && <h2>Loading data...</h2>}
            {error && <h2>{error.message}</h2>}
            
            <div className="location-card__container-view">
                <section className="location-card__container-view__selects">
                    <span>View style: </span>
                    <select className="location-card__container-view__select">
                        <option 
                            value="Cards"
                            onClick={() => setIsTable(false)}
                        >
                            Cards
                        </option>
                        <option 
                            value="Table"
                            onClick={() => setIsTable(true)}
                        >
                            Table
                        </option>
                    </select>

                    <span style={{ marginLeft: "0.25rem" }}>Items per page: </span>
                    <select 
                        className="location-card__container-view__select"
                        value={numberPerPage}
                        onChange={(e) => 
                            {setNumberPerPage(Number(e.target.value));
                            window.scrollTo({top: 0, behavior: "smooth"});
                        }}
                        >
                        {pageSizes.map((number, index) => (
                            <option key={index} value={number}>
                                {number}
                            </option>
                        ))}
                    </select>
                </section>
                <section className="location-card__container-view__search">    
                    <form className="location-card__container-view__search-form" onSubmit={handleSearchSubmit}>
                        <input 
                            className="location-card__container-view__search-input"
                            type="text"
                            placeholder="Search..."
                            value={searchInput}
                            onChange={(e) => setSearchInput(e.target.value)}
                        />
                        <button 
                            type="submit"
                            className="location-card__container-view__search-btn"
                        >
                            <img src={Search} width={24}/>
                        </button>
                    </form>
                    <button 
                        className="location-card__container-view__clear-btn"
                        onClick={() => {handleClearSearch()}}
                    >
                        Clear
                    </button>
                </section> 
            </div>

            {isTable ? (
                <table className="location-card__table-container">
                    {isTable && 
                        <thead>
                            <tr className="location-card__table-container__header">
                                <td className="location-card__table-container__header-row__cell">Type</td>
                                <td className="location-card__table-container__header-row__cell">Name</td>
                                <td className="location-card__table-container__header-row__cell">Location</td>
                                <td className="location-card__table-container__header-row__cell">Status</td>
                                <td className="location-card__table-container__header-row__cell">Vikunja</td>
                                <td className="location-card__table-container__header-row__cell">Has Quest</td>
                            </tr>
                        </thead>
                    }

                    <tbody>
                        {pageResults?.map((location) => (
                            <LocationCard 
                                key={location.id} 
                                location={location}
                                // childrenByParent={childrenByParent}
                                handleClickName={handleClickName}
                                isTable={isTable}
                            />
                        ))}
                    </tbody>
                </table> 
                ) :
                ( <div className="location-card__cards-container">
                    {pageResults?.map((location) => (
                        <LocationCard 
                            key={location.id} 
                            location={location}
                            // childrenByParent={childrenByParent}
                            handleClickName={handleClickName}
                            isTable={isTable}
                        />
                    ))}
                </div>
             )}

            <div className="pagination">

                <section className="pagination__buttons-container">
                    <button
                        className="pagination__first-btn"
                        disabled={page === 1}
                        onClick={() => setPage(page === 1 ? 1 : 1)}
                    >
                        <img src={ChevronDoubleLeft} width={28} height={28} />
                    </button>
        
                    <button
                        className="pagination__previous-btn"
                        disabled={page === 1}
                        onClick={() => 
                            {setPage(page - 1); 
                            window.scrollTo({top: 0, behavior: "smooth"});
                        }}
                    >
                        <img src={ChevronLeft} width={28} height={28} />
                    </button>
                </section>

                <section className="pagination__select-container">
                    <select
                        className="pagination__select"
                        value={page}
                        onChange={(e) => 
                            {setPage(Number(e.target.value));
                            window.scrollTo({top: 0, behavior: "smooth"});
                        }}
                    >
                        {[...Array(totalPages)].map((_, index) => (
                            <option key={index + 1} value={index + 1}>
                                {index + 1}
                            </option>
                        ))}
                    </select>

                    <span className="pagination__select-pages-text">
                        of {totalPages}
                    </span>
                </section>

                <section className="pagination__buttons-container">
                    <button
                        className="pagination__next-btn"
                        disabled={page === totalPages}
                        onClick={() => 
                            {setPage(page + 1); 
                            window.scrollTo({top: 0, behavior: "smooth"});
                        }}
                    >
                        <img src={ChevronRight} width={28} height={28} />
                    </button>

                    <button
                        className="pagination__last-btn"
                        disabled={page === totalPages}
                        onClick={() => setPage(totalPages)}
                    >
                        <img src={ChevronDoubleRight} width={28} height={28} />
                    </button>
                </section>
            </div>
        </div>
    )
});
import { queryOptions } from "@tanstack/react-query";
import { getLocations, getLocationByName, getLocationFilter } from "../api/api";

export function locationsQueryOptions() {
    return queryOptions({
        queryKey: ["locations"],
        queryFn: () => getLocations()
    })
};

export function locationByNameQueryOptions(name: string) {
    return queryOptions({
        queryKey: ["location", name],
        queryFn: () => getLocationByName(name)
    })
};

export function locationFilterQueryOptions(
    query?: string,
    status?: string[], 
    hasQuest?: boolean,
    locationCategory?: string[],
    locationType?: string[], 
    parentLocation?: string[],
    inhabitants?: string[],
    
){
    return queryOptions({
        queryKey: ["locations", { query, status, hasQuest, locationCategory, locationType, parentLocation, inhabitants }],
        queryFn: () => getLocationFilter( query, status, hasQuest, locationCategory, locationType, parentLocation, inhabitants )
    })
};
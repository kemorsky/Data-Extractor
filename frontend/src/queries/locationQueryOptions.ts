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
    status?: string[], 
    hasQuest?: boolean,
    locationCategory?: string[],
    locationType?: string[], 
    parentLocation?: string[],
    inhabitants?: string[],
    
){
    return queryOptions({
        queryKey: ["locations", { status, hasQuest, locationCategory, locationType, parentLocation, inhabitants }],
        queryFn: () => getLocationFilter( status, hasQuest, locationCategory, locationType, parentLocation, inhabitants )
    })
};
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
    locationType?: string[], 
    parentLocation?: string[]
){
    return queryOptions({
        queryKey: ["locations", {status, locationType, parentLocation}],
        queryFn: () => getLocationFilter(status, locationType, parentLocation)
    })
};
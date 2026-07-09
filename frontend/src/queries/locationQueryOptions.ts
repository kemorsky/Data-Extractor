import { queryOptions } from "@tanstack/react-query";
import { getLocations, getLocationByName } from "../api/api";

export function locationsQueryOptions() {
    return queryOptions({
        queryKey: ["locations"],
        queryFn: () => getLocations()
    })
};

export function locationByNameQueryOptions(name: string) {
    return queryOptions({
        queryKey: ["location"],
        queryFn: () => getLocationByName(name)
    })
};
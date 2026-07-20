import { type LocationData } from '../utils/types';

type RequestOptions = {
        method?: string,
        headers?: { [key: string]: string },
        body?: string,
        credentials?: RequestCredentials;
    }

const URL = import.meta.env.VITE_URL;
// const URL = "http://localhost:5161";

const apiRequest = async (url: string, options: RequestOptions = {}) => {
    try {
        const response = await fetch(url, {
            method: "GET",
            // credentials: "include",
            headers: {
                "Content-Type": "application/json",
                ...options.headers
            },
            ...options
        })
        if (!response.ok) {
          let message = 'Request failed';
          let type = 'Error';
          try {
            const errorData = await response.json();
            
            type = errorData.type;
            message = errorData.message;
            
          } catch (error) {
            console.error(error)
          }
            throw Error(`${type}: ${message}`)
        };
        return await response.json();
    } catch (error) {
        console.error(error);
        throw error;
    }
}

export const getLocations = async (): Promise<LocationData[]> => {
    const data = await apiRequest(`${URL}/locations`);
    console.log(data);
    return data;
}

export const getLocationByName = async (name: string): Promise<LocationData> => {
    const data = await apiRequest(`${URL}/locations/${name}`);
    return data;
}

export const getLocationFilter = async (
    status?: string[], 
    //keywords?: string[],
    locationCategory?: string[],
    locationType?: string[], 
    parentLocation?: string[],
    inhabitants?: string[],
): Promise<LocationData[]> => 
    {
        const params = new URLSearchParams();

        if (status?.length) params.append("status", status.join(","));
        //if (keywords?.length) params.append("keywords", keywords.join(","));
        if (locationType?.length) params.append("locationType", locationType.join(","));
        if (locationCategory?.length) params.append("locationCategory", locationCategory.join(","));
        if (parentLocation?.length) params.append("parentLocation", parentLocation.join(","));
        if (inhabitants?.length) params.append("inhabitants", inhabitants.join(","));

        
        const data =  await apiRequest(`${URL}/locations/filter?${params.toString()}`);
        return data;
    };
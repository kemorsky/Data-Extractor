import { type LocationData } from '../utils/types';

type RequestOptions = {
        method?: string,
        headers?: { [key: string]: string },
        body?: string,
        credentials?: RequestCredentials;
    }

const URL = "http://localhost:5161";

const apiRequest = async (url: string, options: RequestOptions = {}) => {
    try {
        const response = await fetch(url, {
            method: "GET",
            credentials: "include",
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
            
            type = errorData.detail.type;
            message = errorData.detail.message;
            
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

export const getLocationByEsm = async (): Promise<LocationData[]> => {
    const data = await apiRequest(`${URL}/mod-data`);
    return data.uniqueLocations;
}


export const getLocation = async (): Promise<LocationData[]> => {
    const data = await apiRequest(`${URL}/test`);
    return data;
}

export const getLocationById = async (id: number): Promise<LocationData> => {
    const data = await apiRequest(`${URL}/test/${id}`);
    return data;
}
import { type LocationData } from '../utils/types';

type RequestOptions = {
        method?: string,
        headers?: { [key: string]: string },
        body?: string,
        credentials?: RequestCredentials;
    }

// const URL = "http://localhost:5161";
const URL = "https://kemorsky.github.io/Data-Extractor/";

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

export const getLocations = async (): Promise<LocationData[]> => {
    const data = await apiRequest(`${URL}/locations`);
    console.log(data);
    return data;
}

export const getLocationByName = async (name: string): Promise<LocationData> => {
    const data = await apiRequest(`${URL}/locations/${name}`);
    return data;
}
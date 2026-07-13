import { createBrowserRouter } from "react-router";
import App from "../App";
import LocationDataPage from "../pages/location-data";

export const router = createBrowserRouter([
    {
        path: "/locations",
        Component: App
    },
    {
        path: "/locations/:name",
        Component: LocationDataPage
    },
    {},
    ], 
    {    
        basename: import.meta.env.BASE_URL
    }
)
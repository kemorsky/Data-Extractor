import { createBrowserRouter } from "react-router";
import App from "../App";
import LocationDataPage from "../pages/location-page";

export const router = createBrowserRouter([
    {
        path: "/",
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
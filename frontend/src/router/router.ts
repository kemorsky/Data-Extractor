import { createBrowserRouter } from "react-router";
import App from "../App";
import LocationDataPage from "../pages/location-data";

export const router = createBrowserRouter([
    {
        path: "/",
        Component: App
    },
    {
        path: "/location/:id",
        Component: LocationDataPage
    },
    {},
])
import { createBrowserRouter } from "react-router";
import App from "../App";
// import LocationDataPage from "../pages/location-page";
import LocationDrawer from "../ui/components/drawer/drawer";
// import LocationRoute from "./location-route";

export const router = createBrowserRouter([
    {
        path: "/",
        Component: App,
        children: [
            {
                path: "locations/:name",
                Component: LocationDrawer,
            }
        ]
    },
    // {
    //     path: "locations/:name",
    //     Component: LocationRoute,
    // },
    // {
    //     path: "/locations/:name",
    //     Component: LocationDrawer,
    // },
    {},
    ], 
    {    
        basename: import.meta.env.BASE_URL
    }
)
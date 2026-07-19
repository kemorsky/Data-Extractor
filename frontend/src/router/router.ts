import { createBrowserRouter } from "react-router";
import App from "../App";
import LocationDrawer from "../ui/components/drawer/drawer";

export const router = createBrowserRouter([
    {
        path: "/",
        Component: App,
        children: [
            {
                path: "locations/:name",
                Component: LocationDrawer,
            },
        ]
    },
], 
    {    
        basename: import.meta.env.BASE_URL
    }
);
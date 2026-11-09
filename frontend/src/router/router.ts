import { createBrowserRouter } from "react-router";
import App from "../App";
import LocationDrawer from "../ui/components/drawer/drawer";
import RedirectPage from "../pages/redirect";

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
    {
        path: "/redirect",
        Component: RedirectPage,
    },
], 
    {    
        basename: import.meta.env.BASE_URL
    }
);
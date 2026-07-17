import { useLocation } from "react-router";
import LocationDrawer from "../ui/components/drawer/drawer";
import LocationDataPage from "../pages/location-page";

export default function LocationRoute() {
  const routerLocation = useLocation();

  const openedFromCard = routerLocation.state?.drawer === true;

  if (openedFromCard) {
    return <LocationDrawer />;
  }

  return <LocationDataPage />;
}
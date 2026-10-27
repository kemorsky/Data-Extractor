import { type LocationData } from "../utils/types"
import { MapContainer, ImageOverlay, Marker, Popup } from 'react-leaflet';
import L from 'leaflet';
import map from "../assets/map/Cyrodiil-Map-5055x3456.webp";
import Icons from "../ui/components/shared/icons";

import 'leaflet/dist/leaflet.css';
import { useNavigate, useLocation } from "react-router";

// import icon from 'leaflet/dist/images/marker-icon.png';
// import iconShadow from 'leaflet/dist/images/marker-shadow.png';
// import { useEffect, useState } from "react";

const getMarkerIcon = (locationType: string) => {
    const element = Icons({
        showText: false,
        text: locationType,
    });

    const children = element.props.children;
    const img = Array.isArray(children) ? children[0] : children;
    const iconUrl = img?.props?.src;

    if (!iconUrl) {
        return new L.Icon.Default();
    }

    return L.icon({
        iconUrl,
        iconSize: [24, 24],
        iconAnchor: [12, 18],
        popupAnchor: [1, -34],
    });
};

interface MapProps {
    locations: LocationData[] | undefined;
}

// Map dimensions
const IMG_WIDTH = 5055;
const IMG_HEIGHT = 3456;
const IMAGE_BOUNDS: L.LatLngBoundsExpression = [[0, 0], [IMG_HEIGHT, IMG_WIDTH]];

function gridToLeaflet(gridX: number, gridY: number): L.LatLngTuple {
    const CELL_SIZE_PX = 32;  // 5150 width / 156 cells

    const ORIGIN_LEAFLET_X = 2050;
    const ORIGIN_LEAFLET_Y = 1420;

    const finalLeafletX = ORIGIN_LEAFLET_X - (CELL_SIZE_PX / 2) + (gridX * CELL_SIZE_PX);
    const finalLeafletY = ORIGIN_LEAFLET_Y - (CELL_SIZE_PX / 2) + (gridY * CELL_SIZE_PX);

    return [finalLeafletY, finalLeafletX];
}

// const MapControls = ({ onZoomChange }: {
//     onZoomChange: (zoom: number) => void;
// }) => {
//     const mapInstance = useMap();

//     useMapEvents({
//         click(e) {
//             console.log(`Leaflet Y: ${e.latlng.lat}, Leaflet X: ${e.latlng.lng}`);
//         },
//         zoomend() {
//             onZoomChange(mapInstance.getZoom());
//         },
//     });

//     return null;
// };

export const Map = (props: MapProps) => {
    const { locations } = props;

    const navigate = useNavigate();
    const location = useLocation();

    function slugify(value: string) {
        return value
            .toLowerCase()
            .trim()
            .replace(/['']/g, "")
            .replace(/\s+/g, "-");
    }

    // const [zoom, setZoom] = useState(0);

    const handleClickName = (name: string) => {
        navigate(`/locations/${slugify(name)}`, {
            state: { 
                drawer: true,
                backgroundLocation: location.state?.backgroundLocation ?? location,
            },
        });
    };
    
    return (
        <div className="map" style={{ width: '100vw', height: '100vh', background: '#0b0b0b' }}>
            <MapContainer 
                crs={L.CRS.Simple}
                minZoom={-2}
                maxZoom={2}
                bounds={IMAGE_BOUNDS}
                maxBounds={IMAGE_BOUNDS}
                maxBoundsViscosity={1.0}
                style={{ width: '100%', height: '100%' }}
                
            >
                {/* <MapControls onZoomChange={setZoom} /> */}
                <ImageOverlay 
                    url={map}
                    bounds={IMAGE_BOUNDS}
                    attribution="The Elder Scrolls V: Skyrim © Bethesda Game Studios"
                />

                {locations?.map((loc) => {
                    const locationOrigin = loc.cells.find(l => l.gridX !== null && l.gridY !== null);

                    if (!locationOrigin) {
                        return null;
                    };
                    
                    const position = gridToLeaflet(locationOrigin.gridX, locationOrigin.gridY);
                    // const size = 24 + zoom * 4;

                    const baseIcon = getMarkerIcon(loc.locationType ?? "");

                    if (loc.cells[1]?.gridX !== null && loc.cells[1]?.gridY !== null) {
                        
                        return (
                            <Marker key={loc.id} position={position} icon={baseIcon}>
                                <Popup>
                                    <div className="map-popup">
                                        <h3 style={{ margin: '0 0 5px 0' }}>{loc.name}</h3>
                                        <p style={{ margin: 0, color: '#666' }}>{loc.parentLocation}</p>
                                        <small style={{ color: '#999' }}>
                                            Cell: {locationOrigin.editorID} <br></br>
                                            Grid: ({locationOrigin.gridX}, {locationOrigin.gridY})
                                        </small>
                                        <button onClick={() => {handleClickName(loc.name)}}>View</button>
                                    </div>
                                </Popup>
                            </Marker>
                        )
                    } else {
                        return null;
                    };
                })}
            </MapContainer>
        </div>
    )
}
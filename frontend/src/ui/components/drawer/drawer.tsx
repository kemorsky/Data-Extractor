import { useMemo, useState } from 'react';
import { useLocation, useNavigate } from 'react-router'
import { Drawer } from "@base-ui/react/drawer";
import styles from './drawer.module.css';
import { useQuery } from "@tanstack/react-query";
import { useParams } from "react-router";
import Status from '../shared/status';
import { locationByNameQueryOptions, locationsQueryOptions } from "../../../queries/locationQueryOptions";
import type { LocationData } from '../../../utils/types';
import X from "../../../assets/icons/cross.svg"
import parse from 'html-react-parser';
import DrawerCityMap from '../shared/drawer-city-map';
import Icons from "../shared/icons";
import { ShowMoreButton } from '../shared/buttons';
import { FiExternalLink } from "react-icons/fi";

export default function LocationDrawer() {
    const navigate = useNavigate();
    const location = useLocation();

    const { name } = useParams();

    const { data: locations } = useQuery(locationsQueryOptions());
    const { data: locationByName } = useQuery({
        ...locationByNameQueryOptions(name ?? ""),
        enabled: !!name,
    });

    console.log(locationByName);

    const [ isOpen, setIsOpen ] = useState(false);
    const [ showChildren, setShowChildren ] = useState(false);
    const [ showCells, setShowCells ] = useState(false);
    const [ showNpcs, setShowNpcs ] = useState(false);

    const visibleCells = showCells
        ? locationByName?.cells
        : locationByName?.cells.slice(0, 3);

    const visibleNpcs = showNpcs
        ? locationByName?.inhabitingNpcs
        : locationByName?.inhabitingNpcs.slice(0, 3);

    const rawNotes = locationByName?.notes && locationByName?.notes !== "None" 
        ? locationByName?.notes
        : "<p>No description available.</p>";

    const childrenByParent = useMemo(() => {
        const map = new Map<string, LocationData[]>();
    
        for (const loc of locations ?? []) {
            if (!loc.parentLocation) continue;

            if (!map.has(loc.parentLocation)) {
                map.set(loc.parentLocation, []);
            }

            map.get(loc.parentLocation)!.push(loc);
        };

        return map;
    }, [locations]);
    
    const children = childrenByParent.get(locationByName?.parentLocation ?? "") ?? [];

    const parentLocationsCities = [
        ...new Set(
            (locations ?? [])
                .filter(location => 
                    location.keywords.includes("LocTypeCity")
                )
                .map(location => location.name)
        ),
    ].sort();

    const handleClick = () => {
        console.log(`Parent Location: ${locationByName?.parentLocation}`, children);
        setShowChildren(!showChildren)
    };

    const backgroundLocation = location.state?.backgroundLocation;

    const handleCloseDrawer = () => {
        setShowChildren(false);
        navigate(backgroundLocation ?? "/");
    };
    
    return (
        <Drawer.Root swipeDirection="right" open={!!name} onOpenChange={(open) => {
            if (!open) {
                handleCloseDrawer();
            }
        }}>
            <Drawer.Portal>         
                <Drawer.Backdrop className={styles.Backdrop} />         
                <Drawer.Viewport className={styles.Viewport}>           
                    <Drawer.Popup className={styles.Popup}>             
                        <Drawer.Content className={styles.Content}>               
                            <article className={styles.Header}>
                                <div className={styles.Status}>
                                    {locationByName?.status !== "None" &&
                                        <Status text={locationByName?.status ?? ""} />
                                    }
                                </div>

                                <article className={styles.Title}>
                                    <Icons width={24} height={24} showText={false} text={locationByName?.locationType ?? ""} />
                                    <h3 className={styles.TitleText}>{locationByName?.name}</h3>
                                </article>
                                
                                <section className={styles.LocationContainer}>
                                    <Icons width={24} height={24} showText={false} text={locationByName?.parentLocation ?? ""} />
                                    
                                    <span 
                                        className={styles.LocationText} 
                                        onClick={() => { handleClick() }} 
                                        style={{ minWidth: "2rem", cursor: "pointer", position: "relative" }}
                                    >
                                        
                                        {locationByName?.parentLocation} *
                                        {locationByName?.region !== "None" && `, ${locationByName?.region}`}                                                                                             
                                    </span>
                                    
                                    <section className={styles.ParentLocationChildren} style={{display: showChildren ? "flex" : "none"}}>
                                        {children.map((child) => (
                                            <span key={child.id}>{child.name}</span>
                                        ))}
                                    </section>

                                    {parentLocationsCities.includes(locationByName?.parentLocation ?? "") && 
                                        <>
                                            <button className={styles.ShowMapButton} onClick={() => setIsOpen(true)}>Show map</button>
                                            <DrawerCityMap 
                                                text={locationByName?.parentLocation ?? ""} 
                                                showText={false} 
                                                isOpen={isOpen}
                                                onClose={() => setIsOpen(false)}
                                            />
                                        </>
                                    }
                                </section>
                                 
                                <section className={styles.ListKeywords}>
                                    {locationByName?.keywords.map((keyword) => (
                                        <span key={keyword} className={styles.ListKeyword}>
                                            {keyword}
                                        </span>))}
                                </section>
                            </article>

                            <div className={styles.Actions}>                 
                                <Drawer.Close 
                                    className={styles.Button}
                                    onClick={() => { setShowCells(false); setShowNpcs(false); }} 
                                >
                                    <img src={X} width={22} height={22} />
                                </Drawer.Close>               
                            </div>

                            <ul className={styles.ListContainer}>
                                <li className={styles.LocationType}>
                                    <span className={styles.LocationText}>Type</span>
                                    <span className={styles.ListItemText}>{locationByName?.locationType}, {locationByName?.locationCategory}</span>
                                </li>
                                
                                <div className={styles.ListsContainer}>     
                                    <section className={styles.List} style={{ flexDirection: "column"}}>
                                        <span className={styles.ListText}>Cells</span>
                                        <ul className={styles.ListArray}>
                                            {visibleCells?.map((cell) => (
                                                <li key={cell.id} className={styles.ListArrayItem}>
                                                    <span className={styles.ListItemText}>
                                                        {cell.editorID} 
                                                        {cell.gridX !== null && cell.gridY !== null &&
                                                            ` (${cell.gridX}, ${cell.gridY})`
                                                        }
                                                    </span>
                                                </li>))
                                            }
                                            {(locationByName?.cells?.length ?? 0) > 3 &&
                                                <ShowMoreButton 
                                                    showState={showCells} 
                                                    setShowState={setShowCells} 
                                                    textTrue={"Show less"}
                                                    textFalse={"Show more"}
                                                />
                                            }
                                        </ul>
                                    </section>
                                    {(locationByName?.inhabitingNpcs?.length ?? 0) > 0 && 
                                        <section className={styles.List} style={{ flexDirection: "column"}}>
                                            <span className={styles.ListText}>Inhabitants</span>
                                            <ul className={styles.ListArray}>
                                                {visibleNpcs?.map((npc) => (
                                                    <li key={npc.name} className={styles.ListArrayItem}>
                                                        <span className={styles.ListItemText}>
                                                            {npc.url !== "" ? (
                                                                <a target="_blank" style={{ fontWeight: 600 }} href={npc.url}>
                                                                    {npc.name}
                                                                </a>
                                                                ) : (
                                                                    <span style={{ fontWeight: 600 }}>
                                                                        {npc.name}
                                                                    </span>
                                                                )
                                                            }
                                                        </span>
                                                    </li>
                                                ))
                                                }
                                            
                                                {(locationByName?.inhabitingNpcs?.length ?? 0) > 3 &&
                                                        <ShowMoreButton 
                                                            showState={showNpcs} 
                                                            setShowState={setShowNpcs} 
                                                            textTrue={"Show less"}
                                                            textFalse={"Show more"}
                                                        />
                                                    }
                                            </ul>
                                        </section>
                                    }
                                    {/* {(locationByName?.inhabitants?.length ?? 0) > 0 && 
                                        <li className={styles.List}>
                                            <span className={styles.ListText}>Inhabitants</span>
                                            <ul className={styles.ListArray}>
                                                {locationByName?.inhabitants.map((inhabitant) => (
                                                    <li key={inhabitant} className={styles.ListArrayItem}>
                                                        <span className={styles.ListItemText}>{inhabitant}</span>
                                                    </li>))}
                                            </ul>
                                        </li>
                                    } */}
                                </div>
                                {locationByName?.relatedQuestName !== "None" ? 
                                    (   <li className={styles.ListItem}>
                                            <span className={styles.ListItemText}>Quest:</span>                                                                                    
                                            <a target="_blank" className={styles.ListItemLink} href={locationByName?.relatedQuestUrl}>
                                                {locationByName?.relatedQuestName}
                                                <FiExternalLink className={styles.ListItemLink} size={21}/>
                                            </a>
                                            
                                        </li>
                                    ) :
                                    (   <li className={styles.ListItem}>
                                            <span className={styles.ListItemText}>Quest Link:</span> 
                                            <span className={styles.ListItemText} style={{ fontStyle: "italic" }}>
                                                No quest matched
                                            </span>
                                        </li>
                                    )
                                }
                                {locationByName?.vikunjaLink !== "" ?
                                    (   <li className={styles.ListItem}>
                                            <span className={styles.ListItemText}>Vikunja Card:</span>                                 
                                            <a className={styles.ListItemLink} target="_blank" style={{ fontWeight: 600 }} href={locationByName?.vikunjaLink}>
                                                    <FiExternalLink className={styles.ListItemLinkIcon} size={21}/>
                                            </a>                              
                                        </li>
                                    ) : 
                                    (
                                        <li className={styles.ListItem} style={{ fontStyle: "italic" }}>
                                            <span className={styles.ListItemText}>Vikunja Link:</span>
                                            <span className={styles.ListItemText}>
                                                No claim card matched
                                            </span>
                                        </li>
                                    ) 

                                }
                            </ul>

                            <div className={styles.NotesContainer}>
                                <section className={styles.Notes}>
                                    <h2 className={styles.NotesTitle}>Notes</h2>
                                    <hr className={styles.NotesSeparator} />
                                    <article className={styles.NotesText}>
                                        {parse(rawNotes)}
                                    </article>
                                </section>
                            </div>   

                        </Drawer.Content>           
                    </Drawer.Popup>         
                </Drawer.Viewport>       
            </Drawer.Portal>     
        </Drawer.Root>
    )
};
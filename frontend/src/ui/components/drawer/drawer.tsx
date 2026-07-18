import { useNavigate } from 'react-router'
import { Drawer } from "@base-ui/react/drawer";
import styles from './drawer.module.css';
import { useQuery } from "@tanstack/react-query";
import { useParams } from "react-router";
import { locationByNameQueryOptions, locationsQueryOptions } from "../../../queries/locationQueryOptions";
import type { LocationData } from '../../../utils/types';

export default function LocationDrawer() {
    const navigate = useNavigate();
    const { name } = useParams();

    const { data: locations } = useQuery(locationsQueryOptions());
    const { data: locationByName } = useQuery({
        ...locationByNameQueryOptions(name ?? ""),
        enabled: !!name,
    });

    const childrenByParent = new Map<string, LocationData[]>();
        
        for (const loc of locations ?? []) {
            if (!loc.parentLocation) continue;
    
            if (!childrenByParent.has(loc.parentLocation)) {
                childrenByParent.set(loc.parentLocation, []);
            }
    
                childrenByParent.get(loc.parentLocation)!.push(loc);
        };
    
        const children = childrenByParent.get(locationByName?.parentLocation ?? "") ?? [];

    const handleClick = () => {
        console.log(`Parent Location: ${locationByName?.parentLocation}`, children);
    }
    
    return (
        <Drawer.Root swipeDirection="right" open={!!name} onOpenChange={(open) => {
        if (!open) {
          navigate("/");
        }
      }}>
            {/* <Drawer.Trigger className={styles.Button}>Open drawer</Drawer.Trigger>        */}
            <Drawer.Portal>         
                <Drawer.Backdrop className={styles.Backdrop} />         
                <Drawer.Viewport className={styles.Viewport}>           
                    <Drawer.Popup className={styles.Popup}>             
                        <Drawer.Content className={styles.Content}>               
                            <article className={styles.Title}>
                                <div className={styles.Actions}>                 
                                    <Drawer.Close className={styles.Button}>Close</Drawer.Close>               
                                </div> 
                                <h3 className={styles.TitleText}>{locationByName?.name}</h3>
                                <article className={styles.Status}>
                                    <span>{locationByName?.status}</span>
                                </article>
                            </article>
                            <ul className={styles.List}>
                                <li onClick={() => {handleClick();}} className={styles.ListItem}>
                                    <span className={styles.ListItemText}>Location:</span> 
                                    <span className={styles.ListItemText}>{locationByName?.parentLocation}, {locationByName?.region}</span>
                                </li>
                                <li className={styles.ListItem}>
                                    <span className={styles.ListItemText}>Type:</span> 
                                    <span className={styles.ListItemText}>{locationByName?.locationCategory}, {locationByName?.locationType}</span>
                                </li>
                                <li className={styles.ListItem}>
                                    <span className={styles.ListItemText}>Inhabitants:</span> 
                                    <span className={styles.ListItemText}>{locationByName?.inhabitants}</span>
                                </li>
                                <li className={styles.ListItem}>
                                    <span className={styles.ListItemText}>Quest Links:</span> 
                                    <span className={styles.ListItemText}>
                                        <a target="_blank" style={{fontWeight: 600}} href={locationByName?.relatedQuestUrl}>
                                            {locationByName?.relatedQuestName}
                                        </a>
                                    </span>
                                </li>
                                <li className={styles.ListItem}>
                                    <span className={styles.ListItemText}>Keywords: </span>
                                    <ul className={styles.ListKeywords}>
                                        {locationByName?.keywords.map((keyword) => (
                                            <li key={keyword} className={styles.ListKeywordsItem}>
                                                <span className={styles.ListItemText}>{keyword}</span>
                                            </li>))}
                                    </ul>
                                </li>
                                {/* <li className="location-header__list-item">
                                    Vikunja Links: <strong>
                                        <a target="_blank" href={location.vikunjaLink[0]}>{location.vikunjaLink[0]}</a>
                                        </strong>
                                </li> */}
                            </ul>
                            <div className={styles.NotesContainer}>
                                <section className={styles.Notes}>
                                    <h2 className={styles.NotesTitle}>Notes</h2>
                                    <hr className={styles.NotesSeparator} />
                                    <p className={styles.NotesText}>{locationByName?.notes}</p>
                                </section>

                            </div>                              
                        </Drawer.Content>           
                    </Drawer.Popup>         
                </Drawer.Viewport>       
            </Drawer.Portal>     
        </Drawer.Root>
    )
}
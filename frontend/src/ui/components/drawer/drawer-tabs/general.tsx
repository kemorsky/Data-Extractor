import styles from '../drawer.module.css';
import{ useState } from 'react';
import parse from 'html-react-parser';
import type { LocationData } from '../../../../utils/types';
import { ShowMoreButton } from '../../shared/buttons';
import { FiExternalLink } from "react-icons/fi";

interface DrawerTabProps {
    locationByName: LocationData | undefined;
}

export default function DrawerGeneralTab(props: DrawerTabProps) {
    const { locationByName } = props;

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

    return (
        <div className={styles.Content}>
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
        </div>
    )
};
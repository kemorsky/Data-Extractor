import { useMemo, useState } from 'react';
import { useLocation, useNavigate } from 'react-router'
import { Drawer } from "@base-ui/react/drawer";
import styles from './drawer.module.css';
import tabsStyles from './drawer-tabs.module.css';
import { useQuery } from "@tanstack/react-query";
import { useParams } from "react-router";
import Status from '../shared/status';
import { locationByNameQueryOptions, locationsQueryOptions } from "../../../queries/locationQueryOptions";
import type { LocationData } from '../../../utils/types';
import X from "../../../assets/icons/cross.svg"
import DrawerCityMap from '../shared/drawer-city-map';
import Icons from "../shared/icons";
import { Tabs } from '@base-ui/react/tabs';
import DrawerGeneralTab from './drawer-tabs/general';

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
                                    // onClick={() => { setShowCells(false); setShowNpcs(false); }} 
                                >
                                    <img src={X} width={22} height={22} />
                                </Drawer.Close>               
                            </div>

                            <Tabs.Root className={tabsStyles.Root} defaultValue="overview">
                                <Tabs.List className={tabsStyles.List}>
                                    <Tabs.Tab className={tabsStyles.Tab} value="general">
                                        General
                                        <div className={tabsStyles.Highlight} />
                                    </Tabs.Tab>
                                    <Tabs.Tab className={tabsStyles.Tab} value="implementation">
                                        Implementation
                                        <div className={tabsStyles.Highlight} />
                                    </Tabs.Tab>
                                    <Tabs.Tab className={tabsStyles.Tab} value="betacomments">
                                        Beta Comments
                                        <div className={tabsStyles.Highlight} />
                                    </Tabs.Tab>
                                    {parentLocationsCities.includes(locationByName?.parentLocation ?? "") && 
                                        <Tabs.Tab className={tabsStyles.Tab} value="parent-map">
                                            Parent Map
                                            <div className={tabsStyles.Highlight} />
                                        </Tabs.Tab>
                                    }
                                    <Tabs.Indicator className={tabsStyles.Indicator} />
                                </Tabs.List>
                                
                                {/* Tabs content render window */}
                                <div className={tabsStyles.PanelViewport}>
                                    <Tabs.Panel key={"general"} className={tabsStyles.Panel} value="general">
                                        <DrawerGeneralTab locationByName={locationByName} />
                                    </Tabs.Panel>
                                    <Tabs.Panel key={"implementation"} className={tabsStyles.Panel} value="implementation">
                                        <span>Implementation</span>
                                    </Tabs.Panel>
                                    <Tabs.Panel key={"betacomments"} className={tabsStyles.Panel} value="betacomments">
                                        <span>Beta Comments</span>
                                    </Tabs.Panel>
                                    <Tabs.Panel key={"parent-map"} className={tabsStyles.Panel} value="parent-map">
                                        
                                        <button className={styles.ShowMapButton} onClick={() => setIsOpen(true)}>Show map</button>
                                        <DrawerCityMap 
                                            text={locationByName?.parentLocation ?? ""} 
                                            showText={false} 
                                            isOpen={isOpen}
                                            onClose={() => setIsOpen(false)}
                                        />
                                           
                                    </Tabs.Panel>
                                </div>
                                </Tabs.Root>

                        </Drawer.Content>           
                    </Drawer.Popup>         
                </Drawer.Viewport>       
            </Drawer.Portal>     
        </Drawer.Root>
    )
};
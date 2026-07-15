import { useParams } from "react-router";
import './location-page.css';
import { useQuery } from '@tanstack/react-query';
import { locationByNameQueryOptions, locationsQueryOptions } from '../queries/locationQueryOptions';
import type { LocationData } from "../utils/types";

export default function LocationDataPage() {
    const { name } = useParams();
    const { data: locations } = useQuery(locationsQueryOptions());
    const { data: locationByName } = useQuery(locationByNameQueryOptions(name ?? ""));

    console.log(locationByName?.image);

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
        <main className="location-page">

            <header className="location-page-header">
                <section className="location-header__last-edited">
                    {/* <img src={locationByName?.image} alt={`${locationByName?.name} image`} width="400" height="250" /> */}
                </section>
                <section className="location-page-header__core-info">
                    <h1 className="location-page-header__core-info__name">{locationByName?.name}</h1>
                    <article className="location-page-header__status">
                        <span>{locationByName?.status}</span>
                    </article>
                </section>
                <ul className="location-page-header__list">
                    <li onClick={() => {handleClick();}} className="location-page-header__list-item">
                        <span className="location-page-header__list__text">Parent Location:</span> 
                        <span className="location-page-header__list__text">{locationByName?.parentLocation}, {locationByName?.region}</span>
                    </li>
                    <li className="location-page-header__list-item">
                        <span className="location-page-header__list__text">Type:</span> 
                        <span className="location-page-header__list__text">{locationByName?.locationType}</span>
                    </li>
                    <li className="location-page-header__list-item">
                        <span className="location-page-header__list__text">Inhabitants:</span> 
                        <span className="location-page-header__list__text">{locationByName?.inhabitants}</span>
                    </li>
                    <li className="location-page-header__list-item">
                        <span className="location-page-header__list__text">Quest Links:</span> 
                        <span className="location-page-header__list__text">
                            <a target="_blank" href={locationByName?.relatedQuestUrl}>
                                {locationByName?.relatedQuestName}
                            </a>
                        </span>
                    </li>
                    <li className="location-page-header__list-item">
                        <span className="location-page-header__list__text">Keywords: </span>
                        <ul style={{marginTop: 0,
            marginBottom: 0,
            paddingLeft: 0,
            }}>
                            {locationByName?.keywords.map((keyword) => (
                                <li key={keyword} className="location-page-header__list-item">
                                    <span className="location-page-header__list__text">{keyword}</span>
                                </li>))}
                        </ul>
                    </li>
                    {/* <li className="location-header__list-item">
                        Vikunja Links: <strong>
                            <a target="_blank" href={location.vikunjaLink[0]}>{location.vikunjaLink[0]}</a>
                            </strong>
                    </li> */}
                </ul>
            </header>

            <div className="location-page-content">
                <section className="location-page-content__notes">
                    <h2>Notes</h2>
                    <hr className="location-page-content__separator" />
                    <p >{locationByName?.notes}</p>
                </section>

            </div>
        </main>
    )
}
import { useParams } from "react-router";
import './location-page.css';
import { useQuery } from '@tanstack/react-query';
import { locationByNameQueryOptions } from '../queries/locationQueryOptions';

export default function LocationDataPage() {
    const { name } = useParams();
    const { data: locationByName } = useQuery(locationByNameQueryOptions(name ?? ""));

    console.log(locationByName?.image);

    return (
        <main className="location-page">

            <header className="location-page-header">
                <section className="location-header__last-edited">
                    {/* <img src={locationByName?.image} alt={`${locationByName?.name} image`} width="400" height="250" /> */}
                </section>
                <section className="location-page-header__core-info">
                    <h1>{locationByName?.name}</h1>
                    <article className="location-page-header__status">
                        <span>{locationByName?.status}</span>
                    </article>
                </section>
                <ul className="location-page-header__list">
                    <li className="location-page-header__list-item">Parent Location: {locationByName?.parentLocation}</li>
                    {/* <li className="location-content__list-item">
                        Location: <strong>{location.locationOnMap}</strong>
                    </li> */}
                    <li className="location-page-header__list-item">
                        Type: <strong>{locationByName?.locationType}</strong>
                    </li>
                    {/* <li className="location-header__list-item">
                        Enemies: <strong>{location.enemies}</strong>
                    </li> */}
                    <li className="location-page-header__list-item">
                        Quest Links: <strong><a target="_blank" href={locationByName?.relatedQuestUrl}>{locationByName?.relatedQuestName}</a></strong>
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
                    <p >{locationByName?.notes}</p>
                </section>

            </div>
        </main>
    )
}
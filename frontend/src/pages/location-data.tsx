import { useParams } from "react-router";
import './Pages.css';
import { useQuery } from '@tanstack/react-query';
import { locationByNameQueryOptions } from '../queries/locationQueryOptions';

export default function LocationDataPage() {
    const { name } = useParams();
    const { data: locationByName } = useQuery(locationByNameQueryOptions(name ?? ""));

    console.log(locationByName?.image);

    return (
        <main>

            <header className="location-header">
                <section className="location-header__last-edited">
                    <article className="location-header__last-edited__date">
                        <p>Last Edited:</p>
                        {/* <p>{location.lastEdited}</p> */}
                    </article>
                    <article className="location-header__last-edited__edited-by">
                        <p>By:</p>
                        {/* <p>{location.editedBy}</p> */}
                    </article>
                    <img src={locationByName?.image} alt={`${locationByName?.name} image`} width="400" height="250" />
                </section>
                <section className="location-header__core-info">
                    <h1>{locationByName?.name}</h1>
                    <span>{locationByName?.parentLocation}</span>
                </section>

                <article className="location-header__status">
                    <span>{locationByName?.status}</span>
                </article>
            </header>

            <div className="location-content">
                <ul className="location-content__list">
                    {/* <li className="location-content__list-item">
                        Location: <strong>{location.locationOnMap}</strong>
                    </li> */}
                    <li className="location-content__list-item">
                        Type: <strong>{locationByName?.locationType}</strong>
                    </li>
                    {/* <li className="location-content__list-item">
                        Enemies: <strong>{location.enemies}</strong>
                    </li> */}
                    <li className="location-content__list-item">
                        Quest Links: <strong><a target="_blank" href={locationByName?.relatedQuestUrl}>{locationByName?.relatedQuestName}</a></strong>
                    </li>
                    {/* <li className="location-content__list-item">
                        Vikunja Links: <strong>
                            <a target="_blank" href={location.vikunjaLink[0]}>{location.vikunjaLink[0]}</a>
                            </strong>
                    </li> */}
                </ul>

                <section className="location-content__notes">
                    <h2>Notes</h2>
                    <p >{locationByName?.notes}</p>
                </section>
                <section className="location-content__writing-ld-wishlist">
                    <h2>Level Design Wishlist</h2>
                    {/* <ul>
                        {location.wishlist.map((item, index) => <li key={index}>{item}</li>)}
                    </ul> */}
                </section>
            </div>
        </main>
    )
}
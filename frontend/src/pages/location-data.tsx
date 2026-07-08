import { useState } from 'react';
import './Pages.css';
import type { LocationData } from '../utils/types';
import { getLocationByName } from '../api/api';

export default function LocationDataPage() {
    const [ location, setLocation ] = useState<LocationData>();

    const handleClick = async (name: string): Promise<LocationData> => {
        console.log("clicked");

        const data = await getLocationByName(name);
        // console.log(data[67]);
        setLocation(data);
        return data;
        console.log("finished");
        console.log(location);
    }

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
                </section>

                <button onClick={() => {handleClick(location?.name ?? "")}}>Test data fetch</button>

                <section className="location-header__core-info">
                    <h1>{location?.name}</h1>
                    <span>{location?.parentLocation}</span>
                </section>

                <article className="location-header__status">
                    <span>{location?.status}</span>
                </article>
            </header>

            <div className="location-content">
                <ul className="location-content__list">
                    {/* <li className="location-content__list-item">
                        Location: <strong>{location.locationOnMap}</strong>
                    </li> */}
                    <li className="location-content__list-item">
                        Type: <strong>{location?.type}</strong>
                    </li>
                    {/* <li className="location-content__list-item">
                        Enemies: <strong>{location.enemies}</strong>
                    </li> */}
                    <li className="location-content__list-item">
                        Quest Links: <strong><a target="_blank" href={location?.relatedQuestUrl}>{location?.relatedQuestName}</a></strong>
                    </li>
                    {/* <li className="location-content__list-item">
                        Vikunja Links: <strong>
                            <a target="_blank" href={location.vikunjaLink[0]}>{location.vikunjaLink[0]}</a>
                            </strong>
                    </li> */}
                </ul>

                <section className="location-content__description">
                    <h2>Description</h2>
                    {/* <p >{location.description}</p> */}
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
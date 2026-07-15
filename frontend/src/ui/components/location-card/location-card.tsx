import './location-card.css'
import type { LocationData } from '../../../utils/types';

interface LocationCardProps {
    location: LocationData;
    childrenByParent: Map<string, LocationData[]>;
    handleClickName: (name: string) => void;
};

export default function LocationCard(props: LocationCardProps) {
    return (
        <div 
            key={props.location.id} 
            onClick={() => {props.handleClickName(props.location.name ?? "")}}
            className="location-card"
        >
            <section className="location-card__labels">
            <p>{props.location.locationType}</p>
            <p>{props.location.status}</p>
            </section>

            <section className="location-card__info">
            <p className="location-card__info__name">{props.location.name}</p>
            <p className="location-card__info__parentLocation">
                {props.location.parentLocation}
            </p>
            </section>
        </div>
    )
};
import './location-card.css'
import type { LocationData } from '../../../utils/types';
import ayleidImage from "../../../assets/ayleid-ruin.webp"
import Fort from "../../../assets/Fort.svg"
import Quest from "../../../assets/Quest-Door.svg"
import Status from "../shared/status";
import Icons from "../shared/icons";

interface LocationCardProps {
    location: LocationData;
    childrenByParent: Map<string, LocationData[]>;
    handleClickName: (name: string) => void;
};

export default function LocationCard(props: LocationCardProps) {
    
    return (
        <div tabIndex={0} key={props.location.id} 
            className="location-card"
            onClick={() => {props.handleClickName(props.location.name ?? "")}} 
            onKeyDown={(e) => {
                if (e.key === "Enter") {
                    {props.handleClickName(props.location.name ?? "")}
                }
            }}
        >
            <img className="location-card__image" src={ayleidImage} width="100%"/>
            <section className="location-card__labels">
                {/* <p>{props.location.locationType}</p> */}
                <section className="location-card__labels-icons">
                    <Icons showText={false} text={props.location.locationType} />
                    {props.location.relatedQuestName !== "None" &&
                        <a target="_blank" href={props.location.relatedQuestUrl} className="location-card__quest">
                            <img src={Quest} alt="Quest anchor icon" className="location-card__quest__icon" />
                        </a>
                    }
                </section>
                {props.location.status !== "None" &&
                    <Status text={props.location.status} />
                }
                {/* <p className="location-card__labels__status">{props.location.status}</p> */}
            </section>
            <section className="location-card__content">
                <section className="location-card__info">
                    <p className="location-card__info__name">{props.location.name}</p>
                    <p className="location-card__info__parentLocation">
                        {props.location.parentLocation}
                    </p>
                </section>
                {/* <section className="location-card__labels">
                    <p>{props.location.locationType}</p>
                    <p className="location-card__labels__status">{props.location.status}</p>
                </section> */}
            </section>
        </div>
    )
};
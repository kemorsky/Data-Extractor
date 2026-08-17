import './location-card.css'
import type { LocationData } from '../../../utils/types';
import Quest from "../../../assets/location-icons/Quest-Door.svg"
import Vikunja from "../../../assets/icons/vikunja.svg"
import Status from "../shared/status";
import Icons from "../shared/icons";
import LocationCardBackground from "../shared/location-card-background";

interface LocationCardProps {
    location: LocationData;
    handleClickName: (name: string) => void;
    isTable: boolean;
};

export default function LocationCard(props: LocationCardProps) {
    const { location, handleClickName, isTable } = props;
    
    return (
        <>
        {isTable ? (
            <div tabIndex={0} key={location.id} 
                className="table__location-card"
                onClick={() => {handleClickName(location.name ?? "")}} 
                onKeyDown={(e) => {
                    if (e.key === "Enter") {
                        {handleClickName(location.name ?? "")}
                    }
                }}
            >
                {/* <LocationCardBackground width="100%" height="100%" text={location.locationType} showText={false} /> */}
                
                
                <section className="table__location-card__cell">
                    <p className="table__location-card__info__name">
                        <Icons width={24} height={24} showText={false} text={location.locationType} />
                        {location.locationType}
                    </p>
                </section>
                <section className="table__location-card__cell">
                    <p className="table__location-card__info__name">
                        {location.name}
                    </p>
                </section>

                <section className="table__location-card__cell">
                    <p className="table__location-card__info__parentLocation">
                        <Icons width={24} height={24} showText={false} text={location.parentLocation} />
                        {location.parentLocation}
                    </p>
                </section>
                
                <section className="table__location-card__cell">
                    <div className="table__location-card__labels-status">
                        {location.status !== "None" &&
                            <Status text={location.status} />
                        }
                    </div>
                </section>

                <section className="table__location-card__cell">
                    <section className="table__location-card__labels-icons">
                        {/* <Icons showText={false} text={location.locationType} /> */}
                        
                        {location.vikunjaLink !== "" && 
                            <a target="_blank" href={location.vikunjaLink} className="table__location-card__vikunja">
                                <img src={Vikunja} width={24} alt="Vikunja anchor icon" className="table__location-card__vikunja__icon" />
                            </a>
                        }
                        
                    </section>
                </section>

                <section className="table__location-card__cell">
                    <section className="table__location-card__labels-icons">
                        {/* <Icons showText={false} text={location.locationType} /> */}
                        {location.relatedQuestName !== "None" &&
                            <a target="_blank" href={location.relatedQuestUrl} className="table__location-card__quest">
                                <img src={Quest} alt="Quest anchor icon" className="table__location-card__quest__icon" />
                            </a>
                        }
                    </section>
                </section>
                        
                    {/* <section className="location-card__labels">
                        <p>{location.locationType}</p>
                        <p className="location-card__labels__status">{location.status}</p>
                    </section> 
                </section> */}
            </div>
            
        ) : (
            <div tabIndex={0} key={location.id} 
                className="location-card"
                onClick={() => {handleClickName(location.name ?? "")}} 
                onKeyDown={(e) => {
                    if (e.key === "Enter") {
                        {handleClickName(location.name ?? "")}
                    }
                }}
            >
                <LocationCardBackground width="100%" height="100%" text={location.locationType} showText={false} />
                {/* <img className="location-card__image" src={ayleidImage} width="100%" height="100%"/> */}
                <section className="location-card__labels">
                    {location.relatedQuestName !== "None" &&
                        <section className="location-card__labels-icons">
                            {/* <Icons showText={false} text={location.locationType} /> */}
                            
                                <a target="_blank" href={location.relatedQuestUrl} className="location-card__quest">
                                    <img src={Quest} width={18} alt="Quest anchor icon" className="location-card__quest__icon" />
                                </a>
                        </section>
                    }                        
                    <div className="location-card__labels-status">
                        {location.status !== "None" &&
                            <Status text={location.status} />
                        }
                    </div>
                </section>
                <section className="location-card__content">
                    <section className="location-card__info">
                        <p className="location-card__info__name">
                            <Icons width={24} height={24} showText={false} text={location.locationType} />
                            {location.name}</p>
                        <p className="location-card__info__parentLocation">
                            <Icons width={24} height={24} showText={false} text={location.parentLocation} />
                            {location.parentLocation}
                        </p>
                    </section>
                    {/* <section className="location-card__labels">
                        <p>{location.locationType}</p>
                        <p className="location-card__labels__status">{location.status}</p>
                    </section> */}
                </section>
            </div>
        )}
        </>
    )
};
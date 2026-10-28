import "./betacomments.css";
import type { LocationData } from "../../../../utils/types";

interface DrawerTabProps {
    locationByName: LocationData | undefined;
}

export default function DrawerBetaCommentsTab(props: DrawerTabProps) {
    const { locationByName } = props;

    return (
        <div className="betacomments-container">
            <header className="betacomments-container__header">
                <span className="betacomments-container__header__cell">Cell</span>
                <span className="betacomments-container__header__cell">FormKey</span>
                <span className="betacomments-container__header__cell">Raw Reference</span>
                <span className="betacomments-container__header__cell">Comment</span>
            </header>
        
            {locationByName?.betacomments.map((betacomment) => (
                <article key={betacomment.id} className="betacomment">
                    <span className="betacomment__cell">{betacomment.cell}</span>
                    <span className="betacomment__cell">{betacomment.referenceFormKey}</span>
                    <span className="betacomment__cell">{betacomment.rawReference}</span>
                    <span className="betacomment__cell">{betacomment.comment}</span>
                </article>
            ))}
        </div>
    )
};
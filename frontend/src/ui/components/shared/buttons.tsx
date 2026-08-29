import "./buttons.css";

import ChevronUp from "../../../assets/icons/chevron-up.svg";
import ChevronDown from "../../../assets/icons/chevron-down.svg";

interface ButtonProps {
    textTrue?: string,
    textFalse?: string;
    onClick?: () => void
    showState?: boolean;
    setShowState?: (value: React.SetStateAction<boolean>) => void;
}

export function ShowMoreButton(props: ButtonProps) {
    return (
        <button 
            className="show-more-btn"
            type="button"
            onClick={() => props.setShowState?.(!props.showState)}
        >
            <span className="show-more-btn__text">{props.showState ? `${props.textTrue}` : `${props.textFalse}` }</span>
            <img width={14} src={props.showState ? ChevronUp :  ChevronDown } />
        </button>
    )
}

export function ClearFiltersButton(props: ButtonProps) {
    return (
        <button className="clear-filters-btn">

        </button>
    )
}
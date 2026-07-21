import { useState } from "react";
import "./checkbox-group.css";
import Icons from "../shared/icons";

type CheckboxProps = {
  title: string,
  options: string[];
  selected: string[];
  counts: Record<string, number>;
  onToggle: (value: string) => void;
};

export default function CheckboxGroup(props: CheckboxProps) {
    const [showTypes, setShowTypes] = useState(false);

    return (
        <fieldset className="checkbox-group">
            <legend className="checkbox-group__legend">
                <button 
                    className="checkbox-group__legend-button"
                    type="button"
                    onClick={() => setShowTypes(!showTypes)}
                >
                    <span>{props.title}</span>
                    <span>{showTypes ? "▲" : "▼" }</span>
                </button>
            </legend>
            {showTypes && (
                props.options.map(option => (
                    <label key={option} className="checkbox-group__option">
                        <input  
                            type="checkbox"
                            className="checkbox-group__option__checkbox"
                            checked={props.selected.includes(option)}
                            onChange={() => props.onToggle(option)}
                        />
                        <Icons text={option} />
                        <span className="checkbox-group__option-text">
                            ({props.counts[option] ?? 0})
                        </span>
                        {/* <span className="checkbox-group__option-text">{option} {props.selected.length} </span> */}
                    </label>
                ))
            )}
        </fieldset>
    )
}
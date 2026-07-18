import { useState } from "react";
import "./checkbox-group.css";

type CheckboxProps = {
  title: string,
  options: string[];
  selected: string[];
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
                        <span className="checkbox-group__option-text">{option}</span>
                    </label>
                ))
            )}
        </fieldset>
    )
}
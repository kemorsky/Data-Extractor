import { useState } from "react";
import "./checkbox-group.css";
import Icons from "../shared/icons";
import { ShowMoreButton } from "../../components/shared/buttons";

type CheckboxProps = {
  title: string,
  options: string[];
  selected: string[] | boolean;
  counts: Record<string, number>;
  onToggle: (value: string) => void;
//   onOpen?: () => void;
};

export default function CheckboxGroup(props: CheckboxProps) {
    const [showTypes, setShowTypes] = useState(false);

    const visibleOptions = showTypes
        ? props.options
        : props.options.slice(0, 3);

    return (
        <fieldset className="checkbox-group">
            <legend 
                className="checkbox-group__legend"
                // onClick={props.onOpen}
            >
                <span>{props.title}</span>
                
            </legend>
            {visibleOptions.map(option => (
                <label key={option} 
                    className="checkbox-group__option"
                    style={{color: props.counts[option] === undefined ? "gray" : undefined}}
                >
                    <input  
                        type="checkbox"
                        className="checkbox-group__option__checkbox"
                        checked={Array.isArray(props.selected) ? props.selected.includes(option) : props.selected}
                        onChange={() => props.onToggle(option)}
                    />
                    <Icons text={option} />
                    <span className="checkbox-group__option-text">
                        ({props.counts[option] ?? 0})
                    </span>
                    {/* <span className="checkbox-group__option-text">{option} {props.selected.length} </span> */}
                </label>
                ))
            }
            {props.options.length > 3 &&
                <ShowMoreButton 
                    showState={showTypes} 
                    setShowState={setShowTypes}
                    textTrue={"Show less"}
                    textFalse={"Show more"} 
                />
            }
        </fieldset>
    )
}
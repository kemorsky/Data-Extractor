import "./checkbox-group.css";

type CheckboxProps = {
  title: string,
  options: string[];
  selected: string[];
  onToggle: (value: string) => void;
};

export default function CheckboxGroup(props: CheckboxProps) {
    return (
        <fieldset className="checkbox-group">
            <legend>{props.title}</legend>
            {props.options.map(option => (
                <label key={option} className="checkbox-group__option">
                    <input  
                        type="checkbox"
                        className="checkbox-group__option__checkbox"
                        checked={props.selected.includes(option)}
                        onChange={() => props.onToggle(option)}
                    />
                    <span className="checkbox-group__option-text">{option}</span>
                </label>
            ))}
        </fieldset>
    )
}
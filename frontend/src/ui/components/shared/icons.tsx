import "./shared.css";

const icons = import.meta.glob("../../../assets/*.svg", {
    eager: true,
    import: "default",
}) as Record<string, string>;

const iconSources = Object.fromEntries(
    Object.entries(icons).map(([path, src]) => {
        const fileName = path.split("/").pop()!.replace(".svg", "");
        return [fileName, src];
    })
);

interface IconsProps {
    text: string;
    showText?: boolean;
    
};

// function isIcon(value: string): value is keyof typeof iconSources {
//     return value in iconSources;
// }

export default function Icons({ showText = true, text }: IconsProps) {
    const src = iconSources[text];

    return (
        <span className="icons">
            {showText && text} 
            {/* checkbox-group__option-text */}
            {src && <img width={30} height={30} src={src} alt={text} />}
        </span>
    )
};

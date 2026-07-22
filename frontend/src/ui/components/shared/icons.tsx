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
    width?: number;
    height?: number;
    
};

// function isIcon(value: string): value is keyof typeof iconSources {
//     return value in iconSources;
// }

export default function Icons({ text, showText = true, width = 30, height = 30 }: IconsProps) {
    const src = iconSources[text];

    return (
        <span className="icons">
            
            {src && 
                <img className="icons__image" 
                    width={width} 
                    height={height} 
                    src={src} 
                    alt={`${text} icon`} 
                />
            }
            {showText && text} 
        </span>
    )
};

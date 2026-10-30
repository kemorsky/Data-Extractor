import "./shared.css";

const images = import.meta.glob("../../../assets/map-cities/*.{svg,png,jpg,webp}", {
    eager: true,
    import: "default",
}) as Record<string, string>;

const iconSources = Object.fromEntries(
    Object.entries(images).map(([path, src]) => {
        const fileName = path.split("/").pop()!.replace(/\.[^.]+$/, "");
        return [fileName, src];
    })
);

interface DrawerCityMapProps {
    text: string;
    showText?: boolean;
};

export default function DrawerCityMap({ text, showText = true }: DrawerCityMapProps) {
    
    const src = iconSources[text];

    return (
        <div 
            id="image-modal" 
            className="parent-map"
        >
            <img className="image-modal__image" 
                src={src} 
                alt={`${text} image`} 
            />
            {showText && text}
        </div>
    )
};

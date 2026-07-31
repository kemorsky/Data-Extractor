import "./shared.css";
import DefaultImage from "../../../assets/location-background-images/Default.jpg"

const images = import.meta.glob("../../../assets/location-background-images/*.{svg,png,jpg,webp}", {
    eager: true,
    import: "default",
}) as Record<string, string>;

const iconSources = Object.fromEntries(
    Object.entries(images).map(([path, src]) => {
        const fileName = path.split("/").pop()!.replace(/\.[^.]+$/, "");
        return [fileName, src];
    })
);

interface LocationCardBackgroundProps {
    text: string;
    showText?: boolean;
    width?: number | string;
    height?: number | string;
    
};

export default function LocationCardBackground({ text, showText = true, width = 30, height = 30 }: LocationCardBackgroundProps) {
    const src = iconSources[text];

    return (
        <>
            
            {src ? (
                <img className="location-card__image" 
                    width={width} 
                    height={height} 
                    src={src} 
                    alt={`${text} image`} 
                />
            ) : (
                <img className="location-card__image" 
                    width={width} 
                    height={height} 
                    src={DefaultImage} 
                    alt={`${text} image`} 
                />
            )}
            {showText && text} 
        </>
    )
};

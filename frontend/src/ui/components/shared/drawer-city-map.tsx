import "./shared.css";
import X from "../../../assets/icons/cross.svg"

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
    width?: number | string;
    height?: number | string;
};

export default function DrawerCityMap({ text, showText = true }: DrawerCityMapProps) {
    const src = iconSources[text];

    return (
        <dialog id="image-modal" popover={""}>
            <button popoverTarget="image-modal">
                <img src={X} width={24} />
            </button>
            <img className="image-modal__image" 
                src={src} 
                alt={`${text} image`} 
            />
            {showText && text}
        </dialog>
    )
};

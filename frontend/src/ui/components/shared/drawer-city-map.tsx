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
    isOpen: boolean;
    onClose: () => void;
};

export default function DrawerCityMap({ text, showText = true, isOpen, onClose }: DrawerCityMapProps) {
    if (!isOpen) return null;
    
    const src = iconSources[text];

    return (
        <dialog 
            id="image-modal" 
            ref={(node) => {
                if (node && !node.open) {
                    node.showModal();
                }
            }} 
            onClose={onClose}
        >
            <button onClick={onClose} aria-label="Close">
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

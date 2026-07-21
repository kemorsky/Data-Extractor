import "./shared.css";

const statusClasses = {
    Finalized: "status__finalized",
    "Needs finalization": "status__needs-finalization",
    "First passed": "status__first-passed",
    "Needs more work": "status__needs-more-work",
    "Not started": "status__not-started",
    Redo: "status__redo",
} as const;

interface StatusProps {
    text: string;
};

function isStatus(value: string): value is keyof typeof statusClasses {
    return value in statusClasses;
}

export default function Status({ text }: StatusProps) {
    return <span className={`status ${isStatus(text) ? statusClasses[text] : "status"}`}>
        {text}
    </span>
}
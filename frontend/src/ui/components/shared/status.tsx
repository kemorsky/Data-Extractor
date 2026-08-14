import "./shared.css";

const statusCircleClasses = {
    Finalized: "status__circle__finalized",
    "Needs finalization": "status__circle__needs-finalization",
    "First passed": "status__circle__first-passed",
    "Needs more work": "status__circle__needs-more-work",
    "Not started": "status__circle__not-started",
    Redo: "status__circle__redo",
} as const;

const statusBorderClasses = {
    Finalized: "status__finalized",
    "Needs finalization": "status__needs-finalization",
    "First passed": "status__first-passed",
    "Needs more work": "status__needs-more-work",
    "Not started": "status__not-started",
    Redo: "status__redo",
} as const;

// const sizeClasses = {
//   sm: "status--sm",
//   md: "status--md",
//   lg: "status--lg",
// } as const;

interface StatusProps {
    text: string;
    // size?: keyof typeof sizeClasses;
};

function isStatus(value: string): value is keyof typeof statusBorderClasses {
    return value in statusBorderClasses;
}

export default function Status({ text }: StatusProps) {
    return (
        <div className={
            [ 
                "status-new",
                isStatus(text) ? statusBorderClasses[text] : "",
            ].join(" ")}>
            <div className={
                [
                    "status__circle",
                    isStatus(text) ? statusCircleClasses[text] : "",
                ].join(" ")}/>
            <span className="status__text">{text}</span>
        </div>
    )
}
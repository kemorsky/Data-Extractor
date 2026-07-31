import "./shared.css";

const statusClasses = {
    Finalized: "status__finalized",
    "Needs finalization": "status__needs-finalization",
    "First passed": "status__first-passed",
    "Needs more work": "status__needs-more-work",
    "Not started": "status__not-started",
    Redo: "status__redo",
} as const;

const sizeClasses = {
  sm: "status--sm",
  md: "status--md",
  lg: "status--lg",
} as const;

interface StatusProps {
    text: string;
    size?: keyof typeof sizeClasses;
};

function isStatus(value: string): value is keyof typeof statusClasses {
    return value in statusClasses;
}

export default function Status({ text, size = "sm" }: StatusProps) {
    // <div
    //   className={[
    //     "status",
    //     sizeClasses[size],
    //     isStatus(text) ? statusClasses[text] : "",
    //   ].join(" ")}
    // ></div>

    return <div className={[
        "status",
        sizeClasses[size],
        isStatus(text) ? statusClasses[text] : "",
      ].join(" ")}>
        <span className="status__text">{text}</span>
    </div>
}
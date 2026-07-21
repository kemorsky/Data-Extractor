import "./shared.css";

export default function Footer() {
    return (
        <footer className="footer">
            <ul className="footer__quick-links">
                <label className="footer__label">Quick Links</label>
                <li >BS Website</li>
                <li>Vikunja</li>
                <li>Pipeline Tool Website</li>
            </ul>
            <span className="footer__copyright">© Beyond Skyrim 2E 26</span>
        </footer>
    )
}
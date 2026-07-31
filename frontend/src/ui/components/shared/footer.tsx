import "./shared.css";
import Marking from "../../../assets/border/markings.svg";

export default function Footer() {
    return (
        <footer className="footer">
            <section className="footer__content">
                <img className="footer__markings" src={Marking} height={40}/>  
                <span className="footer__copyright">© Beyond Skyrim 2E 26</span>
            </section>
        </footer>
    )
}
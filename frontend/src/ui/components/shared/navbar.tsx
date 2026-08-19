import "./shared.css";
import {useState} from "react"
import LogoFull from "../../../assets/logo-full.svg"
import Logo from "../../../assets/logo.svg"
import Hamburger from "../../../assets/icons/hamburger.svg"

export default function Navbar() {
    const [ menuOpen, setMenuOpen ] = useState(false);

    return (
        <nav className="nav">
            <a href="https://beyondskyrim.org/">
                <picture>
                    <source media="(max-width: 890px)" srcSet={Logo} height={32} />
                    <img className="nav__logo" src={LogoFull} height={24} alt="Logo" />
                </picture>
            </a>
            
            <button 
                className="nav__toggle"
                onClick={() => setMenuOpen(!menuOpen)}
                aria-label="Toggle navigation"
                aria-expanded={menuOpen}
            >
                <img src={Hamburger} width={28} alt="hamburger icon" />
            </button>
            <article className={`nav__content`}>
                
                <a href="https://claims.beyondskyrim.org/">
                    <span>Vikunja</span>
                </a>
                <a href="https://beyond-skyrim.pages.beyondskyrim.org/heartlands/se-heartlands/">
                    <span>Pipeline Tool</span>
                </a>
            </article>
        </nav>
    )
}
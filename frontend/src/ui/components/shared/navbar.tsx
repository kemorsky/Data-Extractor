import "./shared.css";
import LogoFull from "../../../assets/logo-full.svg"
import Logo from "../../../assets/logo.svg"

export default function Navbar() {
    return (
        <nav className="nav">
            <a href="https://beyondskyrim.org/">
                <picture>
                    <source media="(max-width: 890px)" srcSet={Logo} height={37} />
                    <img className="nav__logo" src={LogoFull} height={24} alt="Logo" />
                </picture>
            </a>
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
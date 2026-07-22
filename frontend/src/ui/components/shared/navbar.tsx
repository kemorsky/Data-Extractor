import "./shared.css";
import Logo from "../../../assets/logo-full.svg"

export default function Navbar() {
    return (
        <nav className="nav">
            <img className="nav__logo" src={Logo} width={200} height={26.1} />
            <article className="nav__content">
                
                <a href="https://beyondskyrim.org/">
                    <span>Beyond Skyrim Website</span>
                </a>
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
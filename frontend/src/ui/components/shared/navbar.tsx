import "./shared.css";
import styles from '../../../index.module.css';
import LogoFull from "../../../assets/logo-full.svg"
import Logo from "../../../assets/logo.svg"
import { Tabs } from '@base-ui/react/tabs';
import { useState } from "react";
import Hamburger from "../../../assets/icons/hamburger.svg"

export default function Navbar() {
    const [ menuOpen, setMenuOpen ] = useState(false);
    
    return (
        <nav className="nav">
            <header className="nav__header">
                <a className="nav__anchor" href="https://beyondskyrim.org/" tabIndex={0}>
                    <picture className="nav__logo">
                        <source media="(max-width: 905px)" srcSet={Logo} height={37} />
                        <img className="nav__logo__icon" src={LogoFull} height={24} alt="Logo" />
                    </picture>
                </a>
            </header>
            
            <article className="nav__content">
                
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
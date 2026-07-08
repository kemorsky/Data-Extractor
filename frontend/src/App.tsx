import { useState } from 'react'
import { useNavigate } from 'react-router'
import reactLogo from './assets/react.svg'
import viteLogo from './assets/vite.svg'
import './App.css'
import type { LocationData } from './utils/types'
import { getLocations, getLocationByName } from './api/api'

export default function App() {
  const [count, setCount] = useState(0);
  const [ location, setLocation ] = useState<LocationData[] | null>([]);
  const [ locationData, setLocationData ] = useState<LocationData>();
  const navigate = useNavigate();

  const handleClick = async () => {
    const data = await getLocations();
    console.log(data);
    setLocation(data);
    return data;
  }

  const handleClickName = async (name: string): Promise<LocationData> => {
    // console.log("clicked");

    const data = await getLocationByName(name);
    console.log(data);
    setLocationData(data);
    navigate(`/location/${name}`)
    return data;
  }

  return (
    <>
      <section id="center">
        <div className="hero">
          {locationData && (
            <div>
              <h1>{locationData.name}</h1>
              <p>{locationData.parentLocation}</p>
              <p>{locationData.type}</p>
              <p>{locationData.status}</p>
              <p>{locationData.relatedQuestName}</p>
              <p>{locationData.relatedQuestUrl}</p>
            </div>
          )}
          <img src={viteLogo} className="vite" alt="Vite logo" />
          {location?.map((location) => (
            <div key={location.id} onClick={() => {handleClickName(location.name ?? "")}}>
              <h1>{location.name}</h1>
              <p>{location.parentLocation}</p>
              <p>{location.type}</p>
              <p>{location.status}</p>
              <p>{location.relatedQuestName}</p>
              <p>{location.relatedQuestUrl}</p>
            </div>
          ))}
        </div>
        <div>
          <h1>Get started</h1>
          <p>
            Edit <code>src/App.tsx</code> and save to test <code>HMR</code>
          </p>
        </div>
        <button
          type="button"
          className="counter"
          onClick={() => setCount((count) => count + 1)}
        >
          Count is {count}
        </button>
        <button onClick={() => { handleClick()}}>Get locations</button>
        <button onClick={() => { navigate('/location')}}>Go to location</button>
      </section>

      <div className="ticks"></div>

      <section id="next-steps">
        <div id="docs">
          <svg className="icon" role="presentation" aria-hidden="true">
            <use href="/icons.svg#documentation-icon"></use>
          </svg>
          <h2>Documentation</h2>
          <p>Your questions, answered</p>
          <ul>
            <li>
              <a href="https://vite.dev/" target="_blank">
                <img className="logo" src={viteLogo} alt="" />
                Explore Vite
              </a>
            </li>
            <li>
              <a href="https://react.dev/" target="_blank">
                <img className="button-icon" src={reactLogo} alt="" />
                Learn more
              </a>
            </li>
          </ul>
        </div>
        <div id="social">
          <svg className="icon" role="presentation" aria-hidden="true">
            <use href="/icons.svg#social-icon"></use>
          </svg>
          <h2>Connect with us</h2>
          <p>Join the Vite community</p>
          <ul>
            <li>
              <a href="https://github.com/vitejs/vite" target="_blank">
                <svg
                  className="button-icon"
                  role="presentation"
                  aria-hidden="true"
                >
                  <use href="/icons.svg#github-icon"></use>
                </svg>
                GitHub
              </a>
            </li>
            <li>
              <a href="https://chat.vite.dev/" target="_blank">
                <svg
                  className="button-icon"
                  role="presentation"
                  aria-hidden="true"
                >
                  <use href="/icons.svg#discord-icon"></use>
                </svg>
                Discord
              </a>
            </li>
            <li>
              <a href="https://x.com/vite_js" target="_blank">
                <svg
                  className="button-icon"
                  role="presentation"
                  aria-hidden="true"
                >
                  <use href="/icons.svg#x-icon"></use>
                </svg>
                X.com
              </a>
            </li>
            <li>
              <a href="https://bsky.app/profile/vite.dev" target="_blank">
                <svg
                  className="button-icon"
                  role="presentation"
                  aria-hidden="true"
                >
                  <use href="/icons.svg#bluesky-icon"></use>
                </svg>
                Bluesky
              </a>
            </li>
          </ul>
        </div>
      </section>

      <div className="ticks"></div>
      <section id="spacer"></section>
    </>
  )
}
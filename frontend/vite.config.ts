import { defineConfig } from 'vite'
import react, { reactCompilerPreset } from '@vitejs/plugin-react'
import babel from '@rolldown/plugin-babel'
import path from 'path'

// https://vite.dev/config/
export default defineConfig({
  base: '/Data-Extractor/locations/',
  plugins: [
    react(),
    babel({ presets: [reactCompilerPreset()] })
  ],
  envDir: path.resolve(__dirname, '..')
})

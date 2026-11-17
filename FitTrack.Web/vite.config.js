import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// Plugin de Tailwind v4 para Vite.
export default defineConfig({
  plugins: [react(), tailwindcss()],
})

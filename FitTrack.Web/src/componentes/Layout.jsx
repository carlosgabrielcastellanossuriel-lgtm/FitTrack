import { Outlet } from 'react-router-dom'

// Barra superior y contenido centrado.
// Cada pantalla agregará sus enlaces en la parte derecha de la barra.
function Layout() {
  return (
    <div className="min-h-screen bg-white text-zinc-900 dark:bg-zinc-950 dark:text-zinc-100">
      <header className="border-b border-zinc-200 dark:border-zinc-800">
        <div className="mx-auto flex max-w-5xl items-center justify-between px-4 py-4">
          <span className="text-lg font-semibold text-emerald-600 dark:text-emerald-400">
            FitTrack
          </span>

          <nav className="flex items-center gap-4 text-sm">
            {/* Aquí se agregarán los enlaces de cada pantalla. */}
          </nav>
        </div>
      </header>

      <main className="mx-auto max-w-5xl px-4 py-8">
        <Outlet />
      </main>
    </div>
  )
}

export default Layout

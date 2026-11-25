import { Link, Outlet, useNavigate } from 'react-router-dom'
import { useSesion } from '../SesionContext.jsx'

// Barra superior y contenido centrado.
function Layout() {
  const { usuario, cerrarSesion } = useSesion()
  const navigate = useNavigate()

  async function salir() {
    await cerrarSesion()
    navigate('/login')
  }

  return (
    <div className="min-h-screen bg-white text-zinc-900 dark:bg-zinc-950 dark:text-zinc-100">
      <header className="border-b border-zinc-200 dark:border-zinc-800">
        <div className="mx-auto flex max-w-5xl flex-wrap items-center justify-between gap-3 px-4 py-4">
          <Link to="/" className="text-lg font-semibold text-emerald-600 dark:text-emerald-400">
            FitTrack
          </Link>

          <nav className="flex flex-wrap items-center gap-4 text-sm">
            {usuario ? (
              <>
                <span className="hidden text-zinc-500 sm:inline dark:text-zinc-400">
                  {usuario.correo}
                </span>
                <Link
                  to="/alimentos"
                  className="text-zinc-600 transition hover:text-emerald-600 dark:text-zinc-400 dark:hover:text-emerald-400"
                >
                  Alimentos
                </Link>
                <Link
                  to="/documentos"
                  className="text-zinc-600 transition hover:text-emerald-600 dark:text-zinc-400 dark:hover:text-emerald-400"
                >
                  Documentos
                </Link>
                <Link
                  to="/historial"
                  className="text-zinc-600 transition hover:text-emerald-600 dark:text-zinc-400 dark:hover:text-emerald-400"
                >
                  Historial
                </Link>
                <Link
                  to="/permisos"
                  className="text-zinc-600 transition hover:text-emerald-600 dark:text-zinc-400 dark:hover:text-emerald-400"
                >
                  Permisos
                </Link>
                {usuario.rol === 'Administrador' && (
                  <Link
                    to="/usuarios"
                    className="text-zinc-600 transition hover:text-emerald-600 dark:text-zinc-400 dark:hover:text-emerald-400"
                  >
                    Usuarios
                  </Link>
                )}
                <Link
                  to="/perfil"
                  className="text-zinc-600 transition hover:text-emerald-600 dark:text-zinc-400 dark:hover:text-emerald-400"
                >
                  Perfil
                </Link>
                <Link
                  to="/cuenta"
                  className="text-zinc-600 transition hover:text-emerald-600 dark:text-zinc-400 dark:hover:text-emerald-400"
                >
                  Cuenta
                </Link>
                <button
                  type="button"
                  onClick={salir}
                  className="text-zinc-600 transition hover:text-emerald-600 dark:text-zinc-400 dark:hover:text-emerald-400"
                >
                  Salir
                </button>
              </>
            ) : (
              <>
                <Link
                  to="/login"
                  className="text-zinc-600 transition hover:text-emerald-600 dark:text-zinc-400 dark:hover:text-emerald-400"
                >
                  Entrar
                </Link>
                <Link
                  to="/registro"
                  className="font-medium text-emerald-600 transition hover:text-emerald-700 dark:text-emerald-400 dark:hover:text-emerald-300"
                >
                  Crear cuenta
                </Link>
              </>
            )}
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

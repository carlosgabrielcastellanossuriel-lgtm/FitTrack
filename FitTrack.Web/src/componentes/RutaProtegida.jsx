import { Navigate } from 'react-router-dom'
import { useSesion } from '../SesionContext.jsx'
import Cargando from './Cargando.jsx'

// Envuelve las páginas que necesitan sesión.
function RutaProtegida({ soloAdmin = false, children }) {
  const { usuario, cargando } = useSesion()

  // Mientras se valida el token guardado, todavía no se redirige.
  if (cargando) return <Cargando />

  if (!usuario) {
    return <Navigate to="/login" replace />
  }

  // Las páginas marcadas como soloAdmin exigen el rol Administrador.
  if (soloAdmin && usuario.rol !== 'Administrador') {
    return <Navigate to="/" replace />
  }

  return children
}

export default RutaProtegida

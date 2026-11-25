import { createContext, useContext, useEffect, useState } from 'react'
import { llamarApi } from './api.js'

const SesionContexto = createContext(null)

// Guarda el token y el usuario de la sesión actual.
export function SesionProvider({ children }) {
  const [token, setToken] = useState(() => localStorage.getItem('fittrack_token'))
  const [usuario, setUsuario] = useState(null)
  // Solo hay carga inicial si existe un token guardado.
  const [cargando, setCargando] = useState(() => Boolean(localStorage.getItem('fittrack_token')))

  useEffect(() => {
    const guardado = localStorage.getItem('fittrack_token')

    if (!guardado) {
      return
    }

    // Si hay token guardado, se valida y se cargan los datos del usuario.
    llamarApi('/api/auth/yo')
      .then((datos) => setUsuario(datos))
      .catch(() => {
        localStorage.removeItem('fittrack_token')
        setToken(null)
      })
      .finally(() => setCargando(false))
  }, [])

  async function iniciarSesion(nuevoToken) {
    localStorage.setItem('fittrack_token', nuevoToken)
    setToken(nuevoToken)

    // Se cargan los datos para que el Layout muestre el correo y el rol.
    const datos = await llamarApi('/api/auth/yo')
    setUsuario(datos)
  }

  async function cerrarSesion() {
    try {
      await llamarApi('/api/auth/logout', { method: 'POST' })
    } catch {
      // Si la API falla, igual se borra la sesión local.
    }

    localStorage.removeItem('fittrack_token')
    setToken(null)
    setUsuario(null)
  }

  return (
    <SesionContexto.Provider value={{ token, usuario, cargando, iniciarSesion, cerrarSesion }}>
      {children}
    </SesionContexto.Provider>
  )
}

// Hook para usar la sesión desde cualquier componente.
export function useSesion() {
  const contexto = useContext(SesionContexto)

  if (!contexto) {
    throw new Error('useSesion debe usarse dentro de SesionProvider.')
  }

  return contexto
}

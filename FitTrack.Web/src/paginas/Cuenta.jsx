import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { llamarApi } from '../api.js'
import { useSesion } from '../SesionContext.jsx'
import Aviso from '../componentes/Aviso.jsx'
import Boton from '../componentes/Boton.jsx'
import Campo from '../componentes/Campo.jsx'
import Tarjeta from '../componentes/Tarjeta.jsx'
import { claveValida, MENSAJE_CLAVE } from '../validaciones.js'

// Pantalla con los datos de la cuenta y el cambio de contraseña.
function Cuenta() {
  const { usuario, cerrarSesion } = useSesion()
  const navigate = useNavigate()

  const [claveActual, setClaveActual] = useState('')
  const [claveNueva, setClaveNueva] = useState('')
  const [error, setError] = useState('')
  const [enviando, setEnviando] = useState(false)

  const errorActual = claveActual && !claveValida(claveActual) ? MENSAJE_CLAVE : ''
  const errorClave = claveNueva && !claveValida(claveNueva) ? MENSAJE_CLAVE : ''
  const puedeEnviar = claveValida(claveActual) && claveValida(claveNueva) && !enviando

  // Al cerrar la sesión el usuario pasa a null antes de redirigir.
  if (!usuario) return null

  async function cambiar(evento) {
    evento.preventDefault()
    setError('')
    setEnviando(true)

    try {
      const datos = await llamarApi('/api/contrasena/cambiar', {
        method: 'POST',
        body: { claveActual, claveNueva },
      })

      // La API cierra todas las sesiones: hay que volver a entrar.
      await cerrarSesion()
      navigate('/login', { state: { mensaje: datos.mensaje } })
    } catch (e) {
      setError(e.message)
    } finally {
      setEnviando(false)
    }
  }

  return (
    <div className="mx-auto max-w-md space-y-6">
      <h1 className="text-2xl font-semibold">Mi cuenta</h1>

      <Tarjeta className="space-y-1">
        <p className="text-sm text-zinc-500 dark:text-zinc-400">Correo</p>
        <p className="font-medium">{usuario.correo}</p>
        <p className="pt-3 text-sm text-zinc-500 dark:text-zinc-400">Rol</p>
        <p className="font-medium">{usuario.rol}</p>
      </Tarjeta>

      <Tarjeta>
        <h2 className="mb-4 text-lg font-semibold">Cambiar contraseña</h2>

        <form onSubmit={cambiar} className="space-y-4" noValidate>
          {error && <Aviso>{error}</Aviso>}

          <Campo
            etiqueta="Contraseña actual"
            tipo="password"
            value={claveActual}
            onChange={(e) => setClaveActual(e.target.value)}
            error={errorActual}
            autoComplete="current-password"
          />

          <Campo
            etiqueta="Nueva contraseña"
            tipo="password"
            value={claveNueva}
            onChange={(e) => setClaveNueva(e.target.value)}
            error={errorClave}
            autoComplete="new-password"
          />

          <Boton tipo="submit" cargando={enviando} disabled={!puedeEnviar} className="w-full">
            {enviando ? 'Guardando...' : 'Cambiar contraseña'}
          </Boton>
        </form>
      </Tarjeta>
    </div>
  )
}

export default Cuenta

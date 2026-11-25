import { useState } from 'react'
import { Link } from 'react-router-dom'
import { llamarApi } from '../api.js'
import Aviso from '../componentes/Aviso.jsx'
import Boton from '../componentes/Boton.jsx'
import Campo from '../componentes/Campo.jsx'
import Tarjeta from '../componentes/Tarjeta.jsx'
import { claveValida, MENSAJE_CLAVE } from '../validaciones.js'

// Pantalla para usar el código del correo y definir la nueva contraseña.
function RestablecerContrasena() {
  const [codigo, setCodigo] = useState('')
  const [claveNueva, setClaveNueva] = useState('')
  const [error, setError] = useState('')
  const [mensaje, setMensaje] = useState('')
  const [enviando, setEnviando] = useState(false)

  const errorClave = claveNueva && !claveValida(claveNueva) ? MENSAJE_CLAVE : ''
  const puedeEnviar = codigo.trim() !== '' && claveValida(claveNueva) && !enviando

  async function enviar(evento) {
    evento.preventDefault()
    setError('')
    setEnviando(true)

    try {
      const datos = await llamarApi('/api/contrasena/restablecer', {
        method: 'POST',
        body: { codigo: codigo.trim(), claveNueva },
      })

      setMensaje(datos.mensaje)
    } catch (e) {
      setError(e.message)
    } finally {
      setEnviando(false)
    }
  }

  if (mensaje) {
    return (
      <div className="mx-auto max-w-md">
        <h1 className="mb-6 text-2xl font-semibold">Restablecer contraseña</h1>
        <Tarjeta className="space-y-4">
          <Aviso tipo="exito">{mensaje}</Aviso>
          <Link
            to="/login"
            className="inline-block rounded-lg bg-emerald-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-emerald-700 dark:bg-emerald-500 dark:hover:bg-emerald-400"
          >
            Iniciar sesión
          </Link>
        </Tarjeta>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-md">
      <h1 className="mb-6 text-2xl font-semibold">Restablecer contraseña</h1>

      <Tarjeta>
        <form onSubmit={enviar} className="space-y-4" noValidate>
          <p className="text-sm text-zinc-600 dark:text-zinc-400">
            Escribe el código que llegó a tu correo y tu nueva contraseña.
          </p>

          {error && <Aviso>{error}</Aviso>}

          <Campo
            etiqueta="Código del correo"
            value={codigo}
            onChange={(e) => setCodigo(e.target.value)}
            autoComplete="one-time-code"
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

        <p className="mt-4 text-sm text-zinc-600 dark:text-zinc-400">
          <Link to="/login" className="text-emerald-600 hover:underline dark:text-emerald-400">
            Volver a entrar
          </Link>
        </p>
      </Tarjeta>
    </div>
  )
}

export default RestablecerContrasena

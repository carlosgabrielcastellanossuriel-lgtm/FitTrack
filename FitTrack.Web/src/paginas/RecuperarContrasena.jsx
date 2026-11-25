import { useState } from 'react'
import { Link } from 'react-router-dom'
import { llamarApi } from '../api.js'
import Aviso from '../componentes/Aviso.jsx'
import Boton from '../componentes/Boton.jsx'
import Campo from '../componentes/Campo.jsx'
import Tarjeta from '../componentes/Tarjeta.jsx'
import { correoValido, MENSAJE_CORREO } from '../validaciones.js'

// Pantalla para pedir el código de recuperación por correo.
function RecuperarContrasena() {
  const [correo, setCorreo] = useState('')
  const [error, setError] = useState('')
  const [mensaje, setMensaje] = useState('')
  const [enviando, setEnviando] = useState(false)

  const errorCorreo = correo && !correoValido(correo) ? MENSAJE_CORREO : ''
  const puedeEnviar = correoValido(correo) && !enviando

  async function enviar(evento) {
    evento.preventDefault()
    setError('')
    setEnviando(true)

    try {
      const datos = await llamarApi('/api/contrasena/recuperar', {
        method: 'POST',
        body: { correo },
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
        <h1 className="mb-6 text-2xl font-semibold">Recuperar contraseña</h1>
        <Tarjeta className="space-y-4">
          <Aviso tipo="exito">{mensaje}</Aviso>
          <Link
            to="/restablecer-contrasena"
            className="inline-block rounded-lg bg-emerald-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-emerald-700 dark:bg-emerald-500 dark:hover:bg-emerald-400"
          >
            Ya tengo el código
          </Link>
          <p className="text-sm text-zinc-600 dark:text-zinc-400">
            <Link to="/login" className="text-emerald-600 hover:underline dark:text-emerald-400">
              Volver a entrar
            </Link>
          </p>
        </Tarjeta>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-md">
      <h1 className="mb-6 text-2xl font-semibold">Recuperar contraseña</h1>

      <Tarjeta>
        <form onSubmit={enviar} className="space-y-4" noValidate>
          <p className="text-sm text-zinc-600 dark:text-zinc-400">
            Escribe tu correo y te enviaremos un código de recuperación.
          </p>

          {error && <Aviso>{error}</Aviso>}

          <Campo
            etiqueta="Correo"
            tipo="email"
            value={correo}
            onChange={(e) => setCorreo(e.target.value)}
            error={errorCorreo}
            autoComplete="email"
          />

          <Boton tipo="submit" cargando={enviando} disabled={!puedeEnviar} className="w-full">
            {enviando ? 'Enviando...' : 'Enviar código'}
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

export default RecuperarContrasena

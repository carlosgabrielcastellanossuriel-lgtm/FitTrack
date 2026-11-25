import { useState } from 'react'
import { Link } from 'react-router-dom'
import { llamarApi } from '../api.js'
import Aviso from '../componentes/Aviso.jsx'
import Boton from '../componentes/Boton.jsx'
import Campo from '../componentes/Campo.jsx'
import Tarjeta from '../componentes/Tarjeta.jsx'
import { claveValida, correoValido, MENSAJE_CLAVE, MENSAJE_CORREO } from '../validaciones.js'

// Pantalla para crear una cuenta.
function Registro() {
  const [correo, setCorreo] = useState('')
  const [clave, setClave] = useState('')
  const [error, setError] = useState('')
  const [mensaje, setMensaje] = useState('')
  const [enviando, setEnviando] = useState(false)

  const errorCorreo = correo && !correoValido(correo) ? MENSAJE_CORREO : ''
  const errorClave = clave && !claveValida(clave) ? MENSAJE_CLAVE : ''
  const puedeEnviar = correoValido(correo) && claveValida(clave) && !enviando

  async function enviar(evento) {
    evento.preventDefault()
    setError('')
    setEnviando(true)

    try {
      const datos = await llamarApi('/api/auth/registro', {
        method: 'POST',
        body: { correo, clave },
      })

      // "Cuenta creada. Revisa tu correo para activarla."
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
        <h1 className="mb-6 text-2xl font-semibold">Crear cuenta</h1>
        <Tarjeta className="space-y-4">
          <Aviso tipo="exito">{mensaje}</Aviso>
          <p className="text-sm text-zinc-600 dark:text-zinc-400">
            El enlace del correo abre la API directamente y activa la cuenta.
          </p>
          <Link
            to="/login"
            className="text-sm font-medium text-emerald-600 hover:underline dark:text-emerald-400"
          >
            Ir a entrar
          </Link>
        </Tarjeta>
      </div>
    )
  }

  return (
    <div className="mx-auto max-w-md">
      <h1 className="mb-6 text-2xl font-semibold">Crear cuenta</h1>

      <Tarjeta>
        <form onSubmit={enviar} className="space-y-4" noValidate>
          {error && <Aviso>{error}</Aviso>}

          <Campo
            etiqueta="Correo"
            tipo="email"
            value={correo}
            onChange={(e) => setCorreo(e.target.value)}
            error={errorCorreo}
            autoComplete="email"
          />

          <Campo
            etiqueta="Contraseña"
            tipo="password"
            value={clave}
            onChange={(e) => setClave(e.target.value)}
            error={errorClave}
            autoComplete="new-password"
          />

          <Boton tipo="submit" cargando={enviando} disabled={!puedeEnviar} className="w-full">
            {enviando ? 'Creando cuenta...' : 'Crear cuenta'}
          </Boton>
        </form>

        <p className="mt-4 text-sm text-zinc-600 dark:text-zinc-400">
          ¿Ya tienes cuenta?{' '}
          <Link to="/login" className="text-emerald-600 hover:underline dark:text-emerald-400">
            Entrar
          </Link>
        </p>
      </Tarjeta>
    </div>
  )
}

export default Registro

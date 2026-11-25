import { useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { llamarApi } from '../api.js'
import { useSesion } from '../SesionContext.jsx'
import Aviso from '../componentes/Aviso.jsx'
import Boton from '../componentes/Boton.jsx'
import Campo from '../componentes/Campo.jsx'
import Tarjeta from '../componentes/Tarjeta.jsx'
import { claveValida, correoValido, MENSAJE_CLAVE, MENSAJE_CORREO } from '../validaciones.js'

// Pantalla para iniciar sesión.
function Login() {
  const { iniciarSesion } = useSesion()
  const navigate = useNavigate()
  const ubicacion = useLocation()

  const [correo, setCorreo] = useState('')
  const [clave, setClave] = useState('')
  const [error, setError] = useState('')
  const [enviando, setEnviando] = useState(false)

  const errorCorreo = correo && !correoValido(correo) ? MENSAJE_CORREO : ''
  const errorClave = clave && !claveValida(clave) ? MENSAJE_CLAVE : ''
  const puedeEnviar = correoValido(correo) && claveValida(clave) && !enviando

  async function enviar(evento) {
    evento.preventDefault()
    setError('')
    setEnviando(true)

    try {
      const datos = await llamarApi('/api/auth/login', {
        method: 'POST',
        body: { correo, clave },
      })

      await iniciarSesion(datos.token)
      navigate('/')
    } catch (e) {
      // El mensaje viene de la API: credenciales, bloqueo o cuenta sin activar.
      setError(e.message)
    } finally {
      setEnviando(false)
    }
  }

  return (
    <div className="mx-auto max-w-md">
      <h1 className="mb-6 text-2xl font-semibold">Entrar</h1>

      <Tarjeta>
        <form onSubmit={enviar} className="space-y-4" noValidate>
          {ubicacion.state?.mensaje && <Aviso tipo="exito">{ubicacion.state.mensaje}</Aviso>}

          {error && (
            <div className="space-y-2">
              <Aviso>{error}</Aviso>
              {error.includes('no está activa') && (
                <Link
                  to="/reenviar-activacion"
                  className="text-sm text-emerald-600 hover:underline dark:text-emerald-400"
                >
                  Reenviar el correo de activación
                </Link>
              )}
            </div>
          )}

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
            autoComplete="current-password"
          />

          <Boton tipo="submit" cargando={enviando} disabled={!puedeEnviar} className="w-full">
            {enviando ? 'Entrando...' : 'Entrar'}
          </Boton>
        </form>

        <div className="mt-4 flex flex-col gap-1 text-sm text-zinc-600 dark:text-zinc-400">
          <Link to="/recuperar-contrasena" className="hover:text-emerald-600 dark:hover:text-emerald-400">
            Olvidé mi contraseña
          </Link>
          <Link to="/registro" className="hover:text-emerald-600 dark:hover:text-emerald-400">
            Crear cuenta
          </Link>
        </div>
      </Tarjeta>
    </div>
  )
}

export default Login

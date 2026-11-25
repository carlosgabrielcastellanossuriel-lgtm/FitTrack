import { useEffect, useState } from 'react'
import { llamarApi } from '../api.js'
import { useSesion } from '../SesionContext.jsx'
import Aviso from '../componentes/Aviso.jsx'
import Boton from '../componentes/Boton.jsx'
import Cargando from '../componentes/Cargando.jsx'
import Selector from '../componentes/Selector.jsx'
import Tarjeta from '../componentes/Tarjeta.jsx'

// Nombres legibles de cada permiso.
const NOMBRES_PERMISO = {
  ExportarDatos: 'Exportar datos',
  VerEstadisticas: 'Ver estadísticas',
}

// Muestra la fecha que manda la API en un formato corto.
function formatearFecha(fecha) {
  return new Date(fecha).toLocaleString('es', { dateStyle: 'short', timeStyle: 'short' })
}

// Pantalla para solicitar permisos y, si eres Administrador, resolverlos.
function Permisos() {
  const { usuario } = useSesion()
  const esAdmin = usuario?.rol === 'Administrador'

  const [cargando, setCargando] = useState(true)
  const [solicitudes, setSolicitudes] = useState([])
  const [errorCarga, setErrorCarga] = useState('')

  // Formulario para crear una solicitud.
  const [permiso, setPermiso] = useState('ExportarDatos')
  const [errorSolicitud, setErrorSolicitud] = useState('')
  const [mensajeSolicitud, setMensajeSolicitud] = useState('')
  const [solicitando, setSolicitando] = useState(false)

  // Resultado de aprobar o rechazar.
  const [errorAccion, setErrorAccion] = useState('')
  const [mensajeAccion, setMensajeAccion] = useState('')
  const [procesandoId, setProcesandoId] = useState(null)

  // Modal de rechazo con motivo obligatorio.
  const [solicitudRechazo, setSolicitudRechazo] = useState(null)
  const [motivo, setMotivo] = useState('')
  const [errorRechazo, setErrorRechazo] = useState('')
  const [rechazando, setRechazando] = useState(false)

  // Prueba de la acción protegida.
  const [probando, setProbando] = useState(false)
  const [resultadoPrueba, setResultadoPrueba] = useState(null)

  // Pide la lista de pendientes. Se usa al entrar y después de cada acción.
  async function cargar() {
    try {
      const datos = await llamarApi('/api/permisos/pendientes')
      setSolicitudes(datos)
      setErrorCarga('')
    } catch (e) {
      setErrorCarga(e.message)
    } finally {
      setCargando(false)
    }
  }

  useEffect(() => {
    cargar()
  }, [])

  async function solicitar(evento) {
    evento.preventDefault()
    setErrorSolicitud('')
    setMensajeSolicitud('')
    setSolicitando(true)

    try {
      const datos = await llamarApi('/api/permisos', {
        method: 'POST',
        body: { permiso },
      })

      setMensajeSolicitud(datos.mensaje)
      await cargar()
    } catch (e) {
      setErrorSolicitud(e.message)
    } finally {
      setSolicitando(false)
    }
  }

  async function aprobar(solicitud) {
    setErrorAccion('')
    setMensajeAccion('')
    setProcesandoId(solicitud.id)

    try {
      const datos = await llamarApi(`/api/permisos/${solicitud.id}/aprobar`, {
        method: 'POST',
      })

      setMensajeAccion(datos.mensaje)
      await cargar()
    } catch (e) {
      // Aquí se muestra el mensaje si intenta resolver su propia solicitud.
      setErrorAccion(e.message)
    } finally {
      setProcesandoId(null)
    }
  }

  function abrirRechazo(solicitud) {
    setSolicitudRechazo(solicitud)
    setMotivo('')
    setErrorRechazo('')
  }

  function cerrarRechazo() {
    setSolicitudRechazo(null)
    setMotivo('')
    setErrorRechazo('')
  }

  async function rechazar(evento) {
    evento.preventDefault()
    setErrorRechazo('')
    setRechazando(true)

    try {
      const datos = await llamarApi(`/api/permisos/${solicitudRechazo.id}/rechazar`, {
        method: 'POST',
        body: { motivo: motivo.trim() },
      })

      cerrarRechazo()
      setErrorAccion('')
      setMensajeAccion(datos.mensaje)
      await cargar()
    } catch (e) {
      setErrorRechazo(e.message)
    } finally {
      setRechazando(false)
    }
  }

  async function probarAccion() {
    setProbando(true)
    setResultadoPrueba(null)

    try {
      const datos = await llamarApi('/api/permisos/accion-protegida')
      setResultadoPrueba({ ok: true, estado: 200, mensaje: datos.mensaje })
    } catch (e) {
      setResultadoPrueba({ ok: false, estado: e.estado, mensaje: e.message })
    } finally {
      setProbando(false)
    }
  }

  if (cargando) return <Cargando />

  if (errorCarga) {
    return (
      <div className="mx-auto max-w-3xl space-y-4">
        <h1 className="text-2xl font-semibold">Permisos</h1>
        <Aviso>{errorCarga}</Aviso>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold">Permisos</h1>

      <div className="grid gap-6 lg:grid-cols-2">
        <Tarjeta>
          <h2 className="mb-3 text-lg font-semibold">Solicitar permiso</h2>

          <form onSubmit={solicitar} className="space-y-4" noValidate>
            {errorSolicitud && <Aviso>{errorSolicitud}</Aviso>}
            {mensajeSolicitud && <Aviso tipo="exito">{mensajeSolicitud}</Aviso>}

            <Selector etiqueta="Permiso" value={permiso} onChange={(e) => setPermiso(e.target.value)}>
              <option value="ExportarDatos">Exportar datos</option>
              <option value="VerEstadisticas">Ver estadísticas</option>
            </Selector>

            <Boton tipo="submit" cargando={solicitando} disabled={solicitando} className="w-full">
              {solicitando ? 'Enviando...' : 'Solicitar permiso'}
            </Boton>
          </form>
        </Tarjeta>

        <Tarjeta className="space-y-3">
          <h2 className="text-lg font-semibold">Probar acción protegida</h2>
          <p className="text-sm text-zinc-500 dark:text-zinc-400">
            Llama a la acción que exige el permiso Exportar datos y muestra si la API respondió 200 o
            403.
          </p>

          {resultadoPrueba && (
            <Aviso tipo={resultadoPrueba.ok ? 'exito' : 'error'}>
              {resultadoPrueba.estado ? `${resultadoPrueba.estado} · ` : ''}
              {resultadoPrueba.mensaje}
            </Aviso>
          )}

          <Boton onClick={probarAccion} cargando={probando} disabled={probando}>
            {probando ? 'Probando...' : 'Probar acción'}
          </Boton>
        </Tarjeta>
      </div>

      <Tarjeta>
        <h2 className="mb-3 text-lg font-semibold">Solicitudes pendientes</h2>

        {errorAccion && (
          <div className="mb-3">
            <Aviso>{errorAccion}</Aviso>
          </div>
        )}
        {mensajeAccion && (
          <div className="mb-3">
            <Aviso tipo="exito">{mensajeAccion}</Aviso>
          </div>
        )}

        {solicitudes.length === 0 ? (
          <p className="py-4 text-sm text-zinc-500 dark:text-zinc-400">
            No hay solicitudes pendientes.
          </p>
        ) : (
          <ul className="divide-y divide-zinc-200 dark:divide-zinc-800">
            {solicitudes.map((solicitud) => (
              <li key={solicitud.id} className="py-4">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <div className="flex flex-wrap items-center gap-2">
                      <p className="font-medium">
                        {NOMBRES_PERMISO[solicitud.permiso] || solicitud.permiso}
                      </p>
                      <span className="rounded-full bg-zinc-100 px-2 py-0.5 text-xs font-medium text-zinc-600 dark:bg-zinc-800 dark:text-zinc-300">
                        Pendiente
                      </span>
                    </div>
                    <p className="mt-1 text-xs text-zinc-500 dark:text-zinc-400">
                      {esAdmin && solicitud.solicitanteId !== usuario?.id
                        ? `Solicitante: usuario #${solicitud.solicitanteId}`
                        : 'Tu solicitud'}{' '}
                      · {formatearFecha(solicitud.fechaCreacion)}
                    </p>
                  </div>

                  {esAdmin && (
                    <div className="flex gap-2">
                      <button
                        type="button"
                        onClick={() => aprobar(solicitud)}
                        disabled={procesandoId !== null}
                        className="rounded-lg border border-zinc-300 px-3 py-1 text-xs font-medium text-emerald-700 transition hover:border-emerald-500 disabled:cursor-not-allowed disabled:opacity-50 dark:border-zinc-700 dark:text-emerald-400 dark:hover:border-emerald-400"
                      >
                        {procesandoId === solicitud.id ? 'Aprobando...' : 'Aprobar'}
                      </button>
                      <button
                        type="button"
                        onClick={() => abrirRechazo(solicitud)}
                        disabled={procesandoId !== null}
                        className="rounded-lg border border-zinc-300 px-3 py-1 text-xs font-medium text-red-600 transition hover:border-red-400 disabled:cursor-not-allowed disabled:opacity-50 dark:border-zinc-700 dark:text-red-400 dark:hover:border-red-500"
                      >
                        Rechazar
                      </button>
                    </div>
                  )}
                </div>
              </li>
            ))}
          </ul>
        )}
      </Tarjeta>

      {solicitudRechazo && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="w-full max-w-md rounded-xl border border-zinc-200 bg-white p-5 shadow-lg dark:border-zinc-800 dark:bg-zinc-900">
            <div className="mb-4 flex items-center justify-between gap-3">
              <h2 className="text-lg font-semibold">Rechazar solicitud</h2>
              <button
                type="button"
                onClick={cerrarRechazo}
                className="text-sm text-zinc-500 transition hover:text-emerald-600 dark:text-zinc-400 dark:hover:text-emerald-400"
              >
                Cerrar
              </button>
            </div>

            <p className="mb-4 text-sm text-zinc-500 dark:text-zinc-400">
              {NOMBRES_PERMISO[solicitudRechazo.permiso] || solicitudRechazo.permiso} · Solicitante:
              usuario #{solicitudRechazo.solicitanteId}
            </p>

            <form onSubmit={rechazar} className="space-y-4" noValidate>
              {errorRechazo && <Aviso>{errorRechazo}</Aviso>}

              <label className="block">
                <span className="mb-1 block text-sm font-medium text-zinc-700 dark:text-zinc-300">
                  Motivo
                </span>
                <textarea
                  rows="3"
                  value={motivo}
                  onChange={(e) => setMotivo(e.target.value)}
                  autoFocus
                  className="w-full rounded-lg border border-zinc-300 bg-white px-3 py-2 text-sm outline-none transition focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/30 dark:border-zinc-700 dark:bg-zinc-900"
                />
              </label>

              <div className="flex justify-end gap-2">
                <button
                  type="button"
                  onClick={cerrarRechazo}
                  className="rounded-lg border border-zinc-300 px-4 py-2 text-sm font-medium text-zinc-600 transition hover:border-zinc-400 dark:border-zinc-700 dark:text-zinc-300 dark:hover:border-zinc-600"
                >
                  Cancelar
                </button>
                <button
                  type="submit"
                  disabled={motivo.trim() === '' || rechazando}
                  className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-red-700 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  {rechazando ? 'Rechazando...' : 'Rechazar'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}

export default Permisos

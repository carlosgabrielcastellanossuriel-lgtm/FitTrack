import { useEffect, useState } from 'react'
import { descargarArchivo, llamarApi } from '../api.js'
import { useSesion } from '../SesionContext.jsx'
import Aviso from '../componentes/Aviso.jsx'
import Boton from '../componentes/Boton.jsx'
import Cargando from '../componentes/Cargando.jsx'
import Tarjeta from '../componentes/Tarjeta.jsx'

// Tamaño máximo que acepta la API para un archivo: 10 MB.
const TAMANO_MAXIMO = 10 * 1024 * 1024

// Convierte los bytes que manda la API a KB o MB para mostrarlos.
function formatearTamano(bytes) {
  const kb = bytes / 1024

  if (kb < 1024) {
    return `${Math.round(kb * 10) / 10} KB`
  }

  return `${Math.round((kb / 1024) * 10) / 10} MB`
}

// Fecha y hora en formato corto y legible.
function formatearFecha(fecha) {
  return new Date(fecha).toLocaleString('es', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

// Revisa el archivo antes de mandarlo. Devuelve el mensaje de error o ''.
function validarArchivo(archivo) {
  if (!archivo) {
    return 'Debes elegir un archivo.'
  }

  if (archivo.size === 0) {
    return 'El archivo está vacío.'
  }

  if (archivo.size > TAMANO_MAXIMO) {
    return 'El archivo supera el tamaño máximo de 10 MB.'
  }

  return ''
}

// Pantalla de documentos: subir, listar, descargar y eliminar archivos.
function Documentos() {
  const { usuario } = useSesion()
  const esAdmin = usuario?.rol === 'Administrador'

  const [cargando, setCargando] = useState(true)
  const [documentos, setDocumentos] = useState([])
  const [errorCarga, setErrorCarga] = useState('')

  // Mensajes de las acciones sobre la lista.
  const [errorAccion, setErrorAccion] = useState('')
  const [mensajeAccion, setMensajeAccion] = useState('')

  // Archivo elegido y estado de la subida.
  const [archivo, setArchivo] = useState(null)
  const [arrastrando, setArrastrando] = useState(false)
  const [errorArchivo, setErrorArchivo] = useState('')
  const [errorSubida, setErrorSubida] = useState('')
  const [mensajeSubida, setMensajeSubida] = useState('')
  const [subiendo, setSubiendo] = useState(false)

  // Id del documento que se está descargando o eliminando.
  const [procesandoId, setProcesandoId] = useState(null)

  // Pide la lista a la API. Se usa al entrar y después de cada acción.
  async function cargar() {
    try {
      const datos = await llamarApi('/api/documentos')
      setDocumentos(datos)
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

  // Se usa al soltar un archivo o al elegirlo con el botón.
  function elegirArchivo(nuevo) {
    setErrorArchivo('')
    setErrorSubida('')
    setMensajeSubida('')

    const problema = validarArchivo(nuevo)

    if (problema) {
      setArchivo(null)
      setErrorArchivo(problema)
      return
    }

    setArchivo(nuevo)
  }

  function alElegir(evento) {
    const nuevo = evento.target.files?.[0]
    elegirArchivo(nuevo)

    // Se limpia el input para que se pueda elegir el mismo archivo otra vez.
    evento.target.value = ''
  }

  function alArrastrarSobre(evento) {
    evento.preventDefault()
    setArrastrando(true)
  }

  function alSalirArrastre(evento) {
    evento.preventDefault()
    setArrastrando(false)
  }

  function alSoltar(evento) {
    evento.preventDefault()
    setArrastrando(false)
    elegirArchivo(evento.dataTransfer.files?.[0])
  }

  async function subir(evento) {
    evento.preventDefault()

    const problema = validarArchivo(archivo)

    if (problema) {
      setErrorArchivo(problema)
      return
    }

    setSubiendo(true)
    setErrorSubida('')
    setMensajeSubida('')

    // El archivo viaja como form-data en el campo "archivo".
    const formulario = new FormData()
    formulario.append('archivo', archivo)

    try {
      const datos = await llamarApi('/api/documentos', { method: 'POST', body: formulario })
      setMensajeSubida(datos.mensaje)
      setArchivo(null)
      setErrorArchivo('')
      await cargar()
    } catch (e) {
      setErrorSubida(e.message)
    } finally {
      setSubiendo(false)
    }
  }

  async function descargar(documento) {
    setErrorAccion('')
    setMensajeAccion('')
    setProcesandoId(documento.id)

    try {
      await descargarArchivo(`/api/documentos/${documento.id}/descargar`, documento.nombre)
    } catch (e) {
      setErrorAccion(e.message)
    } finally {
      setProcesandoId(null)
    }
  }

  async function eliminar(documento) {
    const confirmado = window.confirm(`¿Eliminar el documento «${documento.nombre}»?`)
    if (!confirmado) return

    setErrorAccion('')
    setMensajeAccion('')
    setProcesandoId(documento.id)

    try {
      const datos = await llamarApi(`/api/documentos/${documento.id}`, { method: 'DELETE' })
      setMensajeAccion(datos.mensaje)
      await cargar()
    } catch (e) {
      setErrorAccion(e.message)
    } finally {
      setProcesandoId(null)
    }
  }

  if (cargando) return <Cargando />

  if (errorCarga) {
    return (
      <div className="mx-auto max-w-3xl space-y-4">
        <h1 className="text-2xl font-semibold">Documentos</h1>
        <Aviso>{errorCarga}</Aviso>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold">Documentos</h1>

      {errorAccion && <Aviso>{errorAccion}</Aviso>}
      {mensajeAccion && <Aviso tipo="exito">{mensajeAccion}</Aviso>}

      <Tarjeta className="space-y-4">
        <div>
          <h2 className="text-lg font-semibold">Subir documento</h2>
          <p className="mt-1 text-sm text-zinc-500 dark:text-zinc-400">
            Arrastra un archivo aquí o elígelo con el botón. Máximo 10 MB.
          </p>
        </div>

        {errorSubida && <Aviso>{errorSubida}</Aviso>}
        {mensajeSubida && <Aviso tipo="exito">{mensajeSubida}</Aviso>}

        <form onSubmit={subir} className="space-y-3">
          <div
            onDragOver={alArrastrarSobre}
            onDragEnter={alArrastrarSobre}
            onDragLeave={alSalirArrastre}
            onDrop={alSoltar}
            className={`flex flex-col items-center justify-center gap-3 rounded-xl border-2 border-dashed px-4 py-10 text-center transition ${
              arrastrando
                ? 'border-emerald-500 bg-emerald-50 dark:border-emerald-400 dark:bg-emerald-950'
                : 'border-zinc-300 dark:border-zinc-700'
            }`}
          >
            <p className="text-sm text-zinc-600 dark:text-zinc-300">
              {archivo ? archivo.name : 'Suelta el archivo en esta zona'}
            </p>

            {archivo && (
              <p className="text-xs text-zinc-500 dark:text-zinc-400">
                {formatearTamano(archivo.size)}
              </p>
            )}

            <label
              htmlFor="archivo-documento"
              className="cursor-pointer rounded-lg border border-zinc-300 px-4 py-2 text-sm font-medium text-zinc-600 transition hover:border-emerald-500 hover:text-emerald-600 dark:border-zinc-700 dark:text-zinc-300 dark:hover:border-emerald-400 dark:hover:text-emerald-400"
            >
              Elegir archivo
            </label>
            <input
              id="archivo-documento"
              type="file"
              onChange={alElegir}
              className="hidden"
            />
          </div>

          {errorArchivo && <Aviso>{errorArchivo}</Aviso>}

          <div className="flex justify-end">
            <Boton tipo="submit" cargando={subiendo} disabled={!archivo || subiendo}>
              {subiendo ? 'Subiendo...' : 'Subir documento'}
            </Boton>
          </div>
        </form>
      </Tarjeta>

      <Tarjeta>
        {documentos.length === 0 ? (
          <p className="py-6 text-sm text-zinc-500 dark:text-zinc-400">
            No hay documentos guardados todavía. Sube el primero con la zona de arriba.
          </p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full min-w-[640px] text-left text-sm">
              <thead>
                <tr className="border-b border-zinc-200 text-xs uppercase text-zinc-500 dark:border-zinc-800 dark:text-zinc-400">
                  <th className="py-2 pr-3 font-medium">Nombre</th>
                  <th className="py-2 pr-3 font-medium">Tipo</th>
                  <th className="py-2 pr-3 font-medium">Tamaño</th>
                  <th className="py-2 pr-3 font-medium">Fecha</th>
                  {esAdmin && <th className="py-2 pr-3 font-medium">Usuario ID</th>}
                  <th className="py-2 font-medium">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-zinc-100 dark:divide-zinc-800">
                {documentos.map((documento) => (
                  <tr key={documento.id}>
                    <td className="py-3 pr-3 font-medium">{documento.nombre}</td>
                    <td className="py-3 pr-3 text-xs text-zinc-500 dark:text-zinc-400">
                      {documento.tipo || 'Desconocido'}
                    </td>
                    <td className="py-3 pr-3 text-zinc-500 dark:text-zinc-400">
                      {formatearTamano(documento.tamano)}
                    </td>
                    <td className="py-3 pr-3 text-zinc-500 dark:text-zinc-400">
                      {formatearFecha(documento.fecha)}
                    </td>
                    {esAdmin && (
                      <td className="py-3 pr-3 text-zinc-500 dark:text-zinc-400">
                        {documento.usuarioId}
                      </td>
                    )}
                    <td className="py-3">
                      <div className="flex flex-wrap gap-2">
                        <button
                          type="button"
                          onClick={() => descargar(documento)}
                          disabled={procesandoId !== null}
                          className="rounded-lg border border-zinc-300 px-3 py-1 text-xs font-medium text-zinc-600 transition hover:border-emerald-500 hover:text-emerald-600 disabled:cursor-not-allowed disabled:opacity-50 dark:border-zinc-700 dark:text-zinc-300 dark:hover:border-emerald-400 dark:hover:text-emerald-400"
                        >
                          Descargar
                        </button>
                        <button
                          type="button"
                          onClick={() => eliminar(documento)}
                          disabled={procesandoId !== null}
                          className="rounded-lg border border-zinc-300 px-3 py-1 text-xs font-medium text-red-600 transition hover:border-red-400 disabled:cursor-not-allowed disabled:opacity-50 dark:border-zinc-700 dark:text-red-400 dark:hover:border-red-500"
                        >
                          Eliminar
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Tarjeta>
    </div>
  )
}

export default Documentos

import { useEffect, useState } from 'react'
import { llamarApi } from '../api.js'
import { useSesion } from '../SesionContext.jsx'
import Aviso from '../componentes/Aviso.jsx'
import Campo from '../componentes/Campo.jsx'
import Cargando from '../componentes/Cargando.jsx'
import Tarjeta from '../componentes/Tarjeta.jsx'

// Nombre legible del rol que manda la API.
function nombreRol(rol) {
  return rol === 'Administrador' ? 'Administrador' : 'Estándar'
}

// Pantalla de administración: lista de usuarios y acciones sobre cada uno.
function Usuarios() {
  const { usuario: usuarioSesion } = useSesion()

  const [cargando, setCargando] = useState(true)
  const [usuarios, setUsuarios] = useState([])
  const [errorCarga, setErrorCarga] = useState('')
  const [busqueda, setBusqueda] = useState('')

  const [errorAccion, setErrorAccion] = useState('')
  const [mensajeAccion, setMensajeAccion] = useState('')
  const [procesandoId, setProcesandoId] = useState(null)

  // Pide la lista a la API. Se usa al entrar y después de cada acción.
  async function cargar() {
    try {
      const datos = await llamarApi('/api/usuarios')
      setUsuarios(datos)
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

  async function cambiarRol(usuario, nuevoRol) {
    if (nuevoRol === usuario.rol) return

    const confirmado = window.confirm(
      `¿Cambiar el rol de ${usuario.correo} a ${nombreRol(nuevoRol)}?`,
    )
    if (!confirmado) {
      // Fuerza un render para que el selector vuelva a mostrar el rol real.
      setUsuarios((actuales) => [...actuales])
      return
    }

    setErrorAccion('')
    setMensajeAccion('')
    setProcesandoId(usuario.id)

    try {
      const datos = await llamarApi(`/api/usuarios/${usuario.id}/rol`, {
        method: 'PUT',
        body: { rol: nuevoRol },
      })
      setMensajeAccion(datos.mensaje)
    } catch (e) {
      setErrorAccion(e.message)
    } finally {
      // Siempre se recarga: así el selector muestra el rol real.
      await cargar()
      setProcesandoId(null)
    }
  }

  async function cambiarEstado(usuario) {
    const activar = !usuario.activo

    // Desactivar es destructivo: se cierran las sesiones del usuario.
    if (!activar) {
      const confirmado = window.confirm(
        `¿Desactivar la cuenta de ${usuario.correo}? Se cerrarán sus sesiones.`,
      )
      if (!confirmado) return
    }

    setErrorAccion('')
    setMensajeAccion('')
    setProcesandoId(usuario.id)

    try {
      const datos = await llamarApi(
        `/api/usuarios/${usuario.id}/${activar ? 'reactivar' : 'desactivar'}`,
        { method: 'POST' },
      )
      setMensajeAccion(datos.mensaje)
      await cargar()
    } catch (e) {
      setErrorAccion(e.message)
    } finally {
      setProcesandoId(null)
    }
  }

  async function forzarRestablecimiento(usuario) {
    const confirmado = window.confirm(
      `¿Forzar el restablecimiento de la contraseña de ${usuario.correo}? Se cerrarán sus sesiones y se enviará un código a su correo.`,
    )
    if (!confirmado) return

    setErrorAccion('')
    setMensajeAccion('')
    setProcesandoId(usuario.id)

    try {
      const datos = await llamarApi(`/api/usuarios/${usuario.id}/forzar-restablecimiento`, {
        method: 'POST',
      })
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
        <h1 className="text-2xl font-semibold">Usuarios</h1>
        <Aviso>{errorCarga}</Aviso>
      </div>
    )
  }

  // Filtro por correo que se aplica mientras se escribe.
  const texto = busqueda.trim().toLowerCase()
  const filtrados = usuarios.filter((u) => u.correo.toLowerCase().includes(texto))

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold">Usuarios</h1>

      {errorAccion && <Aviso>{errorAccion}</Aviso>}
      {mensajeAccion && <Aviso tipo="exito">{mensajeAccion}</Aviso>}

      <Tarjeta>
        <Campo
          etiqueta="Buscar por correo"
          placeholder="Escribe para filtrar..."
          value={busqueda}
          onChange={(e) => setBusqueda(e.target.value)}
        />

        {usuarios.length === 0 ? (
          <p className="py-6 text-sm text-zinc-500 dark:text-zinc-400">
            No hay usuarios registrados.
          </p>
        ) : filtrados.length === 0 ? (
          <p className="py-6 text-sm text-zinc-500 dark:text-zinc-400">
            Ningún usuario coincide con «{busqueda}».
          </p>
        ) : (
          <div className="mt-4 overflow-x-auto">
            <table className="w-full min-w-[760px] text-left text-sm">
              <thead>
                <tr className="border-b border-zinc-200 text-xs uppercase text-zinc-500 dark:border-zinc-800 dark:text-zinc-400">
                  <th className="py-2 pr-3 font-medium">ID</th>
                  <th className="py-2 pr-3 font-medium">Correo</th>
                  <th className="py-2 pr-3 font-medium">Rol</th>
                  <th className="py-2 pr-3 font-medium">Estado</th>
                  <th className="py-2 font-medium">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-zinc-100 dark:divide-zinc-800">
                {filtrados.map((usuario) => {
                  const esPropiaCuenta = usuario.id === usuarioSesion?.id
                  const ocupado = procesandoId !== null

                  return (
                    <tr key={usuario.id}>
                      <td className="py-3 pr-3 text-zinc-500 dark:text-zinc-400">{usuario.id}</td>
                      <td className="py-3 pr-3">
                        <span className="font-medium">{usuario.correo}</span>
                        {esPropiaCuenta && (
                          <span className="ml-2 rounded-full bg-zinc-100 px-2 py-0.5 text-xs font-medium text-zinc-600 dark:bg-zinc-800 dark:text-zinc-300">
                            Tu cuenta
                          </span>
                        )}
                      </td>
                      <td className="py-3 pr-3">
                        <select
                          value={usuario.rol}
                          onChange={(e) => cambiarRol(usuario, e.target.value)}
                          disabled={esPropiaCuenta || ocupado}
                          title={
                            esPropiaCuenta ? 'No puedes cambiar tu propio rol.' : undefined
                          }
                          className="rounded-lg border border-zinc-300 bg-white px-2 py-1 text-sm outline-none transition focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/30 disabled:cursor-not-allowed disabled:opacity-50 dark:border-zinc-700 dark:bg-zinc-900"
                        >
                          <option value="Estandar">Estándar</option>
                          <option value="Administrador">Administrador</option>
                        </select>
                      </td>
                      <td className="py-3 pr-3">
                        <span
                          className={`rounded-full px-2 py-0.5 text-xs font-medium ${
                            usuario.activo
                              ? 'bg-emerald-50 text-emerald-700 dark:bg-emerald-950 dark:text-emerald-300'
                              : 'bg-zinc-100 text-zinc-600 dark:bg-zinc-800 dark:text-zinc-300'
                          }`}
                        >
                          {usuario.activo ? 'Activo' : 'Inactivo'}
                        </span>
                      </td>
                      <td className="py-3">
                        <div className="flex flex-wrap gap-2">
                          <button
                            type="button"
                            onClick={() => cambiarEstado(usuario)}
                            disabled={esPropiaCuenta || ocupado}
                            title={
                              esPropiaCuenta ? 'No puedes modificar tu propia cuenta.' : undefined
                            }
                            className={`rounded-lg border border-zinc-300 px-3 py-1 text-xs font-medium transition disabled:cursor-not-allowed disabled:opacity-50 dark:border-zinc-700 ${
                              usuario.activo
                                ? 'text-red-600 hover:border-red-400 dark:text-red-400 dark:hover:border-red-500'
                                : 'text-emerald-700 hover:border-emerald-500 dark:text-emerald-400 dark:hover:border-emerald-400'
                            }`}
                          >
                            {usuario.activo ? 'Desactivar' : 'Reactivar'}
                          </button>

                          <button
                            type="button"
                            onClick={() => forzarRestablecimiento(usuario)}
                            disabled={esPropiaCuenta || ocupado}
                            title={
                              esPropiaCuenta ? 'No puedes modificar tu propia cuenta.' : undefined
                            }
                            className="rounded-lg border border-zinc-300 px-3 py-1 text-xs font-medium text-red-600 transition hover:border-red-400 disabled:cursor-not-allowed disabled:opacity-50 dark:border-zinc-700 dark:text-red-400 dark:hover:border-red-500"
                          >
                            Forzar restablecimiento
                          </button>
                        </div>
                      </td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
          </div>
        )}
      </Tarjeta>
    </div>
  )
}

export default Usuarios

import { useEffect, useState } from 'react'
import { llamarApi } from '../api.js'
import { useSesion } from '../SesionContext.jsx'
import Aviso from '../componentes/Aviso.jsx'
import Boton from '../componentes/Boton.jsx'
import Campo from '../componentes/Campo.jsx'
import Cargando from '../componentes/Cargando.jsx'
import Selector from '../componentes/Selector.jsx'
import Tarjeta from '../componentes/Tarjeta.jsx'

// Redondea a un decimal solo para mostrar los valores.
function formatear(valor) {
  return Math.round(valor * 10) / 10
}

// Pantalla de alimentos: lista, buscador, alta, edición, borrado e importación desde USDA.
function Alimentos() {
  const { usuario } = useSesion()
  const esAdmin = usuario?.rol === 'Administrador'

  const [cargando, setCargando] = useState(true)
  const [alimentos, setAlimentos] = useState([])
  const [errorCarga, setErrorCarga] = useState('')
  const [busqueda, setBusqueda] = useState('')

  // Mensajes de las acciones sobre la lista.
  const [errorAccion, setErrorAccion] = useState('')
  const [mensajeAccion, setMensajeAccion] = useState('')

  // Estado del formulario dentro del modal.
  const [mostrarModal, setMostrarModal] = useState(false)
  const [editando, setEditando] = useState(null)
  const [nombre, setNombre] = useState('')
  const [calorias, setCalorias] = useState('')
  const [proteina, setProteina] = useState('')
  const [carbohidratos, setCarbohidratos] = useState('')
  const [grasas, setGrasas] = useState('')
  const [tipo, setTipo] = useState('personalizado')
  const [errorModal, setErrorModal] = useState('')
  const [guardando, setGuardando] = useState(false)

  // Estado de la importación desde USDA.
  const [nombreImportar, setNombreImportar] = useState('')
  const [errorImportar, setErrorImportar] = useState('')
  const [mensajeImportar, setMensajeImportar] = useState('')
  const [importando, setImportando] = useState(false)

  // Pide la lista a la API. Se usa al entrar y después de cada acción.
  async function cargar() {
    try {
      const datos = await llamarApi('/api/alimentos')
      setAlimentos(datos)
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

  // Un Estándar solo edita los propios; un Administrador, los básicos y los propios.
  function puedeEditar(alimento) {
    return esAdmin || alimento.personalizado
  }

  function abrirNuevo() {
    setEditando(null)
    setNombre('')
    setCalorias('')
    setProteina('')
    setCarbohidratos('')
    setGrasas('')
    setTipo('personalizado')
    setErrorModal('')
    setMostrarModal(true)
  }

  function abrirEditar(alimento) {
    setEditando(alimento)
    setNombre(alimento.nombre)
    setCalorias(String(alimento.caloriasPor100g))
    setProteina(String(alimento.proteinaPor100g))
    setCarbohidratos(String(alimento.carbohidratosPor100g))
    setGrasas(String(alimento.grasasPor100g))
    setTipo(alimento.personalizado ? 'personalizado' : 'basico')
    setErrorModal('')
    setMostrarModal(true)
  }

  function cerrarModal() {
    setMostrarModal(false)
    setEditando(null)
    setErrorModal('')
  }

  // Validación en vivo de los números: vacío no marca error, pero no deja guardar.
  const errorCalorias =
    calorias !== '' && !(Number(calorias) >= 0) ? 'Escribe un número mayor o igual a 0.' : ''
  const errorProteina =
    proteina !== '' && !(Number(proteina) >= 0) ? 'Escribe un número mayor o igual a 0.' : ''
  const errorCarbohidratos =
    carbohidratos !== '' && !(Number(carbohidratos) >= 0)
      ? 'Escribe un número mayor o igual a 0.'
      : ''
  const errorGrasas =
    grasas !== '' && !(Number(grasas) >= 0) ? 'Escribe un número mayor o igual a 0.' : ''

  const completo =
    nombre.trim() !== '' &&
    calorias !== '' &&
    proteina !== '' &&
    carbohidratos !== '' &&
    grasas !== ''

  const puedeGuardar =
    completo &&
    !errorCalorias &&
    !errorProteina &&
    !errorCarbohidratos &&
    !errorGrasas &&
    !guardando

  async function guardar(evento) {
    evento.preventDefault()
    setErrorModal('')
    setGuardando(true)

    const cuerpo = {
      nombre: nombre.trim(),
      caloriasPor100g: Number(calorias),
      proteinaPor100g: Number(proteina),
      carbohidratosPor100g: Number(carbohidratos),
      grasasPor100g: Number(grasas),
    }

    try {
      let datos

      if (editando) {
        datos = await llamarApi(`/api/alimentos/${editando.id}`, { method: 'PUT', body: cuerpo })
      } else if (esAdmin && tipo === 'basico') {
        datos = await llamarApi('/api/alimentos', { method: 'POST', body: cuerpo })
      } else {
        datos = await llamarApi('/api/alimentos/personalizado', { method: 'POST', body: cuerpo })
      }

      setMostrarModal(false)
      setEditando(null)
      setErrorAccion('')
      setMensajeAccion(datos.mensaje)
      await cargar()
    } catch (e) {
      setErrorModal(e.message)
    } finally {
      setGuardando(false)
    }
  }

  async function borrar(alimento) {
    const confirmado = window.confirm(`¿Borrar el alimento «${alimento.nombre}»?`)
    if (!confirmado) return

    setErrorAccion('')
    setMensajeAccion('')

    try {
      const datos = await llamarApi(`/api/alimentos/${alimento.id}`, { method: 'DELETE' })
      setMensajeAccion(datos.mensaje)
      await cargar()
    } catch (e) {
      // Aquí aparece el mensaje de la API si el alimento ya se usó en comidas.
      setErrorAccion(e.message)
    }
  }

  async function importar(evento) {
    evento.preventDefault()
    setErrorImportar('')
    setMensajeImportar('')
    setImportando(true)

    try {
      const datos = await llamarApi('/api/alimentos/importar', {
        method: 'POST',
        body: { nombre: nombreImportar },
      })

      setMensajeImportar(datos.mensaje)
      setNombreImportar('')
      await cargar()
    } catch (e) {
      setErrorImportar(e.message)
    } finally {
      setImportando(false)
    }
  }

  if (cargando) return <Cargando />

  if (errorCarga) {
    return (
      <div className="mx-auto max-w-3xl space-y-4">
        <h1 className="text-2xl font-semibold">Alimentos</h1>
        <Aviso>{errorCarga}</Aviso>
      </div>
    )
  }

  // Filtro por nombre que se aplica mientras se escribe.
  const texto = busqueda.trim().toLowerCase()
  const filtrados = alimentos.filter((a) => a.nombre.toLowerCase().includes(texto))

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-semibold">Alimentos</h1>
        <Boton onClick={abrirNuevo}>Nuevo alimento</Boton>
      </div>

      {errorAccion && <Aviso>{errorAccion}</Aviso>}
      {mensajeAccion && <Aviso tipo="exito">{mensajeAccion}</Aviso>}

      <Tarjeta>
        <Campo
          etiqueta="Buscar por nombre"
          placeholder="Escribe para filtrar..."
          value={busqueda}
          onChange={(e) => setBusqueda(e.target.value)}
        />

        {alimentos.length === 0 ? (
          <p className="py-6 text-sm text-zinc-500 dark:text-zinc-400">
            No hay alimentos guardados todavía. Crea uno con «Nuevo alimento».
          </p>
        ) : filtrados.length === 0 ? (
          <p className="py-6 text-sm text-zinc-500 dark:text-zinc-400">
            Ningún alimento coincide con «{busqueda}».
          </p>
        ) : (
          <ul className="mt-4 divide-y divide-zinc-200 dark:divide-zinc-800">
            {filtrados.map((alimento) => (
              <li key={alimento.id} className="py-4">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <div className="flex flex-wrap items-center gap-2">
                      <p className="font-medium">{alimento.nombre}</p>
                      <span
                        className={`rounded-full px-2 py-0.5 text-xs font-medium ${
                          alimento.personalizado
                            ? 'bg-emerald-50 text-emerald-700 dark:bg-emerald-950 dark:text-emerald-300'
                            : 'bg-zinc-100 text-zinc-600 dark:bg-zinc-800 dark:text-zinc-300'
                        }`}
                      >
                        {alimento.personalizado ? 'Propio' : 'Básico'}
                      </span>
                    </div>
                    <p className="mt-1 text-xs text-zinc-500 dark:text-zinc-400">
                      Por 100 g: {formatear(alimento.caloriasPor100g)} kcal ·{' '}
                      {formatear(alimento.proteinaPor100g)} g proteína ·{' '}
                      {formatear(alimento.carbohidratosPor100g)} g carbohidratos ·{' '}
                      {formatear(alimento.grasasPor100g)} g grasas
                    </p>
                  </div>

                  {puedeEditar(alimento) && (
                    <div className="flex gap-2">
                      <button
                        type="button"
                        onClick={() => abrirEditar(alimento)}
                        className="rounded-lg border border-zinc-300 px-3 py-1 text-xs font-medium text-zinc-600 transition hover:border-emerald-500 hover:text-emerald-600 dark:border-zinc-700 dark:text-zinc-300 dark:hover:border-emerald-400 dark:hover:text-emerald-400"
                      >
                        Editar
                      </button>
                      <button
                        type="button"
                        onClick={() => borrar(alimento)}
                        className="rounded-lg border border-zinc-300 px-3 py-1 text-xs font-medium text-red-600 transition hover:border-red-400 dark:border-zinc-700 dark:text-red-400 dark:hover:border-red-500"
                      >
                        Borrar
                      </button>
                    </div>
                  )}
                </div>
              </li>
            ))}
          </ul>
        )}
      </Tarjeta>

      <Tarjeta className="space-y-3">
        <h2 className="text-lg font-semibold">Importar desde USDA</h2>
        <p className="text-sm text-zinc-500 dark:text-zinc-400">
          Escribe el nombre del alimento en inglés, ej. banana.
        </p>

        {errorImportar && <Aviso>{errorImportar}</Aviso>}
        {mensajeImportar && <Aviso tipo="exito">{mensajeImportar}</Aviso>}

        <form onSubmit={importar} className="flex flex-col gap-3 sm:flex-row sm:items-end">
          <div className="flex-1">
            <Campo
              etiqueta="Nombre del alimento"
              placeholder="banana"
              value={nombreImportar}
              onChange={(e) => setNombreImportar(e.target.value)}
            />
          </div>
          <Boton
            tipo="submit"
            cargando={importando}
            disabled={nombreImportar.trim() === '' || importando}
          >
            {importando ? 'Importando...' : 'Importar'}
          </Boton>
        </form>
      </Tarjeta>

      {mostrarModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <div className="max-h-[90vh] w-full max-w-lg overflow-y-auto rounded-xl border border-zinc-200 bg-white p-5 shadow-lg dark:border-zinc-800 dark:bg-zinc-900">
            <div className="mb-4 flex items-center justify-between gap-3">
              <h2 className="text-lg font-semibold">
                {editando ? 'Editar alimento' : 'Nuevo alimento'}
              </h2>
              <button
                type="button"
                onClick={cerrarModal}
                className="text-sm text-zinc-500 transition hover:text-emerald-600 dark:text-zinc-400 dark:hover:text-emerald-400"
              >
                Cerrar
              </button>
            </div>

            <form onSubmit={guardar} className="space-y-4" noValidate>
              {errorModal && <Aviso>{errorModal}</Aviso>}

              <Campo
                etiqueta="Nombre"
                value={nombre}
                onChange={(e) => setNombre(e.target.value)}
                autoFocus
              />

              {!editando && esAdmin && (
                <Selector etiqueta="Tipo" value={tipo} onChange={(e) => setTipo(e.target.value)}>
                  <option value="personalizado">Propio (solo lo ves tú)</option>
                  <option value="basico">Básico (lo ven todos)</option>
                </Selector>
              )}

              {editando && (
                <p className="text-xs text-zinc-500 dark:text-zinc-400">
                  Tipo: {editando.personalizado ? 'Propio' : 'Básico'}. No se puede cambiar.
                </p>
              )}

              <div className="grid gap-4 sm:grid-cols-2">
                <Campo
                  etiqueta="Calorías por 100 g (kcal)"
                  tipo="number"
                  min="0"
                  step="0.1"
                  value={calorias}
                  onChange={(e) => setCalorias(e.target.value)}
                  error={errorCalorias}
                />
                <Campo
                  etiqueta="Proteína por 100 g (g)"
                  tipo="number"
                  min="0"
                  step="0.1"
                  value={proteina}
                  onChange={(e) => setProteina(e.target.value)}
                  error={errorProteina}
                />
                <Campo
                  etiqueta="Carbohidratos por 100 g (g)"
                  tipo="number"
                  min="0"
                  step="0.1"
                  value={carbohidratos}
                  onChange={(e) => setCarbohidratos(e.target.value)}
                  error={errorCarbohidratos}
                />
                <Campo
                  etiqueta="Grasas por 100 g (g)"
                  tipo="number"
                  min="0"
                  step="0.1"
                  value={grasas}
                  onChange={(e) => setGrasas(e.target.value)}
                  error={errorGrasas}
                />
              </div>

              <div className="flex justify-end gap-2">
                <button
                  type="button"
                  onClick={cerrarModal}
                  className="rounded-lg border border-zinc-300 px-4 py-2 text-sm font-medium text-zinc-600 transition hover:border-zinc-400 dark:border-zinc-700 dark:text-zinc-300 dark:hover:border-zinc-600"
                >
                  Cancelar
                </button>
                <Boton tipo="submit" cargando={guardando} disabled={!puedeGuardar}>
                  Guardar
                </Boton>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  )
}

export default Alimentos

import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { Cell, Pie, PieChart, ResponsiveContainer } from 'recharts'
import { llamarApi } from '../api.js'
import Aviso from '../componentes/Aviso.jsx'
import BarraProgreso from '../componentes/BarraProgreso.jsx'
import Boton from '../componentes/Boton.jsx'
import Campo from '../componentes/Campo.jsx'
import Cargando from '../componentes/Cargando.jsx'
import EtiquetaEstado from '../componentes/EtiquetaEstado.jsx'
import Selector from '../componentes/Selector.jsx'
import Tarjeta from '../componentes/Tarjeta.jsx'

// Redondea a un decimal solo para mostrar los valores.
function formatear(valor) {
  return Math.round(valor * 10) / 10
}

// Pantalla principal: resumen del día, gráficos y registro de comidas.
function Dashboard() {
  const [cargando, setCargando] = useState(true)
  const [dia, setDia] = useState(null)
  const [noHayDia, setNoHayDia] = useState(false)
  const [errorCarga, setErrorCarga] = useState('')

  // Alimentos para el selector y la vista previa.
  const [alimentos, setAlimentos] = useState([])
  const [errorAlimentos, setErrorAlimentos] = useState('')

  // Formulario para registrar una comida.
  const [busquedaAlimento, setBusquedaAlimento] = useState('')
  const [alimentoId, setAlimentoId] = useState('')
  const [gramos, setGramos] = useState('')
  const [errorComida, setErrorComida] = useState('')
  const [registrando, setRegistrando] = useState(false)

  // Mensajes generales y cierre del día.
  const [mensaje, setMensaje] = useState('')
  const [errorCierre, setErrorCierre] = useState('')
  const [cerrando, setCerrando] = useState(false)

  // Pide el resumen de hoy. El 404 es el estado vacío, no un error.
  async function cargarHoy() {
    try {
      const datos = await llamarApi('/api/resumen/hoy')
      setDia(datos)
      setNoHayDia(false)
      setErrorCarga('')
    } catch (e) {
      if (e.estado === 404) {
        setDia(null)
        setNoHayDia(true)
        setErrorCarga('')
      } else {
        setErrorCarga(e.message)
      }
    }
  }

  async function cargarAlimentos() {
    try {
      const datos = await llamarApi('/api/alimentos')
      setAlimentos(datos)
      setErrorAlimentos('')
    } catch (e) {
      setErrorAlimentos(e.message)
    }
  }

  useEffect(() => {
    async function iniciar() {
      await Promise.all([cargarHoy(), cargarAlimentos()])
      setCargando(false)
    }

    iniciar()
  }, [])

  const cerrado = dia?.estado === 'Cerrado'

  // Filtro del selector de alimentos mientras se escribe.
  const textoBusqueda = busquedaAlimento.trim().toLowerCase()
  const alimentosFiltrados = alimentos.filter((a) =>
    a.nombre.toLowerCase().includes(textoBusqueda),
  )
  const alimentoElegido = alimentos.find((a) => String(a.id) === alimentoId)
  const gramosNumero = Number(gramos)
  const hayPorcion = Boolean(alimentoElegido) && gramos !== '' && gramosNumero > 0

  async function registrar(evento) {
    evento.preventDefault()
    if (!alimentoElegido || !(gramosNumero > 0)) return

    setErrorComida('')
    setMensaje('')
    setRegistrando(true)

    try {
      const datos = await llamarApi('/api/resumen/comida', {
        method: 'POST',
        body: { alimentoId: alimentoElegido.id, gramos: gramosNumero },
      })

      setMensaje(datos.mensaje)
      setBusquedaAlimento('')
      setAlimentoId('')
      setGramos('')
      await cargarHoy()
    } catch (e) {
      setErrorComida(e.message)
    } finally {
      setRegistrando(false)
    }
  }

  async function cerrarDia() {
    const confirmado = window.confirm(
      '¿Cerrar el día de hoy? No podrás registrar más comidas.',
    )
    if (!confirmado) return

    setErrorCierre('')
    setMensaje('')
    setCerrando(true)

    try {
      const datos = await llamarApi('/api/resumen/cerrar', { method: 'POST' })
      setMensaje(datos.mensaje)
      await cargarHoy()
    } catch (e) {
      setErrorCierre(e.message)
    } finally {
      setCerrando(false)
    }
  }

  if (cargando) return <Cargando />

  if (errorCarga) {
    return (
      <div className="mx-auto max-w-3xl space-y-4">
        <h1 className="text-2xl font-semibold">Resumen de hoy</h1>
        <Aviso>{errorCarga}</Aviso>
      </div>
    )
  }

  // Datos del gráfico de anillo de calorías.
  const consumidoCalorias = dia ? dia.consumido.calorias : 0
  const metaCalorias = dia ? dia.metas.calorias : 0
  const restanteCalorias = metaCalorias - consumidoCalorias
  const excedidoCalorias = consumidoCalorias > metaCalorias
  const colorCalorias = excedidoCalorias ? '#dc2626' : '#059669'
  const caloriasEnAnillo = Math.min(consumidoCalorias, metaCalorias)
  const datosAnillo = [
    { valor: caloriasEnAnillo },
    { valor: Math.max(metaCalorias - caloriasEnAnillo, 0) },
  ]

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex flex-wrap items-center gap-3">
          <h1 className="text-2xl font-semibold">Resumen de hoy</h1>
          {dia && <EtiquetaEstado estado={dia.estado} />}
        </div>

        <Boton
          onClick={cerrarDia}
          cargando={cerrando}
          disabled={!dia || cerrado || cerrando}
          title={!dia ? 'Registra una comida antes de cerrar el día.' : undefined}
        >
          Cerrar día
        </Boton>
      </div>

      {mensaje && <Aviso tipo="exito">{mensaje}</Aviso>}
      {errorCierre && <Aviso>{errorCierre}</Aviso>}

      {noHayDia && (
        <Tarjeta>
          <p className="text-sm text-zinc-500 dark:text-zinc-400">
            Hoy no has registrado comidas.
          </p>
        </Tarjeta>
      )}

      {dia && (
        <div className="grid gap-6 lg:grid-cols-2">
          <Tarjeta>
            <h2 className="text-lg font-semibold">Calorías</h2>

            <div className="relative mx-auto mt-2 h-56 w-full max-w-xs">
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={datosAnillo}
                    dataKey="valor"
                    innerRadius="68%"
                    outerRadius="90%"
                    startAngle={90}
                    endAngle={-270}
                    stroke="none"
                  >
                    <Cell fill={colorCalorias} />
                    <Cell fill="#e4e4e7" />
                  </Pie>
                </PieChart>
              </ResponsiveContainer>

              <div className="pointer-events-none absolute inset-0 flex flex-col items-center justify-center">
                <p
                  className={`text-3xl font-semibold ${
                    excedidoCalorias
                      ? 'text-red-600 dark:text-red-400'
                      : 'text-emerald-600 dark:text-emerald-400'
                  }`}
                >
                  {formatear(Math.abs(restanteCalorias))}
                </p>
                <p className="text-xs text-zinc-500 dark:text-zinc-400">
                  {excedidoCalorias ? 'kcal de más' : 'kcal restantes'}
                </p>
              </div>
            </div>

            <p className="text-center text-sm text-zinc-500 dark:text-zinc-400">
              {formatear(consumidoCalorias)} de {formatear(metaCalorias)} kcal
            </p>
          </Tarjeta>

          <Tarjeta className="space-y-5">
            <h2 className="text-lg font-semibold">Macronutrientes</h2>
            <BarraProgreso
              etiqueta="Proteínas"
              consumido={dia.consumido.proteinas}
              meta={dia.metas.proteinas}
              unidad="g"
            />
            <BarraProgreso
              etiqueta="Carbohidratos"
              consumido={dia.consumido.carbohidratos}
              meta={dia.metas.carbohidratos}
              unidad="g"
            />
            <BarraProgreso
              etiqueta="Grasas"
              consumido={dia.consumido.grasas}
              meta={dia.metas.grasas}
              unidad="g"
            />
          </Tarjeta>
        </div>
      )}

      <div className="grid gap-6 lg:grid-cols-2">
        <Tarjeta>
          <h2 className="mb-3 text-lg font-semibold">Registrar comida</h2>

          {cerrado && (
            <div className="mb-4">
              <Aviso>El día está cerrado. No se pueden registrar más comidas.</Aviso>
            </div>
          )}

          <form onSubmit={registrar} className="space-y-4" noValidate>
            {errorAlimentos && <Aviso>{errorAlimentos}</Aviso>}

            {errorComida && (
              <Aviso>
                {errorComida}
                {errorComida.toLowerCase().includes('perfil') && (
                  <Link to="/perfil" className="ml-1 font-medium underline">
                    Crear mi perfil
                  </Link>
                )}
              </Aviso>
            )}

            <Campo
              etiqueta="Buscar alimento"
              placeholder="Escribe el nombre..."
              value={busquedaAlimento}
              onChange={(e) => {
                setBusquedaAlimento(e.target.value)
                setAlimentoId('')
              }}
              disabled={cerrado}
            />

            <Selector
              etiqueta="Alimento"
              value={alimentoId}
              onChange={(e) => setAlimentoId(e.target.value)}
              disabled={cerrado}
            >
              <option value="">Selecciona un alimento</option>
              {alimentosFiltrados.map((a) => (
                <option key={a.id} value={a.id}>
                  {a.nombre}
                </option>
              ))}
            </Selector>

            {!errorAlimentos && alimentosFiltrados.length === 0 && (
              <p className="text-xs text-zinc-500 dark:text-zinc-400">
                No hay alimentos que coincidan con la búsqueda.
              </p>
            )}

            <Campo
              etiqueta="Gramos"
              tipo="number"
              min="0"
              step="1"
              placeholder="100"
              value={gramos}
              onChange={(e) => setGramos(e.target.value)}
              disabled={cerrado}
            />

            <div className="rounded-lg border border-zinc-200 bg-zinc-50 p-3 text-sm dark:border-zinc-800 dark:bg-zinc-950">
              <p className="mb-1 text-xs font-medium text-zinc-500 dark:text-zinc-400">
                Vista previa de la porción
              </p>
              {hayPorcion ? (
                <p className="text-zinc-700 dark:text-zinc-300">
                  {formatear((alimentoElegido.caloriasPor100g * gramosNumero) / 100)} kcal ·{' '}
                  {formatear((alimentoElegido.proteinaPor100g * gramosNumero) / 100)} g proteína ·{' '}
                  {formatear((alimentoElegido.carbohidratosPor100g * gramosNumero) / 100)} g
                  carbohidratos ·{' '}
                  {formatear((alimentoElegido.grasasPor100g * gramosNumero) / 100)} g grasas
                </p>
              ) : (
                <p className="text-zinc-500 dark:text-zinc-400">
                  Elige un alimento y escribe los gramos.
                </p>
              )}
            </div>

            <Boton
              tipo="submit"
              cargando={registrando}
              disabled={cerrado || !hayPorcion || registrando}
              className="w-full"
            >
              {registrando ? 'Registrando...' : 'Registrar comida'}
            </Boton>
          </form>
        </Tarjeta>

        <Tarjeta>
          <h2 className="mb-3 text-lg font-semibold">Comidas de hoy</h2>

          {!dia || dia.comidas.length === 0 ? (
            <p className="text-sm text-zinc-500 dark:text-zinc-400">
              Hoy no has registrado comidas.
            </p>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full min-w-[520px] text-left text-sm">
                <thead>
                  <tr className="border-b border-zinc-200 text-xs uppercase text-zinc-500 dark:border-zinc-800 dark:text-zinc-400">
                    <th className="py-2 pr-3 font-medium">Alimento</th>
                    <th className="py-2 pr-3 font-medium">Gramos</th>
                    <th className="py-2 pr-3 font-medium">kcal</th>
                    <th className="py-2 pr-3 font-medium">Prot.</th>
                    <th className="py-2 pr-3 font-medium">Carb.</th>
                    <th className="py-2 font-medium">Grasas</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-zinc-100 dark:divide-zinc-800">
                  {dia.comidas.map((comida, indice) => (
                    <tr key={indice}>
                      <td className="py-2 pr-3">{comida.alimento}</td>
                      <td className="py-2 pr-3">{formatear(comida.gramos)}</td>
                      <td className="py-2 pr-3">{formatear(comida.calorias)}</td>
                      <td className="py-2 pr-3">{formatear(comida.proteinas)}</td>
                      <td className="py-2 pr-3">{formatear(comida.carbohidratos)}</td>
                      <td className="py-2">{formatear(comida.grasas)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </Tarjeta>
      </div>
    </div>
  )
}

export default Dashboard

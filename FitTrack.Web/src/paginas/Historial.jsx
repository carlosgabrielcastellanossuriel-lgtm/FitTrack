import { useEffect, useState } from 'react'
import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import { llamarApi } from '../api.js'
import Aviso from '../componentes/Aviso.jsx'
import Boton from '../componentes/Boton.jsx'
import Cargando from '../componentes/Cargando.jsx'
import EtiquetaEstado from '../componentes/EtiquetaEstado.jsx'
import Tarjeta from '../componentes/Tarjeta.jsx'

// Redondea a un decimal solo para mostrar los valores.
function formatear(valor) {
  return Math.round(valor * 10) / 10
}

// La API manda la fecha como "AAAA-MM-DD". Se muestra como DD/MM/AAAA.
function fechaCompleta(fecha) {
  return new Date(`${fecha}T00:00:00`).toLocaleDateString('es', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
  })
}

// Versión corta para las etiquetas del gráfico.
function fechaCorta(fecha) {
  return new Date(`${fecha}T00:00:00`).toLocaleDateString('es', {
    day: '2-digit',
    month: '2-digit',
  })
}

// Pantalla con las calorías por día y el detalle de cada resumen.
function Historial() {
  const [cargando, setCargando] = useState(true)
  const [historial, setHistorial] = useState([])
  const [sinPermiso, setSinPermiso] = useState(false)
  const [errorCarga, setErrorCarga] = useState('')

  // Solicitud del permiso VerEstadisticas.
  const [mensajePermiso, setMensajePermiso] = useState('')
  const [errorPermiso, setErrorPermiso] = useState('')
  const [solicitando, setSolicitando] = useState(false)

  async function cargar() {
    try {
      const datos = await llamarApi('/api/resumen/historial')
      setHistorial(datos)
      setSinPermiso(false)
      setErrorCarga('')
    } catch (e) {
      // El 403 significa que falta el permiso, no que la pantalla esté rota.
      if (e.estado === 403) {
        setSinPermiso(true)
        setErrorCarga('')
      } else {
        setErrorCarga(e.message)
      }
    } finally {
      setCargando(false)
    }
  }

  useEffect(() => {
    cargar()
  }, [])

  async function solicitarPermiso() {
    setMensajePermiso('')
    setErrorPermiso('')
    setSolicitando(true)

    try {
      const datos = await llamarApi('/api/permisos', {
        method: 'POST',
        body: { permiso: 'VerEstadisticas' },
      })

      setMensajePermiso(datos.mensaje)
    } catch (e) {
      setErrorPermiso(e.message)
    } finally {
      setSolicitando(false)
    }
  }

  if (cargando) return <Cargando />

  if (errorCarga) {
    return (
      <div className="mx-auto max-w-3xl space-y-4">
        <h1 className="text-2xl font-semibold">Historial</h1>
        <Aviso>{errorCarga}</Aviso>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold">Historial</h1>

      {sinPermiso ? (
        <Tarjeta className="space-y-4">
          <h2 className="text-lg font-semibold">Necesitas el permiso Ver estadísticas</h2>
          <p className="text-sm text-zinc-500 dark:text-zinc-400">
            El historial de tus días está protegido por el permiso VerEstadisticas. Puedes
            solicitarlo y un administrador lo revisará.
          </p>

          {mensajePermiso && <Aviso tipo="exito">{mensajePermiso}</Aviso>}
          {errorPermiso && <Aviso>{errorPermiso}</Aviso>}

          <Boton onClick={solicitarPermiso} cargando={solicitando} disabled={solicitando}>
            {solicitando ? 'Enviando...' : 'Solicitar permiso'}
          </Boton>
        </Tarjeta>
      ) : historial.length === 0 ? (
        <Tarjeta>
          <p className="text-sm text-zinc-500 dark:text-zinc-400">
            Aún no hay días registrados en tu historial.
          </p>
        </Tarjeta>
      ) : (
        <>
          <Tarjeta>
            <h2 className="text-lg font-semibold">Calorías por día</h2>

            <div className="mt-4 h-72 w-full">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={historial} margin={{ top: 8, right: 8, bottom: 0, left: 0 }}>
                  <CartesianGrid stroke="#e4e4e7" strokeDasharray="3 3" vertical={false} />
                  <XAxis
                    dataKey="fecha"
                    tickFormatter={fechaCorta}
                    tick={{ fill: '#71717a', fontSize: 12 }}
                    axisLine={false}
                    tickLine={false}
                  />
                  <YAxis
                    tick={{ fill: '#71717a', fontSize: 12 }}
                    axisLine={false}
                    tickLine={false}
                    width={44}
                  />
                  <Tooltip
                    formatter={(valor) => [`${formatear(valor)} kcal`, 'Calorías']}
                    labelFormatter={fechaCompleta}
                  />
                  <Bar dataKey="calorias" fill="#059669" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </Tarjeta>

          <Tarjeta>
            <h2 className="mb-3 text-lg font-semibold">Días registrados</h2>

            <div className="overflow-x-auto">
              <table className="w-full min-w-[640px] text-left text-sm">
                <thead>
                  <tr className="border-b border-zinc-200 text-xs uppercase text-zinc-500 dark:border-zinc-800 dark:text-zinc-400">
                    <th className="py-2 pr-3 font-medium">Fecha</th>
                    <th className="py-2 pr-3 font-medium">Calorías (kcal)</th>
                    <th className="py-2 pr-3 font-medium">Proteínas (g)</th>
                    <th className="py-2 pr-3 font-medium">Carbohidratos (g)</th>
                    <th className="py-2 pr-3 font-medium">Grasas (g)</th>
                    <th className="py-2 font-medium">Estado</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-zinc-100 dark:divide-zinc-800">
                  {historial.map((dia, indice) => (
                    <tr key={indice}>
                      <td className="py-2 pr-3">{fechaCompleta(dia.fecha)}</td>
                      <td className="py-2 pr-3">{formatear(dia.calorias)}</td>
                      <td className="py-2 pr-3">{formatear(dia.proteinas)}</td>
                      <td className="py-2 pr-3">{formatear(dia.carbohidratos)}</td>
                      <td className="py-2 pr-3">{formatear(dia.grasas)}</td>
                      <td className="py-2">
                        <EtiquetaEstado estado={dia.estado} />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </Tarjeta>
        </>
      )}
    </div>
  )
}

export default Historial

import { useEffect, useState } from 'react'
import { llamarApi } from '../api.js'
import Aviso from '../componentes/Aviso.jsx'
import Boton from '../componentes/Boton.jsx'
import Campo from '../componentes/Campo.jsx'
import Cargando from '../componentes/Cargando.jsx'
import Selector from '../componentes/Selector.jsx'
import Tarjeta from '../componentes/Tarjeta.jsx'

// Descripción corta de cada nivel de actividad.
const DESCRIPCIONES_ACTIVIDAD = {
  Sedentario: 'Casi no haces ejercicio (trabajo de escritorio).',
  Ligero: 'Ejercicio suave de 1 a 3 días por semana.',
  Moderado: 'Ejercicio de 3 a 5 días por semana.',
  Alto: 'Ejercicio intenso de 6 a 7 días por semana.',
}

// Pantalla para ver y editar el perfil, con las metas calculadas.
function Perfil() {
  const [cargando, setCargando] = useState(true)
  const [perfil, setPerfil] = useState(null)
  const [errorCarga, setErrorCarga] = useState('')
  const [mensajeInicial, setMensajeInicial] = useState('')

  const [edad, setEdad] = useState('')
  const [peso, setPeso] = useState('')
  const [pies, setPies] = useState('')
  const [pulgadas, setPulgadas] = useState('')
  const [sexo, setSexo] = useState('Masculino')
  const [nivelActividad, setNivelActividad] = useState('Sedentario')
  const [objetivo, setObjetivo] = useState('Mantenimiento')

  const [error, setError] = useState('')
  const [mensaje, setMensaje] = useState('')
  const [guardando, setGuardando] = useState(false)

  // Al cargar un perfil existente se separa la altura en pies y pulgadas.
  function llenarFormulario(datos) {
    const piesCargados = Math.floor(datos.alturaPulgadas / 12)
    const pulgadasCargadas = Math.round((datos.alturaPulgadas - piesCargados * 12) * 10) / 10

    setEdad(String(datos.edad))
    setPeso(String(datos.pesoLibras))
    setPies(String(piesCargados))
    setPulgadas(String(pulgadasCargadas))
    setSexo(datos.sexo)
    setNivelActividad(datos.nivelActividad)
    setObjetivo(datos.objetivo)
  }

  useEffect(() => {
    async function cargar() {
      try {
        const datos = await llamarApi('/api/perfil')
        setPerfil(datos)
        llenarFormulario(datos)
      } catch (e) {
        // Sin perfil: la API responde 404 y se muestra el formulario vacío.
        if (e.estado === 404) {
          setMensajeInicial('Crea tu perfil para calcular tus metas')
        } else {
          setErrorCarga(e.message)
        }
      } finally {
        setCargando(false)
      }
    }

    cargar()
  }, [])

  // Validación en vivo de los rangos que acepta la API.
  const edadNumero = Number(edad)
  const pesoNumero = Number(peso)
  const alturaTotal = Number(pies) * 12 + Number(pulgadas)

  const errorEdad =
    edad !== '' && !(edadNumero >= 15 && edadNumero <= 100)
      ? 'La edad debe estar entre 15 y 100 años.'
      : ''
  const errorPeso =
    peso !== '' && !(pesoNumero >= 66 && pesoNumero <= 660)
      ? 'El peso debe estar entre 66 y 660 libras.'
      : ''
  const errorPulgadas =
    pulgadas !== '' && !(Number(pulgadas) >= 0 && Number(pulgadas) < 12)
      ? 'Las pulgadas deben ser de 0 a 11.'
      : ''
  const errorAltura =
    pies !== '' && pulgadas !== '' && !(alturaTotal >= 39 && alturaTotal <= 98)
      ? 'La altura debe estar entre 39 y 98 pulgadas en total.'
      : ''

  const completo = edad !== '' && peso !== '' && pies !== '' && pulgadas !== ''
  const puedeGuardar =
    completo && !errorEdad && !errorPeso && !errorPulgadas && !errorAltura && !guardando

  async function guardar(evento) {
    evento.preventDefault()
    setError('')
    setMensaje('')
    setGuardando(true)

    try {
      const datos = await llamarApi('/api/perfil', {
        method: 'PUT',
        body: {
          edad: Number(edad),
          peso: Number(peso),
          altura: alturaTotal,
          sexo,
          nivelActividad,
          objetivo,
        },
      })

      setPerfil(datos)
      setMensajeInicial('')
      setMensaje('Perfil guardado.')
    } catch (e) {
      setError(e.message)
    } finally {
      setGuardando(false)
    }
  }

  if (cargando) return <Cargando />

  if (errorCarga) {
    return (
      <div className="mx-auto max-w-3xl space-y-4">
        <h1 className="text-2xl font-semibold">Perfil</h1>
        <Aviso>{errorCarga}</Aviso>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-semibold">Perfil</h1>

      {mensajeInicial && (
        <p className="rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-800 dark:border-emerald-900 dark:bg-emerald-950 dark:text-emerald-300">
          {mensajeInicial}
        </p>
      )}

      <div className="grid gap-6 lg:grid-cols-2">
        <Tarjeta>
          <form onSubmit={guardar} className="space-y-4" noValidate>
            {error && <Aviso>{error}</Aviso>}
            {mensaje && <Aviso tipo="exito">{mensaje}</Aviso>}

            <div className="grid gap-4 sm:grid-cols-2">
              <Campo
                etiqueta="Edad"
                tipo="number"
                min="15"
                max="100"
                step="1"
                value={edad}
                onChange={(e) => setEdad(e.target.value)}
                error={errorEdad}
              />

              <Campo
                etiqueta="Peso (libras)"
                tipo="number"
                min="66"
                max="660"
                step="0.1"
                value={peso}
                onChange={(e) => setPeso(e.target.value)}
                error={errorPeso}
              />
            </div>

            <div>
              <p className="mb-1 text-sm font-medium text-zinc-700 dark:text-zinc-300">Altura</p>
              <div className="grid grid-cols-2 gap-4">
                <Campo
                  etiqueta="Pies"
                  tipo="number"
                  min="0"
                  max="8"
                  step="1"
                  value={pies}
                  onChange={(e) => setPies(e.target.value)}
                />

                <Campo
                  etiqueta="Pulgadas"
                  tipo="number"
                  min="0"
                  max="11"
                  step="0.1"
                  value={pulgadas}
                  onChange={(e) => setPulgadas(e.target.value)}
                  error={errorPulgadas}
                />
              </div>
              <p className="mt-1 text-xs text-zinc-500 dark:text-zinc-400">
                Entre 39 y 98 pulgadas en total.
              </p>
              {errorAltura && (
                <p className="mt-1 text-xs text-red-600 dark:text-red-400">{errorAltura}</p>
              )}
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <Selector etiqueta="Sexo" value={sexo} onChange={(e) => setSexo(e.target.value)}>
                <option value="Masculino">Masculino</option>
                <option value="Femenino">Femenino</option>
              </Selector>

              <Selector
                etiqueta="Objetivo"
                value={objetivo}
                onChange={(e) => setObjetivo(e.target.value)}
              >
                <option value="Deficit">Déficit (bajar de peso)</option>
                <option value="Mantenimiento">Mantenimiento</option>
                <option value="Superavit">Superávit (subir de peso)</option>
              </Selector>
            </div>

            <div>
              <Selector
                etiqueta="Nivel de actividad"
                value={nivelActividad}
                onChange={(e) => setNivelActividad(e.target.value)}
              >
                <option value="Sedentario">Sedentario: casi sin ejercicio</option>
                <option value="Ligero">Ligero: ejercicio 1 a 3 días por semana</option>
                <option value="Moderado">Moderado: ejercicio 3 a 5 días por semana</option>
                <option value="Alto">Alto: ejercicio intenso 6 a 7 días por semana</option>
              </Selector>
              <p className="mt-1 text-xs text-zinc-500 dark:text-zinc-400">
                {DESCRIPCIONES_ACTIVIDAD[nivelActividad]}
              </p>
            </div>

            <Boton tipo="submit" cargando={guardando} disabled={!puedeGuardar} className="w-full">
              {guardando ? 'Guardando...' : 'Guardar perfil'}
            </Boton>
          </form>
        </Tarjeta>

        <div className="space-y-4">
          <h2 className="text-lg font-semibold">Metas diarias</h2>

          {perfil ? (
            <div
              key={`${perfil.metas.calorias}-${perfil.metas.proteinaGramos}-${perfil.metas.carbohidratosGramos}-${perfil.metas.grasasGramos}`}
              className="aparecer grid gap-4 sm:grid-cols-2"
            >
              <Tarjeta className="text-center">
                <p className="text-sm text-zinc-500 dark:text-zinc-400">Calorías</p>
                <p className="mt-1 text-3xl font-semibold text-emerald-600 dark:text-emerald-400">
                  {perfil.metas.calorias}
                </p>
                <p className="text-xs text-zinc-500 dark:text-zinc-400">kcal al día</p>
              </Tarjeta>

              <Tarjeta className="text-center">
                <p className="text-sm text-zinc-500 dark:text-zinc-400">Proteína</p>
                <p className="mt-1 text-3xl font-semibold text-emerald-600 dark:text-emerald-400">
                  {perfil.metas.proteinaGramos}
                </p>
                <p className="text-xs text-zinc-500 dark:text-zinc-400">g al día</p>
              </Tarjeta>

              <Tarjeta className="text-center">
                <p className="text-sm text-zinc-500 dark:text-zinc-400">Carbohidratos</p>
                <p className="mt-1 text-3xl font-semibold text-emerald-600 dark:text-emerald-400">
                  {perfil.metas.carbohidratosGramos}
                </p>
                <p className="text-xs text-zinc-500 dark:text-zinc-400">g al día</p>
              </Tarjeta>

              <Tarjeta className="text-center">
                <p className="text-sm text-zinc-500 dark:text-zinc-400">Grasas</p>
                <p className="mt-1 text-3xl font-semibold text-emerald-600 dark:text-emerald-400">
                  {perfil.metas.grasasGramos}
                </p>
                <p className="text-xs text-zinc-500 dark:text-zinc-400">g al día</p>
              </Tarjeta>
            </div>
          ) : (
            <Tarjeta>
              <p className="text-sm text-zinc-500 dark:text-zinc-400">
                Aún no hay metas calculadas.
              </p>
            </Tarjeta>
          )}
        </div>
      </div>
    </div>
  )
}

export default Perfil

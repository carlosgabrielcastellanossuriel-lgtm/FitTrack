// Etiqueta con el color de cada estado del resumen diario. Se reutiliza en otras pantallas.
const ESTADOS = {
  EnProgreso: {
    texto: 'En progreso',
    clases: 'bg-zinc-100 text-zinc-600 dark:bg-zinc-800 dark:text-zinc-300',
  },
  DentroDeMeta: {
    texto: 'Dentro de meta',
    clases: 'bg-emerald-50 text-emerald-700 dark:bg-emerald-950 dark:text-emerald-300',
  },
  Excedido: {
    texto: 'Excedido',
    clases: 'bg-red-50 text-red-700 dark:bg-red-950 dark:text-red-300',
  },
  Cerrado: {
    texto: 'Cerrado',
    clases: 'bg-blue-50 text-blue-700 dark:bg-blue-950 dark:text-blue-300',
  },
}

function EtiquetaEstado({ estado }) {
  const datos = ESTADOS[estado] || {
    texto: estado,
    clases: 'bg-zinc-100 text-zinc-600 dark:bg-zinc-800 dark:text-zinc-300',
  }

  return (
    <span className={`rounded-full px-2.5 py-0.5 text-xs font-medium ${datos.clases}`}>
      {datos.texto}
    </span>
  )
}

export default EtiquetaEstado

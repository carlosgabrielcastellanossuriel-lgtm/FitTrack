// Indicador simple de carga.
function Cargando({ texto = 'Cargando...' }) {
  return (
    <div className="flex items-center gap-2 py-6 text-sm text-zinc-500 dark:text-zinc-400">
      <span className="h-4 w-4 animate-spin rounded-full border-2 border-zinc-300 border-t-emerald-600 dark:border-zinc-700 dark:border-t-emerald-400" />
      {texto}
    </div>
  )
}

export default Cargando

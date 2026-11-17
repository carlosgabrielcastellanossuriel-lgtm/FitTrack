// Mensaje corto de error o de éxito.
function Aviso({ tipo = 'error', children }) {
  const clases =
    tipo === 'exito'
      ? 'border-emerald-200 bg-emerald-50 text-emerald-800 dark:border-emerald-900 dark:bg-emerald-950 dark:text-emerald-300'
      : 'border-red-200 bg-red-50 text-red-800 dark:border-red-900 dark:bg-red-950 dark:text-red-300'

  return <div className={`rounded-lg border px-3 py-2 text-sm ${clases}`}>{children}</div>
}

export default Aviso

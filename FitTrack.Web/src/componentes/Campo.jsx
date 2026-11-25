// Campo de formulario: etiqueta + input.
// Si llega "error", el borde se pone rojo y se muestra el mensaje debajo.
function Campo({ etiqueta, tipo = 'text', error = '', ...props }) {
  const borde = error
    ? 'border-red-400 focus:border-red-500 focus:ring-red-500/30 dark:border-red-700'
    : 'border-zinc-300 focus:border-emerald-500 focus:ring-emerald-500/30 dark:border-zinc-700'

  return (
    <label className="block">
      <span className="mb-1 block text-sm font-medium text-zinc-700 dark:text-zinc-300">
        {etiqueta}
      </span>
      <input
        type={tipo}
        className={`w-full rounded-lg border bg-white px-3 py-2 text-sm outline-none transition focus:ring-2 dark:bg-zinc-900 ${borde}`}
        {...props}
      />
      {error && <span className="mt-1 block text-xs text-red-600 dark:text-red-400">{error}</span>}
    </label>
  )
}

export default Campo

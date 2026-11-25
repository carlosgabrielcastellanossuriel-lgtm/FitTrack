// Botón simple con el color de acento.
// Mientras "cargando" es true muestra un spinner y se deshabilita.
function Boton({ children, tipo = 'button', cargando = false, className = '', disabled, ...props }) {
  return (
    <button
      type={tipo}
      disabled={disabled || cargando}
      className={`inline-flex items-center justify-center gap-2 rounded-lg bg-emerald-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-50 dark:bg-emerald-500 dark:hover:bg-emerald-400 ${className}`}
      {...props}
    >
      {cargando && (
        <span className="h-4 w-4 animate-spin rounded-full border-2 border-white/40 border-t-white" />
      )}
      {children}
    </button>
  )
}

export default Boton

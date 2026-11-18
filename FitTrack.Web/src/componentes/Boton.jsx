// Botón simple con el color de acento.
function Boton({ children, tipo = 'button', ...props }) {
  return (
    <button
      type={tipo}
      className="rounded-lg bg-emerald-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-50 dark:bg-emerald-500 dark:hover:bg-emerald-400"
      {...props}
    >
      {children}
    </button>
  )
}

export default Boton

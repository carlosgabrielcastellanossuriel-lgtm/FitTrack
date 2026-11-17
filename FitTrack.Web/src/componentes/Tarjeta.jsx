// Contenedor con borde fino, esquinas redondeadas y sombra suave.
function Tarjeta({ children, className = '' }) {
  return (
    <div
      className={`rounded-xl border border-zinc-200 bg-white p-4 shadow-sm dark:border-zinc-800 dark:bg-zinc-900 ${className}`}
    >
      {children}
    </div>
  )
}

export default Tarjeta

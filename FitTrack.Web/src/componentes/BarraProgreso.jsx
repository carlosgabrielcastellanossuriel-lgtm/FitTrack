// Redondea a un decimal solo para mostrar los valores.
function formatear(valor) {
  return Math.round(valor * 10) / 10
}

// Barra de progreso de un nutriente: consumido / meta y lo que queda.
function BarraProgreso({ etiqueta, consumido, meta, unidad }) {
  const excedido = consumido > meta
  // La barra se llena hasta el 100% aunque se haya pasado de la meta.
  const porcentaje = meta > 0 ? Math.min((consumido / meta) * 100, 100) : 0
  const restante = meta - consumido

  return (
    <div>
      <div className="mb-1 flex items-baseline justify-between gap-2 text-sm">
        <span className="font-medium text-zinc-700 dark:text-zinc-300">{etiqueta}</span>
        <span className="text-zinc-500 dark:text-zinc-400">
          {formatear(consumido)} / {formatear(meta)} {unidad}
        </span>
      </div>

      <div className="h-2.5 w-full overflow-hidden rounded-full bg-zinc-200 dark:bg-zinc-800">
        <div
          className={`h-full rounded-full transition-all duration-500 ${
            excedido ? 'bg-red-500' : 'bg-emerald-500'
          }`}
          style={{ width: `${porcentaje}%` }}
        />
      </div>

      <p
        className={`mt-1 text-xs ${
          excedido ? 'text-red-600 dark:text-red-400' : 'text-zinc-500 dark:text-zinc-400'
        }`}
      >
        {excedido
          ? `Te pasaste por ${formatear(Math.abs(restante))} ${unidad}`
          : `Quedan ${formatear(restante)} ${unidad}`}
      </p>
    </div>
  )
}

export default BarraProgreso

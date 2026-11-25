// Selector con etiqueta, para formularios.
function Selector({ etiqueta, children, ...props }) {
  return (
    <label className="block">
      <span className="mb-1 block text-sm font-medium text-zinc-700 dark:text-zinc-300">
        {etiqueta}
      </span>
      <select
        className="w-full rounded-lg border border-zinc-300 bg-white px-3 py-2 text-sm outline-none transition focus:border-emerald-500 focus:ring-2 focus:ring-emerald-500/30 dark:border-zinc-700 dark:bg-zinc-900"
        {...props}
      >
        {children}
      </select>
    </label>
  )
}

export default Selector

# Reglas del frontend FitTrack
- Stack: React + Vite + Tailwind v4 + React Router + Recharts. JavaScript, sin TypeScript.
- Código simple: componentes funcionales planos, una página por archivo en src/paginas y componentes pequeños en src/componentes. Solo useState, useEffect y un contexto para la sesión. Nada de Redux, Zustand, React Query ni abstracciones hasta que sean necesarias. Legibilidad antes que sofisticación.
- Nombres y comentarios en español simples. Textos de la interfaz en español.
- Toda llamada a la API pasa por src/api.js. La API usa enums como texto, peso en libras y altura en pulgadas. Para ver cuerpos y respuestas exactos, lee el controlador en FitTrack.Api/Controllers.
- Los errores de la API vienen como { mensaje }: muéstralos con Aviso. Toda pantalla tiene estado de carga, estado vacío y manejo de error.
- Estilo minimalista: fondo blanco o zinc claro, un solo color de acento (emerald), bordes finos, esquinas redondeadas, sombras suaves, mucho espacio en blanco, transiciones suaves. Modo oscuro automático (dark: de Tailwind). Responsive.
- Entorno: Windows con PowerShell (usa `;`, no `&&`).
- Git: nunca trabajar en main; cada tarea empieza creando su rama. No hagas commit ni push.
- Al terminar: ejecuta `npm run build` en FitTrack.Web, lista los archivos creados o modificados y explica cómo probarlo con comandos de PowerShell.

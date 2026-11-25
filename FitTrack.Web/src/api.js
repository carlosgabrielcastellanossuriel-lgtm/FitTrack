// URL base de la API. Se puede cambiar con VITE_API_URL en el archivo .env.
const URL_BASE = import.meta.env.VITE_API_URL || 'http://localhost:5262'

// Llama a la API y devuelve el JSON de la respuesta.
// Agrega el token guardado y redirige a /login si la sesión venció.
export async function llamarApi(ruta, opciones = {}) {
  const token = localStorage.getItem('fittrack_token')

  const encabezados = { ...(opciones.headers || {}) }
  if (token) encabezados['Authorization'] = `Bearer ${token}`

  // Si hay cuerpo y no es FormData, se envía como JSON.
  let cuerpo = opciones.body
  if (cuerpo !== undefined && cuerpo !== null && !(cuerpo instanceof FormData)) {
    encabezados['Content-Type'] = 'application/json'
    if (typeof cuerpo !== 'string') cuerpo = JSON.stringify(cuerpo)
  }

  const respuesta = await fetch(`${URL_BASE}${ruta}`, {
    ...opciones,
    headers: encabezados,
    body: cuerpo,
  })

  // Se lee el texto y se intenta convertir a JSON.
  const texto = await respuesta.text()
  let datos = null
  if (texto) {
    try {
      datos = JSON.parse(texto)
    } catch {
      datos = texto
    }
  }

  // Si la respuesta falla, se lanza un error con el mensaje de la API.
  if (!respuesta.ok) {
    if (respuesta.status === 401 && token) {
      localStorage.removeItem('fittrack_token')
      window.location.href = '/login'
    }

    const error = new Error(datos?.mensaje || 'Ocurrió un error al llamar a la API.')
    error.estado = respuesta.status
    throw error
  }

  return datos
}

// Descarga un archivo de la API. El endpoint exige el token, así que no sirve
// un enlace normal: se pide con fetch, se guarda como blob y se dispara la descarga.
export async function descargarArchivo(ruta, nombre) {
  const token = localStorage.getItem('fittrack_token')

  const respuesta = await fetch(`${URL_BASE}${ruta}`, {
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  })

  // Si falla, se intenta leer el { mensaje } que manda la API.
  if (!respuesta.ok) {
    if (respuesta.status === 401 && token) {
      localStorage.removeItem('fittrack_token')
      window.location.href = '/login'
    }

    let mensaje = 'No se pudo descargar el archivo.'
    try {
      const datos = await respuesta.json()
      if (datos?.mensaje) mensaje = datos.mensaje
    } catch {
      // Si la respuesta no es JSON, se usa el mensaje general.
    }

    throw new Error(mensaje)
  }

  const blob = await respuesta.blob()

  // Se crea un enlace temporal con el blob y se hace clic para descargarlo.
  const url = URL.createObjectURL(blob)
  const enlace = document.createElement('a')
  enlace.href = url
  enlace.download = nombre || 'documento'
  document.body.appendChild(enlace)
  enlace.click()
  enlace.remove()

  // Se libera la memoria del blob.
  URL.revokeObjectURL(url)
}

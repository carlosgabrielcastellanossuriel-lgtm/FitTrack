// Reglas de validación compartidas por las pantallas de autenticación.
const FORMATO_CORREO = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

export const MENSAJE_CORREO = 'Escribe un correo con un formato válido.'
export const MENSAJE_CLAVE = 'Mínimo 8 caracteres, con al menos una letra y un número.'

export function correoValido(correo) {
  return FORMATO_CORREO.test(correo.trim())
}

// La misma regla que usa la API: 8 caracteres, una letra y un número.
export function claveValida(clave) {
  return clave.length >= 8 && /[A-Za-z]/.test(clave) && /\d/.test(clave)
}

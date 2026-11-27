# Criterios de aceptación: notificaciones, cola de correos y reportes

Cada criterio se puede comprobar con Swagger o PowerShell sin preguntarle a nadie. Máximo de intentos de envío: 3.

## Notificaciones

### RF-NOT-01
- Dado un usuario Estándar con sesión y dos Administradores activos.
- Cuando el Estándar crea una solicitud (POST /api/permisos).
- Entonces cada Administrador activo tiene una notificación nueva sin leer y ningún Estándar la tiene.
- Comprobar: GET /api/notificaciones con el token de cada Administrador (aparece) y con el del Estándar (no aparece).

### RF-NOT-02
- Dado una solicitud pendiente.
- Cuando un Administrador la aprueba, o la rechaza con motivo.
- Entonces el solicitante tiene una notificación nueva que dice si fue aprobada o rechazada y, si fue rechazada, incluye el motivo. Nadie más la recibe.
- Comprobar: GET /api/notificaciones con el token del solicitante.

### RF-NOT-03
- Dado un usuario con perfil que registra una comida (POST /api/resumen/comida).
- Cuando esa comida hace que el día pase a Excedido.
- Entonces ese usuario tiene una notificación nueva "Día excedido". Si registra otra comida el mismo día no recibe una segunda. Nadie más la recibe.
- Comprobar: registrar una comida que supere la meta y consultar GET /api/notificaciones.

### RF-NOT-04
- Dado dos usuarios con notificaciones.
- Cuando uno consulta GET /api/notificaciones.
- Entonces solo ve las suyas, cada una indica si está leída, y con ?soloNoLeidas=true solo aparecen las no leídas. Sin sesión responde 401.
- Comprobar: consultar con el token de cada usuario y sin token.

### RF-NOT-05
- Dado una notificación del usuario A.
- Cuando A hace POST /api/notificaciones/{id}/leer.
- Entonces responde 200 y queda leída. Si el usuario B intenta con el mismo id responde 400 "Notificación no encontrada." y la notificación sigue sin leer.
- Comprobar: llamar con el token de A y luego con el de B.

### RF-NOT-06
- Dado un usuario con el correo activado (valor por defecto).
- Cuando recibe una notificación.
- Entonces aparece en GET /api/notificaciones y hay una fila Pendiente en CorreosEnCola dirigida a su correo; después de correr el Enviador pasa a Enviado.
- Comprobar: GET /api/correos como Administrador y docker compose logs enviador.

### RF-NOT-07
- Dado un usuario que hizo PUT /api/notificaciones/preferencias con { "correo": false }.
- Cuando recibe una notificación.
- Entonces aparece dentro del sistema y no se agrega ninguna fila a CorreosEnCola. GET /api/notificaciones/preferencias devuelve correo en false.
- Comprobar: desactivar, provocar una notificación y revisar GET /api/correos.

## Cola de correos

### RF-NOT-08
- Dado un servidor de correo apagado o inalcanzable.
- Cuando una operación origina un correo (por ejemplo crear una solicitud).
- Entonces la operación responde 200 y el correo queda Pendiente.
- Comprobar: poner un SMTP_HOST inexistente, ejecutar la operación y revisar GET /api/correos.

### RF-NOT-09
- Dado correos Pendientes en la cola.
- Cuando corre el Enviador, sin repetir la operación que los creó.
- Entonces los correos pasan a Enviado.
- Comprobar: dotnet run --project FitTrack.Enviador o docker compose logs enviador.

### RF-NOT-10
- Dado un servidor SMTP inalcanzable.
- Cuando el Enviador procesa el mismo correo en ejecuciones sucesivas.
- Entonces Intentos aumenta en cada ejecución y al llegar a 3 el estado pasa a Fallido con su último error; no se vuelve a intentar.
- Comprobar: GET /api/correos como Administrador después de varias ejecuciones.

### RF-NOT-11
- Dado correos en distintos estados.
- Cuando un Administrador consulta GET /api/correos.
- Entonces ve para cada correo id, destinatario, asunto, estado, intentos, fechaCreacion, fechaEnvio, fechaUltimoIntento y ultimoError (nunca el cuerpo). Un Estándar recibe 403.
- Comprobar: consultar con el token de un Administrador y con el de un Estándar.

### RF-NOT-12
- Dado correos ya Enviados.
- Cuando el Enviador corre dos veces seguidas.
- Entonces ningún correo Enviado se envía otra vez y la segunda ejecución informa "Correos enviados: 0".
- Comprobar: ejecutar el Enviador dos veces y ver la salida.

### RF-NOT-13
- Dado el repositorio completo y su historial.
- Cuando se buscan credenciales SMTP.
- Entonces solo existen como variables de entorno; .env.example trae únicamente valores de ejemplo.
- Comprobar: git grep -n "SMTP_CLAVE" y git log --all -p -S "SMTP_CLAVE=".

## Reportes

### RF-REP-01
- Dado solicitudes de permiso en varios estados.
- Cuando se consulta GET /api/reportes/solicitudes-permiso.
- Entonces devuelve una fila por estado con el total, no el listado de solicitudes.
- Comprobar: crear solicitudes, aprobar una, rechazar otra y consultar.

### RF-REP-02
- Dado días registrados con distintos estados.
- Cuando se consulta GET /api/reportes/dias-por-estado.
- Entonces devuelve por cada estado cuántos días hubo y el promedio de calorías. Responde: "¿cuántos días terminé dentro de la meta o excedido y con cuántas calorías en promedio?".
- Comprobar: consultar con un usuario que tenga días en más de un estado.

### RF-REP-03
- Dado más de un registro en la misma categoría.
- Cuando se consulta cualquiera de los dos reportes.
- Entonces el resultado tiene menos filas que los registros que resume, porque la agrupación se hace en la base de datos.
- Comprobar: comparar la cantidad de filas del reporte con la cantidad de registros.

### RF-REP-04
- Dado datos de dos usuarios Estándar y de un Administrador.
- Cuando un Estándar consulta cualquiera de los dos reportes.
- Entonces solo incluye sus propios datos. El Administrador ve los datos de todos. Sin sesión responde 401.
- Comprobar: consultar con el token de cada Estándar y con el del Administrador.

### RF-REP-05
- Dado los mismos datos.
- Cuando se consultan con rangos distintos (?desde=2026-11-01&hasta=2026-11-15 y ?desde=2026-11-16&hasta=2026-11-30).
- Entonces los resultados son distintos. Si desde es mayor que hasta responde 400.
- Comprobar: consultar ambos reportes con los dos rangos.

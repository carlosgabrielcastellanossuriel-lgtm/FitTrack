# Bitácora de delegación — Asignación 3

**Asignación:** Auditoría de notificaciones, cola de correos y reportes
**Curso:** Programación III (ITLA)
**Rama:** `auditoria/asignacion-3`
**Tag de entrega:** `asignacion-3`

La Práctica 5 se delegó con el archivo `docs/criterios-notificaciones-reportes.md` (commit `141e67b`) y las reglas de `AGENTS.md`. El agente entregó las notificaciones y la cola en el commit `5ed3cbe` y los reportes en el commit `815859c`. Esta auditoría revisó ese código contra los criterios, contra los requisitos RF-NOT/RF-REP y contra los casos borde que los criterios no cubrían.

---

## H-01 — El reporte de solicitudes falla con `hasta=9999-12-31`

| Campo | Contenido |
|---|---|
| **Hallazgo** | H-01. Reporte de solicitudes con la fecha máxima. Gravedad: **media**. |
| **Qué se pidió** | Implementar `GET /api/reportes/solicitudes-permiso` con filtros `desde` y `hasta`, incluyendo el día `hasta` completo (RF-REP-05). El criterio quedó en `docs/criterios-notificaciones-reportes.md` (commit `141e67b`) y la implementación en el commit `815859c`. |
| **Qué entregó el agente** | `ServicioReportes.SolicitudesPorEstadoAsync` calculaba el fin del rango con `hasta.Value.AddDays(1).ToDateTime(TimeOnly.MinValue)`. Con `hasta = 9999-12-31` (el máximo de `DateOnly`), `AddDays(1)` desborda y la petición termina en 500. |
| **Evidencia** | Prueba `H_01_HastaFechaMaxima_NoLanzaYIncluyeElDia` sobre el commit anterior a la corrección (`b375220^`): `System.ArgumentOutOfRangeException : Value to add was out of range. (Parameter 'value')`; `Failed: 1`. En el tag pasa. También se puede provocar por API con `GET /api/reportes/solicitudes-permiso?hasta=9999-12-31`: antes 500, después 200. |
| **Por qué pasó** | El criterio RF-REP-05 hablaba de rangos normales, pero no del límite máximo de `DateOnly`. La revisión de la Práctica 5 probó fechas de noviembre, no el borde. El agente eligió `AddDays(1)` porque era la forma más directa de incluir el último día, sin proteger el desbordamiento. |
| **Corrección** | La hizo el agente de ejecución (OpenCode) bajo mi revisión. Instrucción: “Corrige H-01: el filtro `hasta` debe incluir el día completo y no desbordar con `DateOnly.MaxValue`”. Commit `b375220`. |
| **Qué cambias** | Se agregó a `docs/criterios-notificaciones-reportes.md` el caso `hasta=9999-12-31` debe responder 200 y se agregó a `AGENTS.md` la regla de no usar `AddDays(1)` sin proteger el máximo. |

---

## H-02 — Un destinatario inválido se encola y se reintenta tres veces

| Campo | Contenido |
|---|---|
| **Hallazgo** | H-02. Correo con destinatario inválido en la cola. Gravedad: **media**. |
| **Qué se pidió** | Encolar un correo cuando el usuario tiene `NotificarPorCorreo = true` (RF-NOT-06) y reintentar como máximo 3 veces ante fallos de envío (RF-NOT-10). El criterio quedó en `docs/criterios-notificaciones-reportes.md` (commit `141e67b`) y la implementación en el commit `5ed3cbe`. |
| **Qué entregó el agente** | `ServicioColaCorreo.EncolarAsync` insertaba cualquier cadena como destinatario, sin validar. `ServicioNotificaciones.NotificarAsync` llamaba a la cola con `usuario.Correo`. El Enviador trataba el `FormatException` de `MailMessage` como un fallo SMTP normal: incrementaba `Intentos` y lo reintentaba hasta marcarlo `Fallido` en el tercer intento. |
| **Evidencia** | Prueba `H_02_NotificacionConCorreoInvalido_NoEncolaCorreo` sobre el commit anterior (`025d1c0^`): la notificación interna se guarda, pero `CorreosEnCola` queda con una fila; `Assert.Empty() Failure: Collection was not empty`; `Failed: 1`. En el tag la cola queda vacía. El código del Enviador (`FitTrack.Enviador/Program.cs`) muestra el `catch (Exception)` que incrementa `Intentos` sin distinguir un destinatario inválido de un SMTP caído. |
| **Por qué pasó** | El criterio no definía qué hacer con un destinatario mal formado. El agente asumió que todo correo que llega a la cola ya es válido y la revisión de la Práctica 5 no probó direcciones inválidas; el alta normal de usuarios sí valida el correo, pero la cola no. |
| **Corrección** | La hizo el agente de ejecución (OpenCode) bajo mi revisión. Instrucción: “Corrige H-02: valida el destinatario antes de encolar; si es inválido, no agregues la fila, pero conserva la notificación interna”. Commit `025d1c0`. |
| **Qué cambias** | Se agregó a `docs/criterios-notificaciones-reportes.md` el caso de destinatario inválido y se agregó a `AGENTS.md` la regla de validar con `Validaciones.CorreoValido` antes de encolar. |

---

## H-03 — La regla `desde <= hasta` vive solo en el controlador

| Campo | Contenido |
|---|---|
| **Hallazgo** | H-03. Validación del rango de fechas en la capa equivocada. Gravedad: **baja**. |
| **Qué se pidió** | Validar el rango de los reportes y devolver 400 si `desde` es mayor que `hasta` (RF-REP-05), manteniendo los controladores delgados y las reglas de negocio en los servicios (RD-02). El criterio quedó en `docs/criterios-notificaciones-reportes.md` (commit `141e67b`) y la implementación en el commit `815859c`. |
| **Qué entregó el agente** | `ReportesController` comprobaba `desde > hasta` en los dos endpoints, pero `ServicioReportes.SolicitudesPorEstadoAsync` y `ServicioResumen.DiasPorEstadoAsync` no. Llamados directamente con un rango invertido, devolvían una lista vacía, indistinguible de “no hay datos”. |
| **Evidencia** | Prueba `H_03_RangoInvertido_LosDosServiciosDeReporteLoRechazan` sobre el commit anterior (`53cac39^`): `Assert.Throws() Failure: No exception was thrown`; `Failed: 1`. En el tag ambos servicios lanzan `ArgumentException`. La API ya devolvía 400 por la comprobación del controlador, pero la regla no estaba en el servicio. |
| **Por qué pasó** | El criterio describía el comportamiento de la API (“si desde es mayor que hasta responde 400”), no la capa donde debía vivir la regla. El agente puso la comprobación en el controlador porque ahí se traduce a HTTP; la revisión solo probó por API y no llamó a los servicios directamente. |
| **Corrección** | La hizo el agente de ejecución (OpenCode) bajo mi revisión. Instrucción: “Corrige H-03: mueve la validación del rango a los dos servicios de reporte, no solo al controlador”. Commit `53cac39`. |
| **Qué cambias** | Se agregó a `docs/criterios-notificaciones-reportes.md` que la validación debe existir en el servicio y se agregó a `AGENTS.md` la regla de validar los rangos en el servicio. |

---

## Verificación

Comandos usados desde la raíz del repositorio:

```powershell
dotnet build
dotnet test FitTrack.Tests
```

Resultado final en el tag: **46 pruebas, 0 fallos**.

Para comprobar el ciclo rojo/verde de cada hallazgo se hizo, por cada commit:

```powershell
git checkout <commit>^
git checkout <commit> -- <archivo-de-prueba>
dotnet test FitTrack.Tests --filter "FullyQualifiedName~H_0X"
# falla

git checkout <commit>
dotnet test FitTrack.Tests --filter "FullyQualifiedName~H_0X"
# pasa
```

Los commits de corrección son `b375220` (H-01), `025d1c0` (H-02) y `53cac39` (H-03). El tag `asignacion-3` apunta al commit de documentación que incluye esta bitácora, los criterios actualizados y `AGENTS.md`.

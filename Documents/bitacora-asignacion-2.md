# Bitácora de sesión con el agente de IA

**Asignación:** Asignación 2 — Gestión de permisos y su primera suite de pruebas
**Curso:** Programación III (ITLA)



\---

## 1\. Tarea delegada

Le delegué al agente construir la pieza Gestión de permisos en `FitTrack.Core/Permisos/`: las entidades, la máquina de estados, el servicio, el controlador y la suite de pruebas. Lo hice en seis prompts cortos, uno por uno, compilando después de cada uno.

## 2\. Lo que le pedí

Para el servicio (prompt 3) le pedí, entre otras cosas:

> "Crea `ServicioPermisos.cs` con `CrearAsync`, `ListarPendientesAsync`, `AprobarAsync` y `RechazarAsync`. Ambos métodos de resolución deben rechazar si el solicitante es el mismo administrador (RF-GP-07), si la transición no está permitida (RF-GP-06), y `RechazarAsync` además debe exigir un motivo (RF-GP-04). Un solo `SaveChangesAsync` al final."

## 3\. Lo que me devolvió

El agente generó `ServicioPermisos` con los cinco métodos, registró el servicio en `Program.cs` y el proyecto compiló. `AprobarAsync` valida la solicitud propia, usa `CambiarEstado` para pasar a `Aprobada`, registra quién y cuándo, agrega el `PermisoUsuario` y pasa a `Aplicada`.

## 4\. Error del agente (caso ilustrativo)

* **Qué hizo mal:** Al iniciar quiso escribir el codigo justo en la rama donde estabamos.
* **Cómo lo detecté:** Vi que miesntras hablabamos aun estabamos en la rama pasada.
* **Cómo lo corregí:** le pedí al agente que cree una rama nueva antes de modificar cualquier archivo.


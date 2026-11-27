# Reglas del agente FitTrack

## Propósito
FitTrack es un gestor de calorías y macronutrientes. Una API REST calcula las metas del usuario a partir de su perfil, registra comidas, lleva un resumen diario con máquina de estados, y tiene control de acceso, permisos, documentos, notificaciones por correo y reportes.

## Lenguaje y framework
C#, .NET 10, ASP.NET Core, Entity Framework Core 10 y SQL Server. El frontend (FitTrack.Web) usa React, Vite y Tailwind y tiene sus propias reglas.
Proyectos: FitTrack.Api (controladores), FitTrack.Core (usuarios, permisos, documentos, correos, ContextoBD y migraciones), FitTrack.Negocio (perfil, alimentos y resumen diario), FitTrack.Enviador (consola que envía los correos de la cola) y FitTrack.Tests.
Dependencias: Api usa Core y Negocio; Negocio usa Core; Enviador usa Core; Core no depende de ningún otro proyecto.

## Cómo correrlo (PowerShell)
- API: `dotnet run --project FitTrack.Api` (http://localhost:5262).
- Todo con Docker: `Copy-Item .env.example .env; docker compose up --build`.
- Enviador de correos: `dotnet run --project FitTrack.Enviador`.
- Nueva migración: `dotnet ef migrations add NombreMigracion --project FitTrack.Core --startup-project FitTrack.Api`.

## Cómo probarlo
- `dotnet build` y `dotnet test FitTrack.Tests` (pruebas en memoria, sin base de datos ni internet). Deben pasar antes de dar la tarea por terminada.

## Convenciones de código
- Nombres, comentarios y mensajes en español.
- Código simple: clases planas, sin patrones de diseño complejos ni abstracciones que no hagan falta. Legibilidad antes que sofisticación.
- Los controladores son delgados; las reglas de negocio viven en los servicios, que devuelven `Resultado`.
- Sesión con [RequiereSesion] y rol con [RequiereRol]. Los errores se devuelven como { mensaje }.
- Enums como texto en la API. Peso en libras y altura en pulgadas.
- En lecturas se usa AsNoTracking; un solo SaveChangesAsync por operación cuando se pueda.
- Las entidades de Negocio se registran en Program.cs con ContextoBD.EntidadesNegocio.
- Todo cambio del modelo lleva una migración nueva.
- Terminal: Windows con PowerShell (usa `;`, no `&&`).

## Git
- Nunca trabajar en main: cada tarea empieza creando su rama.
- No hacer commit ni push. Un pull request es una funcionalidad.

## Restricciones
- No editar a mano migraciones existentes, archivos Designer ni ContextoBDModelSnapshot: solo generar migraciones nuevas con dotnet ef.
- No tocar FitTrack.Web, docker-compose.yml ni Dockerfile salvo que la tarea lo pida.
- No cambiar el comportamiento de lo que ya existe (control de acceso, permisos, documentos, resumen diario). Los tests existentes deben seguir pasando; solo se adaptan si cambia un constructor.
- Nunca escribir credenciales (contraseñas, claves SMTP, claves de API) en el código, README, commits ni archivos de ejemplo. Se leen de variables de entorno.
- Nunca leer, abrir ni modificar el archivo .env. Usa .env.example solo como referencia.

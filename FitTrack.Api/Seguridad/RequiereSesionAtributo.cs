using FitTrack.Core.Usuarios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace FitTrack.Api.Seguridad;

// Una "etiqueta" que se pone sobre un endpoint: [RequiereSesion].
// Antes de ejecutar el endpoint, este código revisa que venga un token válido.
// Si no, responde 401 y el endpoint ni se ejecuta.
public class RequiereSesionAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // El cliente manda:  Authorization: Bearer <token>
        string cabecera = context.HttpContext.Request.Headers.Authorization.ToString();

        if (!cabecera.StartsWith("Bearer "))
        {
            context.Result = new UnauthorizedObjectResult(new { mensaje = "Debes iniciar sesión." });
            return;
        }

        string token = cabecera.Substring("Bearer ".Length).Trim();

        // Los atributos no reciben servicios en el constructor; se piden así.
        ServicioSesion servicio = context.HttpContext.RequestServices.GetRequiredService<ServicioSesion>();
        Usuario? usuario = await servicio.ObtenerUsuarioAsync(token);

        if (usuario == null)
        {
            context.Result = new UnauthorizedObjectResult(new { mensaje = "Sesión no válida o vencida." });
            return;
        }

        // Se guardan para que el endpoint sepa quién llama sin volver a buscarlo.
        context.HttpContext.Items["Usuario"] = usuario;
        context.HttpContext.Items["Token"] = token;
    }
}
using FitTrack.Core.Usuarios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FitTrack.Api.Seguridad;

// [RequiereRol(Rol.Administrador)] = hay que tener sesión válida Y ese rol (RF-CA-05).
// Hereda de RequiereSesion: primero hace la revisión de sesión de siempre
// y después añade la revisión del rol.
public class RequiereRolAttribute : RequiereSesionAttribute
{
    private readonly Rol _rolNecesario;

    public RequiereRolAttribute(Rol rolNecesario)
    {
        _rolNecesario = rolNecesario;
    }

    public override async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        // 1. La revisión de sesión de siempre (401 si no hay sesión válida).
        await base.OnAuthorizationAsync(context);
        if (context.Result != null)
            return;   // ya se rechazó, no hay nada más que revisar

        // 2. La sesión es válida: se compara el rol del usuario con el exigido.
        Usuario usuario = (Usuario)context.HttpContext.Items["Usuario"]!;

        if (usuario.Rol != _rolNecesario)
        {
            // 403 = "sé quién eres, pero no tienes permiso" (RF-CA-06).
            // Pasa en el servidor, así que armar la petición a mano no ayuda (RD-06).
            context.Result = new ObjectResult(new { mensaje = "No tienes permiso para esta operación." })
            {
                StatusCode = 403
            };
        }
    }
}
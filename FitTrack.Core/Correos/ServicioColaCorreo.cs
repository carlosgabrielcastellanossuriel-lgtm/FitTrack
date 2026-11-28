using FitTrack.Core.Datos;
using FitTrack.Core.Usuarios;

namespace FitTrack.Core.Correos;

// El "buzón": solo deja una carta (una fila) en la tabla CorreosEnCola.
// No envía nada. El envío lo hace el proyecto FitTrack.Enviador.
public class ServicioColaCorreo
{
    // Conexión a la base de datos. Se la entrega .NET automáticamente
    // (inyección de dependencias), por eso se pide en el constructor.
    private readonly ContextoBD _db;

    public ServicioColaCorreo(ContextoBD db)
    {
        _db = db;
    }

    public async Task EncolarAsync(string destinatario, string asunto, string cuerpo)
    {
        // Un destinatario inválido no se encola: el Enviador lo reintentaría
        // tres veces sin posibilidad de éxito. La notificación interna no se pierde.
        if (!Validaciones.CorreoValido(destinatario))
            return;

        // Se crea el objeto con los datos del correo.
        // Estado queda "Pendiente" y FechaCreacion con la hora actual
        // por los valores iniciales de la entidad.
        CorreoEnCola correo = new CorreoEnCola();
        correo.Destinatario = destinatario;
        correo.Asunto = asunto;
        correo.Cuerpo = cuerpo;

        _db.CorreosEnCola.Add(correo);     // lo prepara en memoria
        await _db.SaveChangesAsync();      // aquí se hace el INSERT en SQL Server
    }
}
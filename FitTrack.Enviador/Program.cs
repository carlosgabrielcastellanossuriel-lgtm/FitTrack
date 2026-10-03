using System.Net;
using System.Net.Mail;
using FitTrack.Core.Correos;
using FitTrack.Core.Datos;
using Microsoft.EntityFrameworkCore;

// 1. Leer la configuración de las variables de entorno.
//    Nada de claves escritas en el código (RF-NOT-13).
string? conexion = Environment.GetEnvironmentVariable("FITTRACK_CONEXION");
string? host = Environment.GetEnvironmentVariable("SMTP_HOST");
string? puerto = Environment.GetEnvironmentVariable("SMTP_PUERTO");
string? usuario = Environment.GetEnvironmentVariable("SMTP_USUARIO");
string? clave = Environment.GetEnvironmentVariable("SMTP_CLAVE");

// 2. Si falta alguna variable, avisar y terminar.
if (string.IsNullOrWhiteSpace(conexion) || string.IsNullOrWhiteSpace(host) ||
    string.IsNullOrWhiteSpace(puerto) || string.IsNullOrWhiteSpace(usuario) ||
    string.IsNullOrWhiteSpace(clave))
{
    Console.WriteLine("Faltan variables de entorno.");
    Console.WriteLine("Correos enviados: 0");
    return;
}

// 3. Conectar a la base de datos "a mano": se arman las opciones
//    con la cadena de conexión y se crea el ContextoBD.
DbContextOptions<ContextoBD> opciones = new DbContextOptionsBuilder<ContextoBD>()
    .UseSqlServer(conexion)
    .Options;
using ContextoBD db = new ContextoBD(opciones);

// 4. Traer SOLO los pendientes. Los que ya están Enviado no entran,
//    por eso ejecutarlo dos veces no duplica envíos (RF-NOT-12).
List<CorreoEnCola> pendientes = db.CorreosEnCola
    .Where(c => c.Estado == EstadoCorreo.Pendiente)
    .ToList();

int enviados = 0;

// 5. Recorrer los pendientes de uno en uno.
foreach (CorreoEnCola correo in pendientes)
{
    // try/catch: si un envío falla (servidor caído, clave mala, sin internet),
    // el programa no se cae. El correo queda Pendiente para la próxima vez.
    try
    {
        SmtpClient cliente = new SmtpClient(host, int.Parse(puerto));
        cliente.EnableSsl = true;                                   // conexión cifrada
        cliente.Credentials = new NetworkCredential(usuario, clave);
        cliente.Timeout = 15000;                                    // se rinde a los 15 segundos

        MailMessage mensaje = new MailMessage(usuario, correo.Destinatario, correo.Asunto, correo.Cuerpo);
        cliente.Send(mensaje);

        // Esta parte solo se ejecuta si el envío salió bien.
        correo.Estado = EstadoCorreo.Enviado;
        correo.FechaEnvio = DateTime.UtcNow;
        db.SaveChanges();   // se guarda uno por uno, así lo ya enviado queda registrado
        enviados++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"No se pudo enviar el correo {correo.Id}: {ex.Message}");
    }
}

Console.WriteLine($"Correos enviados: {enviados}");
using FitTrack.Core.Usuarios;

namespace FitTrack.Core.Permisos;

// Permiso ya asignado a un usuario (resultado de aprobar y aplicar una solicitud).
public class PermisoUsuario
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }   // a quién se le asignó el permiso

    public Permiso Permiso { get; set; }     // permiso asignado
}

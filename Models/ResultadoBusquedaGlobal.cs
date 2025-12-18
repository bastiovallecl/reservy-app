namespace ProyectoGestionEventos.Models;

public class ResultadoBusquedaGlobal
{
    public string Tipo { get; set; } = null!; // Reserva, Espacio, Evento, Usuario, Recurso
    public int Id { get; set; }
    public string Titulo { get; set; } = null!;
    public string? Subtitulo { get; set; }
    public string? Descripcion { get; set; }
    public string UrlDetalle { get; set; } = null!;
    public string Icono { get; set; } = null!;
    public string ColorBadge { get; set; } = null!;
}

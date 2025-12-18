using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoGestionEventos.Models;

[Table("ConfiguracionesUsuario")]
public class ConfiguracionUsuario
{
    [Key]
    public int IdConfiguracion { get; set; }

    public int IdUsuario { get; set; }

    [StringLength(50)]
    public string? Tema { get; set; } = "claro"; // claro u oscuro

    public bool NotificacionesEmail { get; set; } = true;

    public bool NotificacionesReservas { get; set; } = true;

    public bool NotificacionesEventos { get; set; } = true;

    [StringLength(10)]
    public string? Idioma { get; set; } = "es";

    public int ElementosPorPagina { get; set; } = 10;

    [Column(TypeName = "datetime")]
    public DateTime? FechaActualizacion { get; set; }

    [ForeignKey("IdUsuario")]
    public virtual Usuario? Usuario { get; set; }
}

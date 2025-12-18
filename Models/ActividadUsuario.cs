using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoGestionEventos.Models;

[Table("ActividadesUsuario")]
public class ActividadUsuario
{
    [Key]
    public int IdActividad { get; set; }

    public int IdUsuario { get; set; }

    [Required]
    [StringLength(50)]
    public string TipoAccion { get; set; } = null!; // Crear, Editar, Eliminar, Login, Logout, etc.

    [Required]
    [StringLength(100)]
    public string Modulo { get; set; } = null!; // Usuarios, Reservas, Espacios, etc.

    [StringLength(500)]
    public string? Descripcion { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime FechaHora { get; set; }

    [StringLength(45)]
    public string? DireccionIP { get; set; }

    [StringLength(200)]
    public string? UserAgent { get; set; }

    [ForeignKey("IdUsuario")]
    public virtual Usuario? Usuario { get; set; }
}

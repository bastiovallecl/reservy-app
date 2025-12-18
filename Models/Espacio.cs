using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoGestionEventos.Models;

public partial class Espacio
{
    [Key]
    public int IdEspacio { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "El nombre no puede contener solo espacios")]
    [Display(Name = "Nombre del Espacio")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "La capacidad es obligatoria")]
    [Range(1, 10000, ErrorMessage = "La capacidad debe estar entre 1 y 10000 personas")]
    [Display(Name = "Capacidad")]
    public int Capacidad { get; set; }

    [Required(ErrorMessage = "La ubicación es obligatoria")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "La ubicación debe tener entre 3 y 200 caracteres")]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "La ubicación no puede contener solo espacios")]
    [Display(Name = "Ubicación")]
    public string Ubicacion { get; set; } = null!;

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El tipo de espacio es obligatorio")]
    [StringLength(20)]
    [Display(Name = "Tipo de Espacio")]
    public string TipoEspacio { get; set; } = null!;

    public bool? Estado { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaCreacion { get; set; }

    [InverseProperty("IdEspacioNavigation")]
    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}

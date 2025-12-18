using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoGestionEventos.Models;

public partial class Recurso
{
    [Key]
    public int IdRecurso { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "El nombre no puede contener solo espacios")]
    [Display(Name = "Nombre del Recurso")]
    public string Nombre { get; set; } = null!;

    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El tipo de recurso es obligatorio")]
    [StringLength(30)]
    [Display(Name = "Tipo de Recurso")]
    public string TipoRecurso { get; set; } = null!;

    [Required(ErrorMessage = "La cantidad disponible es obligatoria")]
    [Range(0, 10000, ErrorMessage = "La cantidad debe estar entre 0 y 10000")]
    [Display(Name = "Cantidad Disponible")]
    public int CantidadDisponible { get; set; }

    public bool? Estado { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaRegistro { get; set; }

    [InverseProperty("IdRecursoNavigation")]
    public virtual ICollection<ReservaRecurso> ReservaRecursos { get; set; } = new List<ReservaRecurso>();
}

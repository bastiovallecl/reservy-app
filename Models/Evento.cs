using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoGestionEventos.Models;

public partial class Evento
{
    [Key]
    public int IdEvento { get; set; }

    [Required(ErrorMessage = "El título es obligatorio")]
    [StringLength(150, MinimumLength = 3, ErrorMessage = "El título debe tener entre 3 y 150 caracteres")]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "El título no puede contener solo espacios")]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = null!;

    [Required(ErrorMessage = "La descripción es obligatoria")]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "La descripción debe tener entre 10 y 1000 caracteres")]
    [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "La descripción no puede contener solo espacios")]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = null!;

    [Required(ErrorMessage = "El tipo de evento es obligatorio")]
    [StringLength(20)]
    [Display(Name = "Tipo de Evento")]
    public string TipoEvento { get; set; } = null!;

    public bool? Estado { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaCreacion { get; set; }

    [InverseProperty("IdEventoNavigation")]
    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoGestionEventos.Models;

public partial class Usuario
{
    [Key]
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "El RUN es obligatorio")]
    [StringLength(12, MinimumLength = 10, ErrorMessage = "El RUN debe tener entre 10 y 12 caracteres")]
    [RegularExpression(@"^\d{7,8}-[\dkK]$", ErrorMessage = "El RUN debe tener el formato 12345678-9 o 12345678-K")]
    [Display(Name = "RUN")]
    public string Run { get; set; } = null!;

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El apellido solo puede contener letras")]
    [Display(Name = "Apellido")]
    public string Apellido { get; set; } = null!;

    [Required(ErrorMessage = "El email es obligatorio")]
    [StringLength(150, ErrorMessage = "El email no puede exceder 150 caracteres")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido")]
    [Display(Name = "Email")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [StringLength(15, MinimumLength = 9, ErrorMessage = "El teléfono debe tener entre 9 y 15 dígitos")]
    [RegularExpression(@"^\d+$", ErrorMessage = "El teléfono solo puede contener números")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = null!;

    [Required(ErrorMessage = "El tipo de usuario es obligatorio")]
    [StringLength(20)]
    [Display(Name = "Tipo de Usuario")]
    public string TipoUsuario { get; set; } = null!;

    [StringLength(100, ErrorMessage = "El cargo no puede exceder 100 caracteres")]
    [Display(Name = "Cargo")]
    public string? Cargo { get; set; }

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
    [Display(Name = "Contraseña")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    public int IdRol { get; set; }

    public bool? Estado { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaRegistro { get; set; }

    [InverseProperty("IdUsuarioAprobadorNavigation")]
    public virtual ICollection<HistorialAprobacion> HistorialAprobacions { get; set; } = new List<HistorialAprobacion>();

    public virtual Rol? IdRolNavigation { get; set; } = null!;

    [InverseProperty("IdUsuarioNavigation")]
    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}

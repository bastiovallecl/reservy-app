using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoGestionEventos.Models;

public partial class Reserva
{
    [Key]
    public int IdReserva { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un espacio")]
    [Display(Name = "Espacio")]
    public int IdEspacio { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un usuario")]
    [Display(Name = "Usuario Solicitante")]
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un evento")]
    [Display(Name = "Evento")]
    public int IdEvento { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria")]
    [Column(TypeName = "datetime")]
    [Display(Name = "Fecha y Hora de Inicio")]
    public DateTime FechaInicio { get; set; }

    [Required(ErrorMessage = "La fecha de fin es obligatoria")]
    [Column(TypeName = "datetime")]
    [Display(Name = "Fecha y Hora de Fin")]
    public DateTime FechaFin { get; set; }

    [Required(ErrorMessage = "La cantidad de asistentes es obligatoria")]
    [Range(1, 10000, ErrorMessage = "La cantidad de asistentes debe estar entre 1 y 10000")]
    [Display(Name = "Cantidad de Asistentes")]
    public int CantidadAsistentes { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un estado")]
    [StringLength(20)]
    [Display(Name = "Estado de la Reserva")]
    public string? EstadoReserva { get; set; }

    [StringLength(500, ErrorMessage = "El motivo no puede exceder 500 caracteres")]
    [Display(Name = "Motivo de Rechazo")]
    public string? MotivoRechazo { get; set; }

    [StringLength(1000, ErrorMessage = "Las observaciones no pueden exceder 1000 caracteres")]
    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaSolicitud { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaRespuesta { get; set; }

    [InverseProperty("IdReservaNavigation")]
    public virtual ICollection<HistorialAprobacion> HistorialAprobacions { get; set; } = new List<HistorialAprobacion>();

    [ForeignKey("IdEspacio")]
    [InverseProperty("Reservas")]
    public virtual Espacio IdEspacioNavigation { get; set; } = null!;

    [ForeignKey("IdEvento")]
    [InverseProperty("Reservas")]
    public virtual Evento IdEventoNavigation { get; set; } = null!;

    [ForeignKey("IdUsuario")]
    [InverseProperty("Reservas")]
    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    [InverseProperty("IdReservaNavigation")]
    public virtual ICollection<ReservaRecurso> ReservaRecursos { get; set; } = new List<ReservaRecurso>();
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoGestionEventos.Models;

[Table("HistorialAprobacion")]
public partial class HistorialAprobacion
{
    [Key]
    public int IdHistorial { get; set; }

    public int IdReserva { get; set; }

    public int IdUsuarioAprobador { get; set; }

    [StringLength(20)]
    public string AccionRealizada { get; set; } = null!;

    [StringLength(1000)]
    public string? Observaciones { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaAccion { get; set; }

    [ForeignKey("IdReserva")]
    [InverseProperty("HistorialAprobacions")]
    public virtual Reserva IdReservaNavigation { get; set; } = null!;

    [ForeignKey("IdUsuarioAprobador")]
    [InverseProperty("HistorialAprobacions")]
    public virtual Usuario IdUsuarioAprobadorNavigation { get; set; } = null!;
}

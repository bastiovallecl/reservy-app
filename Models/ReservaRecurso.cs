using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ProyectoGestionEventos.Models;

public partial class ReservaRecurso
{
    [Key]
    public int IdReservaRecurso { get; set; }

    public int IdReserva { get; set; }

    public int IdRecurso { get; set; }

    public int CantidadSolicitada { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? FechaAsignacion { get; set; }

    [ForeignKey("IdRecurso")]
    [InverseProperty("ReservaRecursos")]
    public virtual Recurso IdRecursoNavigation { get; set; } = null!;

    [ForeignKey("IdReserva")]
    [InverseProperty("ReservaRecursos")]
    public virtual Reserva IdReservaNavigation { get; set; } = null!;
}

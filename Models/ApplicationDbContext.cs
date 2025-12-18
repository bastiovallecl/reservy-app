using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProyectoGestionEventos.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Espacio> Espacios { get; set; }

    public virtual DbSet<Evento> Eventos { get; set; }

    public virtual DbSet<HistorialAprobacion> HistorialAprobacions { get; set; }

    public virtual DbSet<Recurso> Recursos { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<ReservaRecurso> ReservaRecursos { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<ActividadUsuario> ActividadesUsuario { get; set; }

    public virtual DbSet<ConfiguracionUsuario> ConfiguracionesUsuario { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Espacio>(entity =>
        {
            entity.HasKey(e => e.IdEspacio).HasName("PK__Espacios__CA4C0889F276B42A");
        });

        modelBuilder.Entity<Evento>(entity =>
        {
            entity.HasKey(e => e.IdEvento).HasName("PK__Eventos__034EFC04E1E028FC");
        });

        modelBuilder.Entity<HistorialAprobacion>(entity =>
        {
            entity.HasKey(e => e.IdHistorial).HasName("PK__Historia__9CC7DBB4F03C3FB2");

            entity.HasOne(d => d.IdReservaNavigation).WithMany(p => p.HistorialAprobacions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Historial__IdRes__59FA5E80");

            entity.HasOne(d => d.IdUsuarioAprobadorNavigation).WithMany(p => p.HistorialAprobacions)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Historial__IdUsu__5AEE82B9");
        });

        modelBuilder.Entity<Recurso>(entity =>
        {
            entity.HasKey(e => e.IdRecurso).HasName("PK__Recursos__B91948E95CC97450");
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.IdReserva).HasName("PK__Reservas__0E49C69D35156BCB");

            entity.HasOne(d => d.IdEspacioNavigation).WithMany(p => p.Reservas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reservas__IdEspa__5165187F");

            entity.HasOne(d => d.IdEventoNavigation).WithMany(p => p.Reservas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reservas__IdEven__534D60F1");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Reservas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reservas__IdUsua__52593CB8");
        });

        modelBuilder.Entity<ReservaRecurso>(entity =>
        {
            entity.HasKey(e => e.IdReservaRecurso).HasName("PK__ReservaR__4B1C5ADB53B19099");

            entity.HasOne(d => d.IdRecursoNavigation).WithMany(p => p.ReservaRecursos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__ReservaRe__IdRec__571DF1D5");

            entity.HasOne(d => d.IdReservaNavigation).WithMany(p => p.ReservaRecursos).HasConstraintName("FK__ReservaRe__IdRes__5629CD9C");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Rols__2A49584CEE08E5A7");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuarios__5B65BF97745B5C59");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuarios__IdRol__4BAC3F29");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

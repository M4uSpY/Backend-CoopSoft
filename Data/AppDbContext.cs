using System;
using BackendCoopSoft.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSets (SIN PlanillaValor)
    public DbSet<Persona> Personas => Set<Persona>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Rol> Roles => Set<Rol>();

    public DbSet<Trabajador> Trabajadores => Set<Trabajador>();
    public DbSet<Cargo> Cargos => Set<Cargo>();
    public DbSet<Oficina> Oficinas => Set<Oficina>();

    public DbSet<Horario> Horarios => Set<Horario>();
    public DbSet<Asistencia> Asistencias => Set<Asistencia>();
    public DbSet<Capacitacion> Capacitaciones => Set<Capacitacion>();
    public DbSet<FormacionAcademica> FormacionesAcademicas => Set<FormacionAcademica>();

    public DbSet<Clasificador> Clasificadores => Set<Clasificador>();
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();

    public DbSet<HuellaDactilar> HuellasDactilares => Set<HuellaDactilar>();
    public DbSet<Falta> Faltas => Set<Falta>();
    public DbSet<Licencia> Licencias { get; set; } = null!;

    public DbSet<HistoricoUsuario> HistoricosUsuario => Set<HistoricoUsuario>();
    public DbSet<HistoricoFalta> HistoricosFalta => Set<HistoricoFalta>();
    public DbSet<HistoricoTrabajador> HistoricosTrabajador => Set<HistoricoTrabajador>();
    public DbSet<HistoricoPersona> HistoricosPersona => Set<HistoricoPersona>();
    public DbSet<HistoricoTrabajadorPlanilla> HistoricosTrabajadorPlanilla => Set<HistoricoTrabajadorPlanilla>();
    public DbSet<HistoricoPlanilla> HistoricoPlanilla => Set<HistoricoPlanilla>();

    public DbSet<Planilla> Planillas => Set<Planilla>();
    public DbSet<Concepto> Conceptos => Set<Concepto>();
    public DbSet<TrabajadorPlanilla> TrabajadorPlanillas => Set<TrabajadorPlanilla>();
    public DbSet<TrabajadorPlanillaValor> TrabajadorPlanillaValores => Set<TrabajadorPlanillaValor>();

    public DbSet<Contrato> Contratos => Set<Contrato>();
    public DbSet<LogAcceso> LogAccesos => Set<LogAcceso>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unicidades básicas
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.IdPersona)
            .IsUnique();

        // Persona (1) ↔ (0..1) Trabajador (dependiente requerido)
        modelBuilder.Entity<Trabajador>()
            .HasOne(t => t.Persona)
            .WithOne(p => p.Trabajador)
            .HasForeignKey<Trabajador>(t => t.IdPersona)
            .IsRequired()                          // el Trabajador SIEMPRE tiene Persona
            .OnDelete(DeleteBehavior.Restrict);    // evita cascadas múltiples


        modelBuilder.Entity<Clasificador>()
            .HasIndex(c => new { c.Categoria, c.ValorCategoria })
            .IsUnique();

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.NombreUsuario)
            .IsUnique();

        modelBuilder.Entity<Horario>()
            .HasIndex(h => new { h.IdTrabajador, h.DiaSemana })
            .IsUnique();

        // Decimales
        modelBuilder.Entity<Trabajador>()
            .Property(t => t.HaberBasico)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<TrabajadorPlanilla>()
            .Property(tp => tp.HaberBasicoMes)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<TrabajadorPlanillaValor>()
            .Property(tp => tp.Valor)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Concepto>()
            .Property(c => c.Monto)
            .HasColumnType("decimal(18,2)");

        modelBuilder.Entity<Concepto>()
            .Property(c => c.Porcentaje)
            .HasColumnType("decimal(9,6)");

        // Reglas planillas
        modelBuilder.Entity<TrabajadorPlanilla>()
            .HasIndex(tp => new { tp.IdTrabajador, tp.IdPlanilla })
            .IsUnique();

        modelBuilder.Entity<TrabajadorPlanillaValor>()
            .HasIndex(tp => new { tp.IdTrabajadorPlanilla, tp.IdConcepto })
            .IsUnique();

        modelBuilder.Entity<Concepto>()
            .HasIndex(c => c.Codigo)
            .IsUnique();

        modelBuilder.Entity<Planilla>()
            .HasIndex(p => new { p.IdTipoPlanilla, p.Gestion, p.Mes })
            .IsUnique();

        // Relaciones con DeleteBehavior para evitar cascadas múltiples
        modelBuilder.Entity<Falta>()
            .HasOne(f => f.Trabajador)
            .WithMany(t => t.Faltas)
            .HasForeignKey(f => f.IdTrabajador)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Contrato>()
            .HasOne(c => c.Trabajador)
            .WithMany(t => t.Contratos)
            .HasForeignKey(c => c.IdTrabajador)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Solicitud>()
            .HasOne(s => s.Trabajador)
            .WithMany(t => t.Solicitudes)
            .HasForeignKey(s => s.IdTrabajador)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Capacitacion>()
            .HasOne(c => c.Trabajador)
            .WithMany(t => t.Capacitaciones)
            .HasForeignKey(c => c.IdTrabajador)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Asistencia>()
            .HasOne(a => a.Trabajador)
            .WithMany(t => t.Asistencias)
            .HasForeignKey(a => a.IdTrabajador)
            .OnDelete(DeleteBehavior.Restrict);

        // Clasificador ↔ otras entidades (emparejando las navs del Clasificador)
        modelBuilder.Entity<Persona>()
            .HasOne(p => p.Nacionalidad)
            .WithMany(c => c.Personas)
            .HasForeignKey(p => p.IdNacionalidad)
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<Contrato>()
            .HasOne(c => c.TipoContrato)
            .WithMany(cl => cl.TiposContrato)
            .HasForeignKey(c => c.IdTipoContrato)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Contrato>()
            .HasOne(c => c.PeriodoPago)
            .WithMany(cl => cl.PeriodosPago)
            .HasForeignKey(c => c.IdPeriodoPago)
            .OnDelete(DeleteBehavior.Restrict);


        modelBuilder.Entity<Licencia>()
        .Property(l => l.CantidadJornadas)
        .HasColumnType("decimal(5,2)");

        modelBuilder.Entity<Licencia>()
        .HasOne(l => l.Trabajador)
        .WithMany(t => t.Licencias)
        .HasForeignKey(l => l.IdTrabajador)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Licencia>()
            .HasOne(l => l.TipoLicencia)
            .WithMany(c => c.Licencias)
            .HasForeignKey(l => l.IdTipoLicencia)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Licencia>()
            .HasOne(l => l.EstadoLicencia)
            .WithMany(c => c.EstadosLicencia)
            .HasForeignKey(l => l.IdEstadoLicencia)
            .OnDelete(DeleteBehavior.Restrict);



        modelBuilder.Entity<Falta>()
            .HasOne(f => f.TipoFalta)
            .WithMany(c => c.Faltas)
            .HasForeignKey(f => f.IdTipoFalta)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Solicitud>()
            .HasOne(s => s.EstadoSolicitud)
            .WithMany(cl => cl.EstadosSolicitud)
            .HasForeignKey(s => s.IdEstadoSolicitud)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Planilla>()
            .HasOne(p => p.TipoPlanilla)
            .WithMany(c => c.TiposPlanilla)
            .HasForeignKey(p => p.IdTipoPlanilla)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Concepto>()
            .HasOne(c => c.Naturaleza)
            .WithMany(cl => cl.NaturalezasConcepto)
            .HasForeignKey(c => c.IdNaturaleza)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Concepto>()
            .HasOne(c => c.MetodoCalculo)
            .WithMany(cl => cl.MetodosCalculoConcepto)
            .HasForeignKey(c => c.IdMetodoCalculo)
            .OnDelete(DeleteBehavior.Restrict);

        // Históricos: relaciones con Usuario (actor) en Restrict para evitar ciclos
        modelBuilder.Entity<HistoricoUsuario>()
            .HasOne(h => h.Usuario)
            .WithMany(u => u.HistoricosComoTitular)
            .HasForeignKey(h => h.IdUsuario)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HistoricoUsuario>()
            .HasOne(h => h.UsuarioModifico)
            .WithMany(u => u.HistoricosModificadosPorMi)
            .HasForeignKey(h => h.UsuarioModificoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HistoricoFalta>()
            .HasOne(h => h.UsuarioModifico)
            .WithMany(u => u.HistoricosFaltaModificadosPorMi)
            .HasForeignKey(h => h.UsuarioModificoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HistoricoTrabajadorPlanilla>()
            .HasOne(h => h.UsuarioModifico)
            .WithMany(u => u.HistoricosTrabajadorPlanillaModificadosPorMi)
            .HasForeignKey(h => h.UsuarioModificoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HistoricoPlanilla>()
            .HasOne(h => h.UsuarioModifico)
            .WithMany(u => u.HistoricosPlanillaModificadosPorMi)
            .HasForeignKey(h => h.UsuarioModificoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HistoricoTrabajador>()
            .HasOne(h => h.UsuarioModifico)
            .WithMany(u => u.HistoricosTrabajadorModificadosPorMi)
            .HasForeignKey(h => h.UsuarioModificoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HistoricoPersona>()
            .HasOne(h => h.UsuarioModifico)
            .WithMany(u => u.HistoricosPersonaModificadosPorMi)
            .HasForeignKey(h => h.UsuarioModificoId)
            .OnDelete(DeleteBehavior.Restrict);

        // (Opcional) índices útiles
        modelBuilder.Entity<Concepto>()
            .HasIndex(c => c.OrdenCalculo);
    }
}

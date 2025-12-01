using System.Globalization;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Licencias;
using BackendCoopSoft.Models;
using BackendCoopSoft.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace BackendCoopSoft.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class LicenciasController : ControllerBase
{
    private readonly AppDbContext _db;
    private const string CARGO_ADMIN = "Administrador General";
    private const int ID_TIPO_LICENCIA_PERMISO_TEMPORAL = 25; // <-- CAMBIA ESTO

    public LicenciasController(AppDbContext db)
    {
        _db = db;
    }

    private string? GetRolUsuarioActual()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value;
    }

    private static bool EsCargoAdministrador(string? nombreCargo)
    {
        if (string.IsNullOrWhiteSpace(nombreCargo))
            return false;

        return string.Equals(nombreCargo.Trim(), CARGO_ADMIN, StringComparison.OrdinalIgnoreCase);
    }

    private static bool EsCargoCasual(string? nombreCargo)
    {
        return !EsCargoAdministrador(nombreCargo);
    }

    private static bool PuedeGestionarSegunRol(string? rolActual, string? nombreCargoTrabajador)
    {
        if (string.IsNullOrWhiteSpace(rolActual))
            return false;

        rolActual = rolActual.Trim();

        if (string.Equals(rolActual, "Consejo", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(rolActual, "Consejo de Administración", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(rolActual, "Consejo de Administracion", StringComparison.OrdinalIgnoreCase))
        {
            return EsCargoAdministrador(nombreCargoTrabajador);
        }

        if (string.Equals(rolActual, "Administrador", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(rolActual, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return EsCargoCasual(nombreCargoTrabajador);
        }

        return false;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerLicencias()
    {
        var rolActual = GetRolUsuarioActual();
        if (string.IsNullOrWhiteSpace(rolActual))
            return Forbid("No se pudo determinar el rol del usuario actual.");

        var rol = rolActual.Trim();

        var query = _db.Licencias
            .Include(l => l.Trabajador).ThenInclude(t => t.Persona)
            .Include(l => l.Trabajador).ThenInclude(t => t.Cargo)
            .Include(l => l.TipoLicencia)
            .Include(l => l.EstadoLicencia)
            .AsQueryable();

        // Consejo -> solo "Administrador General"
        if (rol.Equals("Consejo", StringComparison.OrdinalIgnoreCase) ||
            rol.Equals("Consejo de Administración", StringComparison.OrdinalIgnoreCase) ||
            rol.Equals("Consejo de Administracion", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(l => l.Trabajador.Cargo.NombreCargo == CARGO_ADMIN);
        }
        // Administrador -> todos MENOS "Administrador General"
        else if (rol.Equals("Administrador", StringComparison.OrdinalIgnoreCase) ||
                 rol.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(l => l.Trabajador.Cargo.NombreCargo != CARGO_ADMIN);
        }
        else
        {
            return Forbid("No tiene permisos para ver la lista de licencias.");
        }

        var licencias = await query.ToListAsync();

        var dto = licencias.Select(l => new LicenciaListarDTO
        {
            IdLicencia = l.IdLicencia,
            IdTrabajador = l.IdTrabajador,
            CI = l.Trabajador.Persona.CarnetIdentidad,
            ApellidosNombres = $"{l.Trabajador.Persona.ApellidoPaterno} {l.Trabajador.Persona.ApellidoMaterno} {l.Trabajador.Persona.PrimerNombre}",
            Cargo = l.Trabajador.Cargo.NombreCargo,
            TipoLicencia = l.TipoLicencia.ValorCategoria,
            FechaInicio = l.FechaInicio,
            FechaFin = l.FechaFin,
            HoraInicio = l.HoraInicio,
            HoraFin = l.HoraFin,
            CantidadJornadas = l.CantidadJornadas,
            Estado = l.EstadoLicencia.ValorCategoria,
            Motivo = l.Motivo,
            Observacion = l.Observacion,
            TieneArchivoJustificativo = l.ArchivoJustificativo != null && l.ArchivoJustificativo.Length > 0
        }).ToList();

        return Ok(dto);
    }


    [HttpGet("trabajador/{idTrabajador:int}")]
    public async Task<IActionResult> ObtenerLicenciasPorTrabajador(int idTrabajador)
    {
        var licencias = await _db.Licencias
            .Include(l => l.TipoLicencia)
            .Include(l => l.EstadoLicencia)
            .Where(l => l.IdTrabajador == idTrabajador)
            .OrderByDescending(l => l.FechaInicio)
            .ToListAsync();

        var dto = licencias.Select(l => new LicenciaListarDTO
        {
            IdLicencia = l.IdLicencia,
            IdTrabajador = l.IdTrabajador,
            TipoLicencia = l.TipoLicencia.ValorCategoria,
            FechaInicio = l.FechaInicio,
            FechaFin = l.FechaFin,
            HoraInicio = l.HoraInicio,
            HoraFin = l.HoraFin,
            CantidadJornadas = l.CantidadJornadas,
            Estado = l.EstadoLicencia.ValorCategoria,
            Motivo = l.Motivo,
            Observacion = l.Observacion,
            TieneArchivoJustificativo = l.ArchivoJustificativo != null && l.ArchivoJustificativo.Length > 0
        }).ToList();

        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> CrearLicencia([FromBody] LicenciaCrearDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // 1) Trabajador
        var trabajador = await _db.Trabajadores
            .Include(t => t.Persona)
            .Include(t => t.Horarios)
            .FirstOrDefaultAsync(t => t.IdTrabajador == dto.IdTrabajador);

        if (trabajador is null)
            return BadRequest("El trabajador indicado no existe.");

        // 2) Tipo de licencia
        var tipoLicencia = await _db.Clasificadores
            .FirstOrDefaultAsync(c =>
                c.IdClasificador == dto.IdTipoLicencia &&
                c.Categoria == "TipoLicencia");

        if (tipoLicencia is null)
            return BadRequest("El tipo de licencia no es válido.");

        var estadoPendiente = await _db.Clasificadores
            .FirstOrDefaultAsync(c =>
                c.Categoria == "EstadoSolicitud" &&
                c.ValorCategoria == "Pendiente");

        if (estadoPendiente is null)
            return StatusCode(500, "No está configurado el estado 'Pendiente'.");

        var horarios = trabajador.Horarios.ToList();

        // Copias locales que vamos a ajustar según reglas
        var fechaInicio = dto.FechaInicio.Date;
        var fechaFin = dto.FechaFin.Date;
        var horaInicio = dto.HoraInicio;
        var horaFin = dto.HoraFin;

        if (fechaFin < fechaInicio)
            return BadRequest("La fecha de fin no puede ser anterior a la fecha de inicio.");

        if (fechaInicio == fechaFin && horaFin <= horaInicio)
            return BadRequest("En licencias de un solo día, la hora de fin debe ser mayor a la hora de inicio.");

        string nombreTipo = (tipoLicencia.ValorCategoria ?? string.Empty).Trim();

        // ⭐ Detectar permiso temporal por nombre (case-insensitive)
        bool esPermisoTemporal =
            string.Equals(nombreTipo, "Permiso temporal", StringComparison.OrdinalIgnoreCase);

        // ======================================================
        //  REGLAS ESPECÍFICAS POR TIPO DE LICENCIA (resumen)
        // ======================================================
        switch (nombreTipo)
        {
            // --------------------------------------------------
            // MATERNIDAD: 45 días hábiles exactos
            // --------------------------------------------------
            case "Maternidad":
                {
                    var diasHabiles = ContarDiasHabiles(fechaInicio, fechaFin);
                    if (diasHabiles != 45)
                    {
                        return BadRequest(
                            $"La licencia por maternidad debe ser de exactamente 45 días hábiles. " +
                            $"Actualmente el rango seleccionado tiene {diasHabiles} día(s) hábil(es).");
                    }
                }
                break;

            // --------------------------------------------------
            // PATERNIDAD: 3 días corridos exactos
            // --------------------------------------------------
            case "Paternidad":
                {
                    var dias = (fechaFin - fechaInicio).TotalDays + 1;
                    if (dias != 3)
                        return BadRequest("La licencia por paternidad debe ser de exactamente 3 días consecutivos.");
                }
                break;

            // --------------------------------------------------
            // MATRIMONIO: 3 días corridos exactos
            // --------------------------------------------------
            case "Matrimonio":
                {
                    var dias = (fechaFin - fechaInicio).TotalDays + 1;
                    if (dias != 3)
                        return BadRequest("La licencia por matrimonio debe ser de exactamente 3 días consecutivos.");
                }
                break;

            // --------------------------------------------------
            // LUTO / DUELO: 3 días corridos exactos
            // --------------------------------------------------
            case "Luto / Duelo":
                {
                    var dias = (fechaFin - fechaInicio).TotalDays + 1;
                    if (dias != 3)
                        return BadRequest("La licencia por luto o duelo debe ser de exactamente 3 días consecutivos.");
                }
                break;

            // --------------------------------------------------
            // ESTADO CRÍTICO DE SALUD:
            //   (supuesto) mínimo 1 día hábil y máximo 7 hábiles
            //   → ajusta los límites si tu reglamento dice otra cosa
            // --------------------------------------------------
            case "Estado crítico de salud":
                {
                    var diasHabiles = ContarDiasHabiles(fechaInicio, fechaFin);
                    if (diasHabiles <= 0)
                        return BadRequest("La licencia por estado crítico de salud debe tener al menos 1 día hábil.");

                    if (diasHabiles > 7) // <-- ajusta este 7 si tu normativa dice otro máximo
                        return BadRequest(
                            $"La licencia por estado crítico de salud no puede exceder los 7 días hábiles. " +
                            $"Actualmente tiene {diasHabiles} día(s) hábil(es).");
                }
                break;

            // --------------------------------------------------
            // CUMPLEAÑOS:
            //   - debe ser SOLO el día del cumpleaños
            //   - solo media jornada (≤ 50% de la jornada del día)
            //   - solo una vez por año
            // --------------------------------------------------
            case "Cumpleaños":
                {
                    // 1) Un solo día
                    if (fechaInicio != fechaFin)
                        return BadRequest("La licencia por cumpleaños debe solicitarse para un solo día.");

                    // 2) Validar que ese día sea el cumpleaños del trabajador
                    var fechaNac = trabajador.Persona.FechaNacimiento.Date;
                    var cumpleEsteAnio = new DateTime(fechaInicio.Year, fechaNac.Month, fechaNac.Day);

                    if (fechaInicio.Date != cumpleEsteAnio.Date)
                        return BadRequest("La licencia por cumpleaños solo puede solicitarse para el día del cumpleaños del trabajador.");

                    // 3) Verificar que no haya usado ya la licencia de cumpleaños este año
                    var yaUsoCumpleanio = await _db.Licencias
                        .Include(l => l.EstadoLicencia)
                        .Where(l =>
                            l.IdTrabajador == dto.IdTrabajador &&
                            l.IdTipoLicencia == tipoLicencia.IdClasificador &&
                            l.FechaInicio.Year == fechaInicio.Year &&
                            l.EstadoLicencia.ValorCategoria != "Rechazado")
                        .AnyAsync();

                    if (yaUsoCumpleanio)
                        return BadRequest("El trabajador ya utilizó su licencia por cumpleaños en esta gestión.");

                    // 4) Validar que el rango de horas no exceda media jornada
                    var horarioDia = ObtenerHorarioDia(horarios, fechaInicio);
                    if (horarioDia is null)
                        return BadRequest("El trabajador no tiene horario asignado para el día de su cumpleaños.");

                    var horasJornada = (horarioDia.HoraSalida - horarioDia.HoraEntrada).TotalHours;
                    var horasLicencia = (horaFin - horaInicio).TotalHours;

                    if (horasLicencia <= 0)
                        return BadRequest("La licencia por cumpleaños debe tener una duración mayor a cero.");

                    if (horasLicencia > (horasJornada / 2.0) + 0.01) // media jornada
                    {
                        return BadRequest(
                            $"La licencia por cumpleaños no puede exceder la media jornada laboral. " +
                            $"Jornada del día: {horasJornada:0.##} h, máximo permitido: {(horasJornada / 2.0):0.##} h.");
                    }
                }
                break;

            // --------------------------------------------------
            // PERMISO TEMPORAL: ya tenías reglas (un día + 3h)
            // --------------------------------------------------
            case "Permiso temporal":
                {
                    // Debe ser un solo día
                    if (fechaInicio != fechaFin)
                        return BadRequest("El permiso temporal debe solicitarse para un solo día.");

                    // Máximo 3 horas por solicitud
                    var duracionHoras = (horaFin - horaInicio).TotalHours;
                    if (duracionHoras <= 0)
                        return BadRequest("La duración del permiso debe ser mayor a cero.");
                    if (duracionHoras > 3.0)
                        return BadRequest("Cada permiso temporal no puede exceder las 3 horas.");
                }
                break;

            // --------------------------------------------------
            // EXÁMENES MÉDICOS: 1 día hábil
            // --------------------------------------------------
            case "Examen Papanicolau / Mamografía":
            case "Examen Próstata":
            case "Examen Colon":
                {
                    var diasHabiles = ContarDiasHabiles(fechaInicio, fechaFin);
                    if (diasHabiles != 1)
                    {
                        return BadRequest(
                            $"La licencia por examen médico debe ser de exactamente 1 día hábil. " +
                            $"Actualmente el rango seleccionado tiene {diasHabiles} día(s) hábil(es).");
                    }

                    // (Opcional) Solo una vez por año:
                    var yaUsoExamen = await _db.Licencias
                        .Include(l => l.EstadoLicencia)
                        .Where(l =>
                            l.IdTrabajador == dto.IdTrabajador &&
                            l.IdTipoLicencia == tipoLicencia.IdClasificador &&
                            l.FechaInicio.Year == fechaInicio.Year &&
                            l.EstadoLicencia.ValorCategoria != "Rechazado")
                        .AnyAsync();

                    if (yaUsoExamen)
                        return BadRequest("El trabajador ya tiene registrada una licencia de este tipo de examen en la gestión actual.");
                }
                break;

            default:
                break;
        }

        // ======================================================
        //  REGLA ESPECIAL: PERMISO TEMPORAL 3h/MES  ⭐⭐⭐
        // ======================================================
        if (esPermisoTemporal)
        {
            var horasSolicitudActual = (horaFin - horaInicio).TotalHours;

            int year = fechaInicio.Year;
            int month = fechaInicio.Month;

            var licenciasMes = await _db.Licencias
                .Include(l => l.EstadoLicencia)
                .Where(l => l.IdTrabajador == dto.IdTrabajador
                            && l.IdTipoLicencia == tipoLicencia.IdClasificador
                            && l.FechaInicio.Year == year
                            && l.FechaInicio.Month == month
                            && l.EstadoLicencia.ValorCategoria != "Rechazado")
                .ToListAsync();

            double horasYaUsadas = licenciasMes
                .Sum(l => (l.HoraFin - l.HoraInicio).TotalHours);

            if (horasYaUsadas + horasSolicitudActual > 3.0)
            {
                double restantes = 3.0 - horasYaUsadas;
                if (restantes < 0) restantes = 0;

                return BadRequest(
                    $"El trabajador ya utilizó {horasYaUsadas:0.##} h de permiso temporal este mes. " +
                    $"Sólo le quedan {restantes:0.##} h de las 3 h mensuales permitidas.");
            }
        }

        // ======================================================
        //  EVITAR CHOQUE CON VACACIONES / PERMISOS (Solicitudes) ⭐
        // ======================================================
        var haySolicitudAprobada = await _db.Vacaciones
            .Include(s => s.EstadoSolicitud)
            .Where(s => s.IdTrabajador == dto.IdTrabajador
                        && s.EstadoSolicitud.Categoria == "EstadoSolicitud"
                        && s.EstadoSolicitud.ValorCategoria == "Aprobado"
                        && s.FechaInicio <= fechaFin
                        && s.FechaFin >= fechaInicio)
            .AnyAsync();

        if (haySolicitudAprobada)
        {
            return BadRequest("El rango de la licencia se solapa con una vacación o permiso ya aprobado.");
        }

        // ======================================================
        //  EVITAR SOLAPAMIENTO CON OTRAS LICENCIAS             ⭐
        // ======================================================
        var licenciasSolapadas = await _db.Licencias
            .Include(l => l.EstadoLicencia)
            .Include(l => l.TipoLicencia)
            .Where(l =>
                l.IdTrabajador == dto.IdTrabajador &&
                l.EstadoLicencia.ValorCategoria != "Rechazado" &&
                l.FechaInicio <= fechaFin &&
                l.FechaFin >= fechaInicio)
            .ToListAsync();

        if (esPermisoTemporal)
        {
            // 1) NO permitir permiso temporal sobre un día que ya tiene otra licencia NO temporal
            var licenciasNoTemporales = licenciasSolapadas
                .Where(l =>
                    l.TipoLicencia != null &&
                    !string.Equals(l.TipoLicencia.ValorCategoria, "Permiso temporal", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (licenciasNoTemporales.Any())
            {
                var existente = licenciasNoTemporales.First();
                return BadRequest(
                    $"El permiso temporal se solapa en fechas con una licencia de tipo '{existente.TipoLicencia.ValorCategoria}' " +
                    $"del {existente.FechaInicio:dd/MM/yyyy} al {existente.FechaFin:dd/MM/yyyy}.");
            }

            // 2) Permitir varios permisos temporales el MISMO día,
            //    pero sin solapamiento de HORARIOS
            var permisosTemporalesMismoDia = licenciasSolapadas
                .Where(l =>
                    l.TipoLicencia != null &&
                    string.Equals(l.TipoLicencia.ValorCategoria, "Permiso temporal", StringComparison.OrdinalIgnoreCase) &&
                    l.FechaInicio.Date == fechaInicio.Date &&
                    l.FechaFin.Date == fechaFin.Date)
                .ToList();

            // ¿Algún permiso temporal existente se cruza en horas con el nuevo?
            var permisoConHorasSolapadas = permisosTemporalesMismoDia.FirstOrDefault(l =>
                l.HoraInicio < horaFin &&   // inicio existente < fin nuevo
                l.HoraFin > horaInicio      // fin existente > inicio nuevo
            );

            if (permisoConHorasSolapadas != null)
            {
                return BadRequest(
                    $"El horario solicitado ({horaInicio:hh\\:mm} - {horaFin:hh\\:mm}) " +
                    $"se solapa con otro permiso temporal ya registrado " +
                    $"({permisoConHorasSolapadas.HoraInicio:hh\\:mm} - {permisoConHorasSolapadas.HoraFin:hh\\:mm}) " +
                    $"el día {fechaInicio:dd/MM/yyyy}.");
            }

            // Si llega aquí: ✔ puede tener varios permisos temporales ese día, sin solaparse y respetando las 3h mensuales (que ya validas más arriba)
        }
        else
        {
            // Licencias normales: seguir con la lógica antigua (no permitir solape de fechas)
            if (licenciasSolapadas.Any())
            {
                var existente = licenciasSolapadas.First();
                return BadRequest(
                    $"El rango seleccionado se solapa con otra licencia de tipo '{existente.TipoLicencia.ValorCategoria}' " +
                    $"del {existente.FechaInicio:dd/MM/yyyy} al {existente.FechaFin:dd/MM/yyyy}.");
            }
        }


        // ======================================================
        //  LIMITAR LICENCIAS ÚNICAS (Luto / Duelo, Paternidad, Matrimonio) ⭐
        // ======================================================
        if (nombreTipo == "Luto / Duelo" ||
            nombreTipo == "Paternidad" ||
            nombreTipo == "Matrimonio")
        {
            bool yaExisteMismaLicencia = await _db.Licencias.AnyAsync(l =>
                l.IdTrabajador == dto.IdTrabajador &&
                l.IdTipoLicencia == tipoLicencia.IdClasificador &&
                l.FechaInicio == fechaInicio &&
                l.FechaFin == fechaFin &&
                l.HoraInicio == horaInicio &&
                l.HoraFin == horaFin);

            if (yaExisteMismaLicencia)
            {
                return BadRequest(
                    $"Ya existe una licencia de tipo '{nombreTipo}' para este trabajador " +
                    $"con el mismo rango de fechas y horas.");
            }
        }

        // ======================================================
        //  CANTIDAD DE JORNADAS (usando tu helper actual)
        // ======================================================
        var cantidadJornadas = LicenciaHelper.CalcularCantidadJornadas(
            new LicenciaCrearDTO
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                HoraInicio = horaInicio,
                HoraFin = horaFin
            },
            horarios);

        // ======================================================
        //  CREAR LICENCIA EN ESTADO PENDIENTE
        // ======================================================
        var licencia = new Licencia
        {
            IdTrabajador = dto.IdTrabajador,
            IdTipoLicencia = tipoLicencia.IdClasificador,
            IdEstadoLicencia = estadoPendiente.IdClasificador,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            HoraInicio = horaInicio,
            HoraFin = horaFin,
            CantidadJornadas = cantidadJornadas,
            Motivo = dto.Motivo,
            Observacion = dto.Observacion,
            ArchivoJustificativo = dto.ArchivoJustificativo ?? Array.Empty<byte>(),
            FechaRegistro = DateTime.Now
        };

        _db.Licencias.Add(licencia);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(ObtenerLicencias), new { id = licencia.IdLicencia }, licencia.IdLicencia);
    }


    [HttpGet("{id:int}/archivo")]
    public async Task<IActionResult> DescargarArchivoJustificativo(int id)
    {
        var licencia = await _db.Licencias
            .FirstOrDefaultAsync(l => l.IdLicencia == id);

        if (licencia is null)
            return NotFound("Licencia no encontrada.");

        if (licencia.ArchivoJustificativo == null || licencia.ArchivoJustificativo.Length == 0)
            return NotFound("La licencia no tiene archivo justificativo.");

        var nombreArchivo = $"Justificativo_Licencia_{id}.pdf";

        return File(
            licencia.ArchivoJustificativo,
            "application/pdf",
            nombreArchivo
        );
    }


    // PUT api/Licencias/5/aprobar
    [HttpPut("{id:int}/aprobar")]
    public async Task<IActionResult> AprobarLicencia(int id)
    {
        var rolActual = GetRolUsuarioActual();
        if (string.IsNullOrWhiteSpace(rolActual))
            return Forbid("No se pudo determinar el rol del usuario actual.");

        var licencia = await _db.Licencias
            .Include(l => l.Trabajador)
                .ThenInclude(t => t.Cargo)
            .Include(l => l.EstadoLicencia)
            .FirstOrDefaultAsync(l => l.IdLicencia == id);

        if (licencia is null)
            return NotFound("Licencia no encontrada.");

        var cargoTrabajador = licencia.Trabajador?.Cargo?.NombreCargo;

        if (!PuedeGestionarSegunRol(rolActual, cargoTrabajador))
            return Forbid("No tiene permiso para aprobar la licencia de este trabajador.");

        var idAprobado = await _db.Clasificadores
            .Where(c => c.Categoria == "EstadoSolicitud" && c.ValorCategoria == "Aprobado")
            .Select(c => c.IdClasificador)
            .FirstOrDefaultAsync();

        if (idAprobado == 0)
            return StatusCode(500, "No está configurado el estado 'Aprobado' en Clasificador.");

        licencia.IdEstadoLicencia = idAprobado;
        licencia.FechaAprobacion = DateTime.Today;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // PUT api/Licencias/5/rechazar
    [HttpPut("{id:int}/rechazar")]
    public async Task<IActionResult> RechazarLicencia(int id)
    {
        var rolActual = GetRolUsuarioActual();
        if (string.IsNullOrWhiteSpace(rolActual))
            return Forbid("No se pudo determinar el rol del usuario actual.");

        var licencia = await _db.Licencias
            .Include(l => l.Trabajador)
                .ThenInclude(t => t.Cargo)
            .FirstOrDefaultAsync(l => l.IdLicencia == id);

        if (licencia is null)
            return NotFound("Licencia no encontrada.");

        var cargoTrabajador = licencia.Trabajador?.Cargo?.NombreCargo;

        if (!PuedeGestionarSegunRol(rolActual, cargoTrabajador))
            return Forbid("No tiene permiso para rechazar la licencia de este trabajador.");

        var idRechazado = await _db.Clasificadores
            .Where(c => c.Categoria == "EstadoSolicitud" && c.ValorCategoria == "Rechazado")
            .Select(c => c.IdClasificador)
            .FirstOrDefaultAsync();

        if (idRechazado == 0)
            return StatusCode(500, "No está configurado el estado 'Rechazado' en Clasificador.");

        licencia.IdEstadoLicencia = idRechazado;
        licencia.FechaAprobacion = DateTime.Today;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ==================== ELIMINAR LICENCIA =====================
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> EliminarLicencia(int id)
    {
        var rolActual = GetRolUsuarioActual();
        if (string.IsNullOrWhiteSpace(rolActual))
            return Forbid("No se pudo determinar el rol del usuario actual.");

        var licencia = await _db.Licencias
            .Include(l => l.Trabajador)
                .ThenInclude(t => t.Cargo)
            .Include(l => l.EstadoLicencia)
            .FirstOrDefaultAsync(l => l.IdLicencia == id);

        if (licencia is null)
            return NotFound("Licencia no encontrada.");

        var cargoTrabajador = licencia.Trabajador?.Cargo?.NombreCargo;

        if (!PuedeGestionarSegunRol(rolActual, cargoTrabajador))
            return Forbid("No tiene permiso para eliminar la licencia de este trabajador.");

        // Solo permitir eliminar si está 'Pendiente'
        if (!string.Equals(licencia.EstadoLicencia?.ValorCategoria, "Pendiente", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Solo se pueden eliminar licencias en estado 'Pendiente'.");

        _db.Licencias.Remove(licencia);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // helpers internos del controlador
    private static Horario? ObtenerHorarioDia(List<Horario> horarios, DateTime fecha)
    {
        var cultura = new CultureInfo("es-ES");
        string diaSemana = fecha.ToString("dddd", cultura);
        diaSemana = char.ToUpper(diaSemana[0]) + diaSemana.Substring(1);
        return horarios.FirstOrDefault(h => h.DiaSemana == diaSemana);
    }

    private static void AjustarHorasJornadaCompleta(
        List<Horario> horarios,
        DateTime fechaInicio,
        DateTime fechaFin,
        out TimeSpan horaMinima,
        out TimeSpan horaMaxima)
    {
        horaMinima = TimeSpan.MaxValue;
        horaMaxima = TimeSpan.Zero;

        for (var f = fechaInicio.Date; f <= fechaFin.Date; f = f.AddDays(1))
        {
            var h = ObtenerHorarioDia(horarios, f);
            if (h is null) continue;

            if (h.HoraEntrada < horaMinima) horaMinima = h.HoraEntrada;
            if (h.HoraSalida > horaMaxima) horaMaxima = h.HoraSalida;
        }

        if (horaMinima == TimeSpan.MaxValue || horaMaxima == TimeSpan.Zero)
        {
            horaMinima = TimeSpan.FromHours(8);
            horaMaxima = TimeSpan.FromHours(16);
        }
    }

    private static int ContarDiasHabiles(DateTime inicio, DateTime fin)
    {
        if (fin < inicio)
            return 0;

        int dias = 0;
        for (var fecha = inicio.Date; fecha <= fin.Date; fecha = fecha.AddDays(1))
        {
            if (fecha.DayOfWeek != DayOfWeek.Saturday &&
                fecha.DayOfWeek != DayOfWeek.Sunday)
            {
                dias++;
            }
        }
        return dias;
    }

}

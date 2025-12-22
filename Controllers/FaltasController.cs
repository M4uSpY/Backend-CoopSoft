using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Faltas;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,Consejo")]
    [ApiController]
    public class FaltasController : ControllerBase
    {

        private const string CATEGORIA_TIPO_FALTA = "TipoFalta";
        private const string VALOR_INASISTENCIA_LABORAL = "Inasistencia laboral";

        private const string CATEGORIA_ESTADO_SOLICITUD = "EstadoSolicitud";
        private const string ESTADO_APROBADO = "Aprobado";
        private const string TIPO_PERMISO_TEMPORAL = "Permiso temporal";

        private readonly AppDbContext _db;


        private readonly IMapper _mapper;
        public FaltasController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerFaltas()
        {
            var faltas = await _db.Faltas
                .Where(f => f.EstadoFalta)
                .Include(f => f.Trabajador).ThenInclude(t => t.Persona)
                .Include(f => f.TipoFalta).ToListAsync();
            var faltasDTO = _mapper.Map<List<ListarFaltasDTO>>(faltas);
            return Ok(faltasDTO);
        }

        [HttpPost("{id}/justificativo")]
        public async Task<IActionResult> SubirJustificativo(int id, [FromForm] ArchivoJustificativoDTO dto)
        {
            var falta = await _db.Faltas.FindAsync(id);
            if (falta is null)
            {
                return NotFound("La falta no fue encontrada");
            }

            var idUsuarioActual = ObtenerIdUsuarioActual();
            if (idUsuarioActual is null)
            {
                return Unauthorized("No se pudo identificar al usuario que modifica.");
            }

            var teniaArchivoAntes = falta.ArchivoJustificativo != null && falta.ArchivoJustificativo.Length > 0;


            using var ms = new MemoryStream();
            await dto.Archivo.CopyToAsync(ms);
            falta.ArchivoJustificativo = ms.ToArray();

            var historico = new HistoricoFalta
            {
                IdFalta = falta.IdFalta,
                UsuarioModificoId = idUsuarioActual.Value,
                FechaModificacion = DateTime.Now,
                Accion = teniaArchivoAntes ? "ACTUALIZAR" : "CREAR",
                Campo = "ArchivoJustificativo",
                ValorAnterior = teniaArchivoAntes ? "ConArchivo" : "SinArchivo",
                ValorActual = "ConArchivo"
            };

            await _db.HistoricosFalta.AddAsync(historico);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id}/justificativo")]
        public async Task<IActionResult> DescargarJustificativo(int id)
        {
            var falta = await _db.Faltas.FindAsync(id);
            if (falta is null)
            {
                return NotFound("La falta no fue encontrada");
            }

            if (falta.ArchivoJustificativo == null || falta.ArchivoJustificativo.Length == 0)
            {
                return NotFound("La falta no tienen archivo justificativo");
            }
            var fileName = $"Justificativo_Falta_{id}.pdf";
            return File(falta.ArchivoJustificativo, "application/octet-stream", fileName);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarFalta(int id)
        {
            var falta = await _db.Faltas.FindAsync(id);
            if (falta is null)
                return NotFound("Falta no encontrada");

            var idUsuarioActual = ObtenerIdUsuarioActual();
            if (idUsuarioActual is null)
                return Unauthorized("No se pudo identificar al usuario que modifica.");

            // Estado anterior (por si quieres auditarlo)
            var estadoAnterior = falta.EstadoFalta;

            // 👉 Solo se inactiva
            falta.EstadoFalta = false;

            // Registrar histórico de "inactivación"
            var historico = new HistoricoFalta
            {
                IdFalta = falta.IdFalta,
                UsuarioModificoId = idUsuarioActual.Value,
                FechaModificacion = DateTime.Now,
                Accion = "INACTIVAR",
                Campo = "EstadoFalta",
                ValorAnterior = estadoAnterior.ToString(),
                ValorActual = falta.EstadoFalta.ToString()
            };

            await _db.HistoricosFalta.AddAsync(historico);

            await _db.SaveChangesAsync();

            return NoContent();
        }


        private int? ObtenerIdUsuarioActual()
        {
            var claimSub = User.FindFirst(JwtRegisteredClaimNames.Sub);
            var claimNameId = User.FindFirst(ClaimTypes.NameIdentifier);
            var claim = claimSub ?? claimNameId;

            if (claim is null)
                return null;

            return int.TryParse(claim.Value, out var idUsuario)
                ? idUsuario
                : (int?)null;
        }

        // Devuelve las fechas (L-V) que son faltas injustificadas:
        // sin jornada completa, sin vacación, sin licencia que cuente como trabajada.
        private async Task<List<DateTime>> ObtenerDiasInjustificadosAsync(
    int idTrabajador,
    DateTime desde,
    DateTime hasta)
        {
            // 👉 Incluimos los horarios del trabajador
            var trabajador = await _db.Trabajadores
                .Include(t => t.Horarios)
                .FirstOrDefaultAsync(t => t.IdTrabajador == idTrabajador);

            if (trabajador is null)
                return new List<DateTime>();

            var fechaIngreso = trabajador.FechaIngreso.Date;

            var inicio = desde.Date;
            var fin = hasta.Date;

            var hoy = DateTime.Today.Date;
            var fechaCorte = hoy.AddDays(-1);

            var finEfectivo = fin <= fechaCorte ? fin : fechaCorte;
            if (finEfectivo < inicio)
                return new List<DateTime>();

            var asistencias = await _db.Asistencias
                .Where(a => a.IdTrabajador == idTrabajador
                            && a.Fecha >= inicio
                            && a.Fecha <= finEfectivo)
                .ToListAsync();

            var vacaciones = await _db.Vacaciones
                .Include(s => s.EstadoSolicitud)
                .Where(s => s.IdTrabajador == idTrabajador
                            && s.EstadoSolicitud.Categoria == CATEGORIA_ESTADO_SOLICITUD
                            && s.EstadoSolicitud.ValorCategoria == ESTADO_APROBADO
                            && s.FechaFin >= inicio
                            && s.FechaInicio <= finEfectivo)
                .ToListAsync();

            var licencias = await _db.Licencias
                .Include(l => l.EstadoLicencia)
                .Include(l => l.TipoLicencia)
                .Where(l => l.IdTrabajador == idTrabajador
                            && l.EstadoLicencia.Categoria == CATEGORIA_ESTADO_SOLICITUD
                            && l.EstadoLicencia.ValorCategoria == ESTADO_APROBADO
                            && l.FechaFin >= inicio
                            && l.FechaInicio <= finEfectivo)
                .ToListAsync();

            var diasInjustificados = new List<DateTime>();

            var cultura = new CultureInfo("es-ES");

            for (var fecha = inicio; fecha <= finEfectivo; fecha = fecha.AddDays(1))
            {
                if (fecha < fechaIngreso)
                    continue;

                // Solo lunes–viernes
                if (fecha.DayOfWeek == DayOfWeek.Saturday ||
                    fecha.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                // 🔴 NUEVO: solo considerar días donde el trabajador TIENE horario
                string diaSemana = fecha.ToString("dddd", cultura);
                diaSemana = char.ToUpper(diaSemana[0]) + diaSemana.Substring(1);

                bool tieneHorarioEseDia = trabajador.Horarios
                    .Any(h => h.DiaSemana == diaSemana);

                if (!tieneHorarioEseDia)
                    continue; // ese día no se espera que trabaje → no es falta

                // =========================
                // Lógica actual de jornada / vacación / licencia
                // =========================

                bool tieneEntrada = asistencias.Any(a => a.Fecha == fecha && a.EsEntrada);
                bool tieneSalida = asistencias.Any(a => a.Fecha == fecha && !a.EsEntrada);
                bool jornadaCompleta = tieneEntrada && tieneSalida;

                bool diaEnVacacion = vacaciones.Any(v =>
                    v.FechaInicio.Date <= fecha && v.FechaFin.Date >= fecha);

                var licenciasDia = licencias
                    .Where(l => l.FechaInicio.Date <= fecha && l.FechaFin.Date >= fecha)
                    .ToList();

                bool diaConLicenciaNoTemporal = licenciasDia.Any(l =>
                    l.TipoLicencia != null &&
                    !string.Equals(
                        l.TipoLicencia.ValorCategoria,
                        TIPO_PERMISO_TEMPORAL,
                        StringComparison.OrdinalIgnoreCase));

                bool diaConPermisoTemporal = licenciasDia.Any(l =>
                    l.TipoLicencia != null &&
                    string.Equals(
                        l.TipoLicencia.ValorCategoria,
                        TIPO_PERMISO_TEMPORAL,
                        StringComparison.OrdinalIgnoreCase));

                bool diaEnLicenciaQueCuentaComoTrabajada =
                    diaConLicenciaNoTemporal ||
                    (diaConPermisoTemporal && jornadaCompleta);

                bool esDiaPagado =
                    jornadaCompleta || diaEnVacacion || diaEnLicenciaQueCuentaComoTrabajada;

                if (!esDiaPagado)
                    diasInjustificados.Add(fecha);
            }

            return diasInjustificados;
        }


        /// <summary>
        /// Genera faltas de "Inasistencia laboral" para TODOS los trabajadores activos,
        /// en el rango [desde, hasta], usando asistencias, vacaciones y licencias.
        /// </summary>
        [HttpPost("generar-inasistencias")]
        public async Task<IActionResult> GenerarInasistencias(
            [FromQuery] DateTime desde,
            [FromQuery] DateTime hasta)
        {
            if (desde.Date > hasta.Date)
                return BadRequest("La fecha 'desde' no puede ser mayor que 'hasta'.");

            // Buscar IdTipoFalta = "Inasistencia laboral"
            var idTipoInasistencia = await _db.Clasificadores
                .Where(c => c.Categoria == CATEGORIA_TIPO_FALTA &&
                            c.ValorCategoria == VALOR_INASISTENCIA_LABORAL)
                .Select(c => c.IdClasificador)
                .FirstOrDefaultAsync();

            if (idTipoInasistencia == 0)
                return BadRequest("No se encontró el tipo de falta 'Inasistencia laboral' en Clasificador.");

            // Trabajadores activos
            var trabajadores = await _db.Trabajadores
                .Where(t => t.EstadoTrabajador)
                .ToListAsync();

            if (!trabajadores.Any())
                return BadRequest("No hay trabajadores activos.");

            int totalCreadas = 0;

            foreach (var t in trabajadores)
            {
                var diasInjustificados = await ObtenerDiasInjustificadosAsync(
                    t.IdTrabajador,
                    desde,
                    hasta);

                if (!diasInjustificados.Any())
                    continue;

                // Fechas donde YA existe falta de inasistencia activa
                var fechasConFalta = await _db.Faltas
                    .Where(f =>
                        f.IdTrabajador == t.IdTrabajador &&
                        f.IdTipoFalta == idTipoInasistencia &&
                        f.EstadoFalta &&
                        f.Fecha >= desde.Date &&
                        f.Fecha <= hasta.Date)
                    .Select(f => f.Fecha.Date)
                    .Distinct()
                    .ToListAsync();

                var nuevasFechas = diasInjustificados
                    .Where(fecha => !fechasConFalta.Contains(fecha))
                    .ToList();

                foreach (var fecha in nuevasFechas)
                {
                    var falta = new Falta
                    {
                        IdTrabajador = t.IdTrabajador,
                        IdTipoFalta = idTipoInasistencia,
                        Fecha = fecha,
                        Descripcion = "Inasistencia laboral injustificada.",
                        EstadoFalta = true,
                        ArchivoJustificativo = Array.Empty<byte>()
                    };

                    _db.Faltas.Add(falta);
                    totalCreadas++;
                }
            }

            if (totalCreadas == 0)
                return Ok("No se generaron nuevas faltas de inasistencia laboral en el rango indicado.");

            await _db.SaveChangesAsync();
            return Ok($"Se generaron {totalCreadas} faltas de inasistencia laboral entre {desde:yyyy-MM-dd} y {hasta:yyyy-MM-dd}.");
        }


    }
}

using System.Globalization;
using AutoMapper;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Asistencia;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    [ApiController]
    public class AsistenciasController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        // (SOLO PARA PRUEBAS - CAMBIARLO)
        private const int MINUTOS_MINIMOS_JORNADA = 4;

        // (SOLO PARA PRUEBAS - CAMBIARLO)
        private const int MINUTOS_TOLERANCIA_RETRASO = 2;

        // IdTipoFalta para ATRASO (Clasificador)
        private const int ID_TIPO_FALTA_ATRASO = 18;



        public AsistenciasController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerAsistencias()
        {
            var asistencias = await _db.Asistencias.Include(a => a.Trabajador).ThenInclude(t => t.Persona).Include(a => a.Trabajador).ThenInclude(t => t.Cargo)
                .ThenInclude(c => c.Oficina).ToListAsync();
            var asistenciaDTO = _mapper.Map<List<AsistenciaListaDTO>>(asistencias);
            return Ok(asistenciaDTO);
        }

        [HttpPost]
        public async Task<IActionResult> RegistrarAsistencia([FromBody] AsistenciaCrearDTO dto)
        {
            var hoy = dto.Fecha.Date;
            var fechaHoraMarcacion = dto.Fecha.Date + dto.Hora;

            // =========================================================
            // 0) LICENCIAS APROBADAS
            // =========================================================
            if (await TrabajadorEnLicenciaAsync(dto.IdTrabajador, fechaHoraMarcacion))
            {
                return Ok(new AsistenciaRegistrarResultadoDTO
                {
                    Registrado = false,
                    TipoMarcacion = "EN_PROCESO",
                    Mensaje = "No es posible registrar asistencia porque el trabajador se encuentra con una licencia autorizada en este horario."
                });
            }

            // =========================================================
            // 1) VACACIONES / PERMISOS APROBADOS (Solicitud)
            // =========================================================
            var vacacionHoy = await _db.Vacaciones
                .Include(s => s.EstadoSolicitud)
                .Where(s => s.IdTrabajador == dto.IdTrabajador
                            && s.FechaInicio <= hoy
                            && s.FechaFin >= hoy
                            && s.EstadoSolicitud.Categoria == "EstadoSolicitud"
                            && s.EstadoSolicitud.ValorCategoria == "Aprobado")
                .FirstOrDefaultAsync();

            if (vacacionHoy != null)
            {
                return Ok(new AsistenciaRegistrarResultadoDTO
                {
                    Registrado = false,
                    TipoMarcacion = "EN_PROCESO",
                    Mensaje = "No es posible registrar asistencia porque el trabajador se encuentra de vacación o con un permiso aprobado en esta fecha."
                });
            }

            // =========================================================
            // 2) Trabajador + Horario
            // =========================================================
            var trabajador = await _db.Trabajadores
                .Include(t => t.Horarios)
                .FirstOrDefaultAsync(t => t.IdTrabajador == dto.IdTrabajador);

            if (trabajador is null)
            {
                return NotFound(new AsistenciaRegistrarResultadoDTO
                {
                    Registrado = false,
                    TipoMarcacion = "EN_PROCESO",
                    Mensaje = "Trabajador no encontrado."
                });
            }

            // Obtener el horario de hoy (según DiaSemana = "Lunes", "Martes", etc.)
            var cultura = new CultureInfo("es-ES");
            string diaSemanaActual = dto.Fecha.ToString("dddd", cultura);
            diaSemanaActual = char.ToUpper(diaSemanaActual[0]) + diaSemanaActual.Substring(1);

            var horarioHoy = trabajador.Horarios
                .FirstOrDefault(h => h.DiaSemana == diaSemanaActual);

            if (horarioHoy is null)
            {
                return Ok(new AsistenciaRegistrarResultadoDTO
                {
                    Registrado = false,
                    TipoMarcacion = "EN_PROCESO",
                    Mensaje = "El trabajador no tiene un horario asignado para el día de hoy."
                });
            }

            // =========================================================
            // 3) Asistencias del día
            // =========================================================
            var asistenciasHoy = await _db.Asistencias
                .Where(a => a.IdTrabajador == dto.IdTrabajador && a.Fecha == hoy)
                .OrderBy(a => a.Hora)
                .ToListAsync();

            var horaMarcada = hoy + dto.Hora;

            // ⭐ NUEVO: ajustar hora de entrada esperada según licencias
            var horaEntradaReprogramada = await ObtenerHoraEntradaEsperadaAsync(
                dto.IdTrabajador,
                hoy,
                horarioHoy.HoraEntrada);

            var horaEntradaProgramada = hoy + horaEntradaReprogramada;

            // =========================================================
            // 4) PRIMERA MARCACIÓN (ENTRADA)
            // =========================================================
            if (!asistenciasHoy.Any())
            {
                // Rango permitido para ENTRADA: ±30 minutos
                var rangoInicio = horaEntradaProgramada.AddMinutes(-30);
                var rangoFin = horaEntradaProgramada.AddMinutes(30);

                if (horaMarcada < rangoInicio || horaMarcada > rangoFin)
                {
                    return Ok(new AsistenciaRegistrarResultadoDTO
                    {
                        Registrado = false,
                        TipoMarcacion = "EN_PROCESO",
                        Mensaje = $"Marcación fuera del rango permitido para ENTRADA. " +
                                  $"Debe marcar entre {rangoInicio:HH:mm} y {rangoFin:HH:mm}."
                    });
                }

                // Tolerancia de 2 minutos antes de considerar atraso
                var limiteSinAtraso = horaEntradaProgramada.AddMinutes(2);
                bool esAtrasado = horaMarcada > limiteSinAtraso;

                double minutosRetraso = 0;
                if (esAtrasado)
                    minutosRetraso = (horaMarcada - horaEntradaProgramada).TotalMinutes;

                var asistenciaEntrada = _mapper.Map<Asistencia>(dto);
                asistenciaEntrada.Fecha = hoy;
                asistenciaEntrada.Hora = dto.Hora;
                asistenciaEntrada.EsEntrada = true;

                _db.Asistencias.Add(asistenciaEntrada);

                // FALTA POR ATRASO (si corresponde)
                if (esAtrasado)
                {
                    bool yaTieneFaltaHoy = await _db.Faltas
                        .AnyAsync(f => f.IdTrabajador == dto.IdTrabajador
                                       && f.Fecha == hoy
                                       && f.IdTipoFalta == ID_TIPO_FALTA_ATRASO);

                    if (!yaTieneFaltaHoy)
                    {
                        var falta = new Falta
                        {
                            IdTrabajador = dto.IdTrabajador,
                            IdTipoFalta = ID_TIPO_FALTA_ATRASO,
                            Fecha = hoy,
                            Descripcion = $"Falta por atraso. Llegó {Math.Round(minutosRetraso)} minuto(s) tarde " +
                                          $"respecto a la hora de entrada programada ({horaEntradaProgramada:HH:mm}).",
                            ArchivoJustificativo = Array.Empty<byte>()
                        };

                        _db.Faltas.Add(falta);
                    }
                }

                // Guardamos ENTRADA (+ posible falta)
                await _db.SaveChangesAsync();

                string mensajeEntrada = esAtrasado
                    ? $"Entrada registrada a las {horaMarcada:HH:mm}. Llegó {Math.Round(minutosRetraso)} minuto(s) tarde. Se generó una falta por atraso."
                    : $"Entrada registrada correctamente a las {horaMarcada:HH:mm}.";

                return Ok(new AsistenciaRegistrarResultadoDTO
                {
                    Registrado = true,
                    TipoMarcacion = "ENTRADA",
                    HoraEntrada = hoy + asistenciaEntrada.Hora,
                    Mensaje = mensajeEntrada
                });
            }

            // =========================================================
            // 5) SEGUNDA MARCACIÓN (SALIDA)
            // =========================================================
            var entrada = asistenciasHoy.FirstOrDefault(a => a.EsEntrada);
            var salidaRegistrada = asistenciasHoy.FirstOrDefault(a => !a.EsEntrada);

            if (entrada is null)
            {
                return Ok(new AsistenciaRegistrarResultadoDTO
                {
                    Registrado = false,
                    TipoMarcacion = "EN_PROCESO",
                    Mensaje = "No existe una ENTRADA registrada para hoy. Primero debe registrar la ENTRADA."
                });
            }

            if (salidaRegistrada != null)
            {
                return Ok(new AsistenciaRegistrarResultadoDTO
                {
                    Registrado = false,
                    TipoMarcacion = "COMPLETADO",
                    Mensaje = "Ya registraste ENTRADA y SALIDA hoy."
                });
            }

            // Tiempo mínimo para poder marcar salida
            var horaEntradaReal = hoy + entrada.Hora;
            var minimoSalida = horaEntradaReal.AddMinutes(MINUTOS_MINIMOS_JORNADA);

            if (horaMarcada < minimoSalida)
            {
                var faltan = minimoSalida - horaMarcada;

                return Ok(new AsistenciaRegistrarResultadoDTO
                {
                    Registrado = false,
                    TipoMarcacion = "EN_PROCESO",
                    HoraEntrada = horaEntradaReal,
                    Mensaje = $"Aún no puede marcar SALIDA. " +
                              $"Faltan {faltan.Minutes:D2} minuto(s) para completar la jornada mínima."
                });
            }

            var asistenciaSalida = _mapper.Map<Asistencia>(dto);
            asistenciaSalida.Fecha = hoy;
            asistenciaSalida.Hora = dto.Hora;
            asistenciaSalida.EsEntrada = false;

            _db.Asistencias.Add(asistenciaSalida);
            await _db.SaveChangesAsync();

            return Ok(new AsistenciaRegistrarResultadoDTO
            {
                Registrado = true,
                TipoMarcacion = "SALIDA",
                HoraEntrada = hoy + entrada.Hora,
                HoraSalida = hoy + asistenciaSalida.Hora,
                Mensaje = $"Salida registrada correctamente a las {horaMarcada:HH:mm}."
            });
        }

        private async Task<bool> TrabajadorEnLicenciaAsync(int idTrabajador, DateTime fechaHoraMarcacion)
        {
            var fecha = fechaHoraMarcacion.Date;

            var licencias = await _db.Licencias
                .Include(l => l.EstadoLicencia)
                .Where(l => l.IdTrabajador == idTrabajador
                            && l.FechaInicio <= fecha
                            && l.FechaFin >= fecha
                            && l.EstadoLicencia.Categoria == "EstadoSolicitud"
                            && l.EstadoLicencia.ValorCategoria == "Aprobado")
                .ToListAsync();

            foreach (var l in licencias)
            {
                var inicio = l.FechaInicio.Date + l.HoraInicio;
                var fin = l.FechaFin.Date + l.HoraFin;

                if (fechaHoraMarcacion >= inicio && fechaHoraMarcacion <= fin)
                    return true;
            }

            return false;
        }

        private async Task<TimeSpan> ObtenerHoraEntradaEsperadaAsync(int idTrabajador, DateTime fecha, TimeSpan horaEntradaHorario)
        {
            var licencias = await _db.Licencias
                .Include(l => l.EstadoLicencia)
                .Where(l =>
                    l.IdTrabajador == idTrabajador &&
                    l.FechaInicio <= fecha &&
                    l.FechaFin >= fecha &&
                    l.EstadoLicencia.Categoria == "EstadoSolicitud" &&
                    l.EstadoLicencia.ValorCategoria == "Aprobado")
                .ToListAsync();

            var horaEsperada = horaEntradaHorario;

            foreach (var l in licencias)
            {
                var inicio = (l.FechaInicio.Date == fecha.Date)
                    ? l.HoraInicio
                    : TimeSpan.Zero;

                var fin = (l.FechaFin.Date == fecha.Date)
                    ? l.HoraFin
                    : new TimeSpan(23, 59, 59);

                // Si la licencia cubre la hora de entrada normal, corremos la hora de entrada al fin de la licencia
                if (inicio <= horaEntradaHorario && fin > horaEntradaHorario)
                {
                    if (fin > horaEsperada)
                        horaEsperada = fin;
                }
            }

            return horaEsperada;
        }
    }
}

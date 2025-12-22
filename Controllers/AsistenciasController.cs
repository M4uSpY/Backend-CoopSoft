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
    [Authorize(Roles = "Administrador, Consejo")]
    [ApiController]
    public class AsistenciasController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        private const int MINUTOS_MINIMOS_JORNADA = 480;

        // (SOLO PARA PRUEBAS - CAMBIARLO)
        private const int MINUTOS_TOLERANCIA_RETRASO = 2;

        // IdTipoFalta para ATRASO (Clasificador)
        private const int ID_TIPO_FALTA_ATRASO = 18;

        private const int ID_TIPO_LICENCIA_CUMPLEANIOS = 24;



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
        [AllowAnonymous]
        public async Task<IActionResult> RegistrarAsistencia([FromBody] AsistenciaCrearDTO dto)
        {
            var hoy = dto.Fecha.Date;
            var fechaHoraMarcacion = dto.Fecha.Date + dto.Hora;

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

            // ajustar hora de entrada esperada según licencias

            bool esSalida = asistenciasHoy.Any(a => a.EsEntrada) && !asistenciasHoy.Any(a => !a.EsEntrada);

            // SOLO validar licencias bloqueantes para ENTRADA
            if (!asistenciasHoy.Any()) // primera marcación del día = ENTRADA
            {
                if (await TrabajadorEnLicenciaAsync(dto.IdTrabajador, fechaHoraMarcacion, esSalida: false))
                {
                    return Ok(new AsistenciaRegistrarResultadoDTO
                    {
                        Registrado = false,
                        TipoMarcacion = "EN_PROCESO",
                        Mensaje = "No es posible registrar asistencia porque el trabajador se encuentra con una licencia autorizada en este horario."
                    });
                }
            }


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
                var limiteSinAtraso = horaEntradaProgramada.AddMinutes(MINUTOS_TOLERANCIA_RETRASO);
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

            var permisoFinal = await ObtenerPermisoTemporalFinalDelDiaAsync(
    dto.IdTrabajador,
    hoy,
    horarioHoy.HoraSalida);

            bool marcandoDentroPermisoFinal = false;

            if (permisoFinal != null)
            {
                var inicioPermisoFinal = hoy + permisoFinal.HoraInicio;
                var finPermisoFinal = hoy + permisoFinal.HoraFin;

                // ¿La hora de salida está dentro del tramo del permiso final?
                marcandoDentroPermisoFinal =
                    horaMarcada >= inicioPermisoFinal &&
                    horaMarcada <= finPermisoFinal;
            }

            // Minutos de permiso temporal EN LA JORNADA de hoy
            int minutosPermisoTemporal = await ObtenerMinutosPermisoTemporalDiaAsync(
                dto.IdTrabajador,
                hoy,
                horarioHoy.HoraEntrada,
                horarioHoy.HoraSalida);


            // Jornada mínima efectiva = 8h - minutos de permiso temporal
            int minutosJornadaEfectiva = MINUTOS_MINIMOS_JORNADA - minutosPermisoTemporal;
            if (minutosJornadaEfectiva < 0)
                minutosJornadaEfectiva = 0;

            var minimoSalida = horaEntradaReal.AddMinutes(minutosJornadaEfectiva);


            if (!marcandoDentroPermisoFinal && horaMarcada < minimoSalida)
            {
                var faltan = minimoSalida - horaMarcada;

                int horasFaltan = (int)faltan.TotalHours;
                int minutosFaltan = faltan.Minutes;

                return Ok(new AsistenciaRegistrarResultadoDTO
                {
                    Registrado = false,
                    TipoMarcacion = "EN_PROCESO",
                    HoraEntrada = horaEntradaReal,
                    Mensaje = $"Aún no puede marcar SALIDA. " +
                              $"Faltan {horasFaltan:D2} hora(s) y {minutosFaltan:D2} minuto(s) para completar la jornada mínima."
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

        private async Task<bool> TrabajadorEnLicenciaAsync(int idTrabajador, DateTime fechaHoraMarcacion, bool esSalida)
        {
            var fecha = fechaHoraMarcacion.Date;

            var licencias = await _db.Licencias
                .Include(l => l.EstadoLicencia)
                .Include(l => l.TipoLicencia)
                .Where(l => l.IdTrabajador == idTrabajador
                            && l.FechaInicio <= fecha
                            && l.FechaFin >= fecha
                            && l.EstadoLicencia.Categoria == "EstadoSolicitud"
                            && l.EstadoLicencia.ValorCategoria == "Aprobado")
                .ToListAsync();

            const string TIPO_PERMISO_TEMPORAL = "Permiso temporal";
            // const string TIPO_CUMPLEANIOS = "Cumpleaños";

            foreach (var l in licencias)
            {
                if (l.TipoLicencia == null)
                    continue;

                bool esPermisoTemporal =
            string.Equals(
                l.TipoLicencia.ValorCategoria,
                TIPO_PERMISO_TEMPORAL,
                StringComparison.OrdinalIgnoreCase);

                bool esCumpleanios = l.IdTipoLicencia == ID_TIPO_LICENCIA_CUMPLEANIOS;

                // Rango de la licencia en esa fecha
                var inicio = l.FechaInicio.Date + l.HoraInicio;
                var fin = l.FechaFin.Date + l.HoraFin;

                if (esPermisoTemporal)
                {
                    if (!esSalida)
                    {
                        // 👉 ENTRADA: bloquear SIEMPRE dentro del intervalo del permiso
                        if (fechaHoraMarcacion >= inicio && fechaHoraMarcacion <= fin)
                            return true;
                    }
                    else
                    {
                        // 👉 SALIDA:
                        // Para permisos temporales, NUNCA bloqueamos la marcación de salida.
                        // La lógica de si puede o no salir antes ya la controlas con:
                        // - minutosPermisoTemporal
                        // - minimoSalida
                        // - y el caso especial del permiso final (marcandoDentroPermisoFinal).
                    }

                    // Para cualquier permiso temporal, ya tratamos la ENTRADA / SALIDA aquí:
                    continue;
                }


                if (esCumpleanios)
                {
                    // Por ahora el cumpleaños NO bloquea la marcación (lo dejas como lo tenías)
                    continue;
                }

                // Licencias "normales": bloquean todo el intervalo
                if (fechaHoraMarcacion >= inicio && fechaHoraMarcacion <= fin)
                    return true;
            }

            return false;
        }


        private async Task<TimeSpan> ObtenerHoraEntradaEsperadaAsync(
    int idTrabajador,
    DateTime fecha,
    TimeSpan horaEntradaHorario)
        {
            const string TIPO_PERMISO_TEMPORAL = "Permiso temporal";
            // const string TIPO_CUMPLEANIOS = "Cumpleaños";

            var licencias = await _db.Licencias
                .Include(l => l.EstadoLicencia)
                .Include(l => l.TipoLicencia)
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
                if (l.TipoLicencia == null)
                    continue;

                // SOLO nos interesan los permisos que NO bloquean:
                // Permiso temporal + Cumpleaños
                bool esPermisoTemporal =
                    string.Equals(
                        l.TipoLicencia.ValorCategoria,
                        TIPO_PERMISO_TEMPORAL,
                        StringComparison.OrdinalIgnoreCase);

                bool esCumpleanios = l.IdTipoLicencia == ID_TIPO_LICENCIA_CUMPLEANIOS;

                if (!esPermisoTemporal && !esCumpleanios)
                    continue;

                // Tramo del permiso EN ESE DÍA
                var inicio = (l.FechaInicio.Date == fecha.Date)
                    ? l.HoraInicio
                    : TimeSpan.Zero;

                var fin = (l.FechaFin.Date == fecha.Date)
                    ? l.HoraFin
                    : new TimeSpan(23, 59, 59);

                // Si el permiso cubre la hora de entrada normal,
                // movemos la entrada al FIN del permiso
                if (inicio <= horaEntradaHorario && fin > horaEntradaHorario)
                {
                    if (fin > horaEsperada)
                        horaEsperada = fin;
                }
            }

            return horaEsperada;
        }



        private async Task<int> ObtenerMinutosPermisoTemporalDiaAsync(
    int idTrabajador,
    DateTime fecha,
    TimeSpan horaJornadaInicio,
    TimeSpan horaJornadaFin)
        {
            const string TIPO_PERMISO_TEMPORAL = "Permiso temporal";
            // const string TIPO_CUMPLEANIOS = "Cumpleaños";

            // Jornada de trabajo del día
            var inicioJornada = fecha.Date + horaJornadaInicio;
            var finJornada = fecha.Date + horaJornadaFin;

            var licencias = await _db.Licencias
                .Include(l => l.EstadoLicencia)
                .Include(l => l.TipoLicencia)
                .Where(l =>
                    l.IdTrabajador == idTrabajador &&
                    l.FechaInicio <= fecha &&
                    l.FechaFin >= fecha &&
                    l.EstadoLicencia.Categoria == "EstadoSolicitud" &&
                    l.EstadoLicencia.ValorCategoria == "Aprobado" &&
                    l.TipoLicencia != null &&
                    (
                        l.TipoLicencia.ValorCategoria == TIPO_PERMISO_TEMPORAL ||
                        l.IdTipoLicencia == ID_TIPO_LICENCIA_CUMPLEANIOS
                    ))
                .ToListAsync();

            double totalMinutos = 0;

            foreach (var l in licencias)
            {
                // Permiso de ese día
                var inicioPermiso = fecha.Date + l.HoraInicio;
                var finPermiso = fecha.Date + l.HoraFin;

                // Intersección permiso ⨉ jornada
                var inicioEfectivo = inicioPermiso > inicioJornada ? inicioPermiso : inicioJornada;
                var finEfectivo = finPermiso < finJornada ? finPermiso : finJornada;

                if (finEfectivo <= inicioEfectivo)
                    continue;

                var minutos = (finEfectivo - inicioEfectivo).TotalMinutes;

                // DEBUG (puedes dejar unos días para probar)
                Console.WriteLine(
                    $"Permiso día {fecha:yyyy-MM-dd} ({l.TipoLicencia.ValorCategoria}): " +
                    $"{inicioEfectivo:HH:mm} - {finEfectivo:HH:mm} = {minutos} min");

                totalMinutos += minutos;
            }

            // Limitar por seguridad
            if (totalMinutos < 0) totalMinutos = 0;
            if (totalMinutos > MINUTOS_MINIMOS_JORNADA)
                totalMinutos = MINUTOS_MINIMOS_JORNADA;

            return (int)Math.Round(totalMinutos, MidpointRounding.AwayFromZero);
        }

        private async Task<Licencia?> ObtenerPermisoTemporalFinalDelDiaAsync(
    int idTrabajador,
    DateTime fecha,
    TimeSpan horaSalidaHorario)
        {
            const string TIPO_PERMISO_TEMPORAL = "Permiso temporal";

            return await _db.Licencias
                .Include(l => l.EstadoLicencia)
                .Include(l => l.TipoLicencia)
                .Where(l =>
                    l.IdTrabajador == idTrabajador &&
                    l.FechaInicio.Date == fecha.Date &&
                    l.FechaFin.Date == fecha.Date &&
                    l.EstadoLicencia.Categoria == "EstadoSolicitud" &&
                    l.EstadoLicencia.ValorCategoria == "Aprobado" &&
                    l.TipoLicencia != null &&
                    l.TipoLicencia.ValorCategoria == TIPO_PERMISO_TEMPORAL &&

                    // 🔴 En vez de "HoraFin == horaSalidaHorario":
                    // el permiso tiene que TOCAR el último tramo de la jornada:
                    l.HoraInicio < horaSalidaHorario &&   // empieza antes de la hora de salida
                    l.HoraFin >= horaSalidaHorario        // termina en o después de la hora de salida
                )
                .OrderByDescending(l => l.HoraInicio)  // por si hubiera varios, tomamos el "más pegado" al final
                .FirstOrDefaultAsync();
        }


    }
}
using System.Globalization;
using AutoMapper;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Asistencia;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AsistenciasController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        // 🔥 PARA PRUEBAS: 4 minutos
        // En producción cámbialo a: 480 (8 horas)
        private const int MINUTOS_MINIMOS_JORNADA = 4;

        public AsistenciasController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }


        // ====================================================================
        // =====================  MÉTODO COMPLETO DE ASISTENCIA  ==============
        // ====================================================================

        [HttpPost]
        public async Task<IActionResult> RegistrarAsistencia([FromBody] AsistenciaCrearDTO dto)
        {
            // 1) Buscar trabajador + horarios
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

            // 2) Obtener el horario de hoy
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
                    Mensaje = "El trabajador no tiene un horario definido para hoy."
                });
            }

            // 3) Asistencias registradas hoy
            var asistenciasHoy = await _db.Asistencias
                .Where(a => a.IdTrabajador == dto.IdTrabajador && a.Fecha == dto.Fecha.Date)
                .OrderBy(a => a.Hora)
                .ToListAsync();

            // Convertir a DateTime para cálculos
            var horaMarcada = dto.Fecha.Date + dto.Hora;
            var horaEntradaProgramada = dto.Fecha.Date + horarioHoy.HoraEntrada;

            // ====================================================================
            // ============================  PRIMERA MARCACIÓN  ===================
            // ====================================================================
            if (!asistenciasHoy.Any())
            {
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

                // Registrar ENTRADA
                var asistenciaEntrada = _mapper.Map<Asistencia>(dto);
                asistenciaEntrada.Fecha = dto.Fecha.Date;
                asistenciaEntrada.Hora = dto.Hora;
                asistenciaEntrada.EsEntrada = true;

                _db.Asistencias.Add(asistenciaEntrada);
                await _db.SaveChangesAsync();

                return Ok(new AsistenciaRegistrarResultadoDTO
                {
                    Registrado = true,
                    TipoMarcacion = "ENTRADA",
                    HoraEntrada = horaMarcada,
                    Mensaje = $"Entrada registrada correctamente a las {horaMarcada:HH:mm}."
                });
            }


            // ====================================================================
            // =================  YA HAY ENTRADA, VEAMOS SI ES SALIDA  ============
            // ====================================================================

            var entrada = asistenciasHoy.FirstOrDefault(a => a.EsEntrada);
            var salida = asistenciasHoy.FirstOrDefault(a => !a.EsEntrada);

            // Si ya existe salida → fin
            if (entrada != null && salida != null)
            {
                var dtEntrada = dto.Fecha.Date + entrada.Hora;
                var dtSalida = dto.Fecha.Date + salida.Hora;
                var diff = dtSalida - dtEntrada;

                return Ok(new AsistenciaRegistrarResultadoDTO
                {
                    Registrado = false,
                    TipoMarcacion = "SALIDA",
                    HoraEntrada = dtEntrada,
                    HoraSalida = dtSalida,
                    HorasTrabajadas = $"{(int)diff.TotalHours:D2}:{diff.Minutes:D2}",
                    Mensaje = "Ya registraste ENTRADA y SALIDA hoy."
                });
            }

            // Validamos tiempo mínimo
            var horaEntradaReal = dto.Fecha.Date + entrada!.Hora;
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
                              $"Faltan {faltan.Minutes:D2} minuto(s) para completar la jornada."
                });
            }

            // ====================================================================
            // ==============================  SALIDA  =============================
            // ====================================================================
            var asistenciaSalida = _mapper.Map<Asistencia>(dto);
            asistenciaSalida.Fecha = dto.Fecha.Date;
            asistenciaSalida.Hora = dto.Hora;
            asistenciaSalida.EsEntrada = false;

            _db.Asistencias.Add(asistenciaSalida);
            await _db.SaveChangesAsync();

            var horasTrabajadas = horaMarcada - horaEntradaReal;
            var horasFormateadas = $"{(int)horasTrabajadas.TotalHours:D2}:{horasTrabajadas.Minutes:D2}";

            return Ok(new AsistenciaRegistrarResultadoDTO
            {
                Registrado = true,
                TipoMarcacion = "SALIDA",
                HoraEntrada = horaEntradaReal,
                HoraSalida = horaMarcada,
                HorasTrabajadas = horasFormateadas,
                Mensaje = $"Salida registrada a las {horaMarcada:HH:mm}. " +
                          $"Tiempo trabajado: {horasFormateadas}."
            });
        }
    }
}

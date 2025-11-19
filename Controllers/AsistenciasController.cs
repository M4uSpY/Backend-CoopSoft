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

        // AJUSTA ESTE ID según la fila real en Clasificador (tipo falta)
        private const int ID_TIPO_FALTA_INASISTENCIA = 1;

        public AsistenciasController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerAsistencias()
        {
            var asistencias = await _db.Asistencias
                .Include(a => a.Trabajador)
                    .ThenInclude(t => t.Persona)
                .Include(a => a.Trabajador)
                    .ThenInclude(t => t.Cargo)
                        .ThenInclude(c => c.Oficina)
                .ToListAsync();

            var listaAsistencias = _mapper.Map<List<AsistenciaListaDTO>>(asistencias);
            return Ok(listaAsistencias);
        }

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
                    Mensaje = "Trabajador no encontrado."
                });
            }

            // 2) Día de la semana actual en español (según la fecha enviada)
            var cultura = new CultureInfo("es-ES");
            string diaSemanaActual = dto.Fecha.ToString("dddd", cultura);
            diaSemanaActual = char.ToUpper(diaSemanaActual[0]) + diaSemanaActual.Substring(1);

            var horarioHoy = trabajador.Horarios
                .FirstOrDefault(h => h.DiaSemana == diaSemanaActual);

            if (horarioHoy is null)
            {
                // No tiene horario ese día → igual registramos, pero lo indicamos
                var asistenciaSinHorario = _mapper.Map<Asistencia>(dto);
                // Ignoramos dto.esEntrada y decidimos luego, pero aquí no hay horario

                _db.Asistencias.Add(asistenciaSinHorario);
                await _db.SaveChangesAsync();

                return Ok(new AsistenciaRegistrarResultadoDTO
                {
                    Registrado = true,
                    EsEntrada = true, // por defecto
                    FaltaGenerada = false,
                    Mensaje = "Asistencia registrada, pero el trabajador no tiene horario definido para hoy."
                });
            }

            // 3) Asistencias ya registradas hoy
            var asistenciasHoy = await _db.Asistencias
                .Where(a => a.IdTrabajador == dto.IdTrabajador && a.Fecha == dto.Fecha)
                .OrderBy(a => a.Hora)
                .ToListAsync();

            bool esEntradaMarcacion;

            if (!asistenciasHoy.Any())
            {
                // Primera marcación del día → ENTRADA
                esEntradaMarcacion = true;
            }
            else if (asistenciasHoy.Count == 1 && asistenciasHoy[0].EsEntrada)
            {
                // Ya tiene ENTRADA → esta es SALIDA
                esEntradaMarcacion = false;
            }
            else
            {
                // Ya tiene ENTRADA y SALIDA (o más registros)
                return BadRequest(new AsistenciaRegistrarResultadoDTO
                {
                    Registrado = false,
                    EsEntrada = false,
                    FaltaGenerada = false,
                    Mensaje = "El trabajador ya tiene todas sus marcaciones registradas para hoy."
                });
            }

            TimeSpan horaMarcada = dto.Hora;
            TimeSpan horaEntrada = horarioHoy.HoraEntrada;

            bool faltaGenerada = false;

            // 4) Si es ENTRADA, revisar si pasó 30 minutos después de la hora de entrada
            if (esEntradaMarcacion)
            {
                var limiteFalta = horaEntrada.Add(TimeSpan.FromMinutes(30));

                if (horaMarcada > limiteFalta)
                {
                    // Verificar si ya existe una falta hoy (por seguridad)
                    bool yaTieneFalta = await _db.Faltas
                        .AnyAsync(f => f.IdTrabajador == dto.IdTrabajador && f.Fecha == dto.Fecha);

                    if (!yaTieneFalta)
                    {
                        var falta = new Falta
                        {
                            IdTrabajador = dto.IdTrabajador,
                            IdTipoFalta = ID_TIPO_FALTA_INASISTENCIA, // AJUSTA ESTE VALOR
                            Fecha = dto.Fecha,
                            Descripcion = "Falta generada automáticamente por no registrar asistencia dentro de los 30 minutos posteriores a la hora de entrada.",
                            ArchivoJustificativo = Array.Empty<byte>()
                        };

                        _db.Faltas.Add(falta);
                        faltaGenerada = true;
                    }
                }
            }

            // 5) Registrar la asistencia
            var asistencia = _mapper.Map<Asistencia>(dto);
            asistencia.EsEntrada = esEntradaMarcacion; // ignoramos el dto.esEntrada que mande el cliente

            _db.Asistencias.Add(asistencia);
            await _db.SaveChangesAsync();

            string mensaje;

            if (esEntradaMarcacion)
            {
                mensaje = "Entrada registrada correctamente.";
                if (faltaGenerada)
                    mensaje += " Se generó una falta por ingreso fuera del tiempo permitido.";
            }
            else
            {
                mensaje = "Salida registrado correctamente.";
            }

            var resultado = new AsistenciaRegistrarResultadoDTO
            {
                Registrado = true,
                EsEntrada = esEntradaMarcacion,
                FaltaGenerada = faltaGenerada,
                Mensaje = mensaje
            };

            return Ok(resultado);
        }
    }
}

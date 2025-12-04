using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Trabajadores;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrabajadoresController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public TrabajadoresController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // GET: api/Trabajadores
        [HttpGet]
        public async Task<IActionResult> ObtenerTrabajadores()
        {
            var trabajadores = await _db.Trabajadores
                .Where(t => t.EstadoTrabajador)
                .Include(t => t.Persona)
                .ThenInclude(p => p.Nacionalidad)
                .Include(t => t.Cargo)
                .ThenInclude(c => c.Oficina)
                .Include(t => t.Horarios)
                .Include(t => t.Contratos)
                .ToListAsync();

            var listTrabajadores = _mapper.Map<List<TrabajadoresListarDTO>>(trabajadores);
            return Ok(listTrabajadores);
        }

        // GET: api/Trabajadores/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerTrabajadorId(int id)
        {
            var trabajador = await _db.Trabajadores
                .Include(t => t.Persona)
                .ThenInclude(p => p.Nacionalidad)
                .Include(t => t.Cargo)
                .ThenInclude(c => c.Oficina)
                .Include(t => t.Horarios)
                .Include(t => t.Contratos)
                .FirstOrDefaultAsync(t => t.IdTrabajador == id);

            if (trabajador is null)
                return NotFound("Trabajador no encontrado");

            var trabajadorDTO = _mapper.Map<TrabajadoresListarDTO>(trabajador);
            return Ok(trabajadorDTO);
        }

        // POST: api/Trabajadores
        [HttpPost]
        public async Task<IActionResult> CrearTrabajador(TrabajadorCrearDTO trabajadorCrearDTO)
        {
            if (trabajadorCrearDTO is null)
                return BadRequest("El trabajador no puede ser nulo");

            // Validar duplicados dentro del DTO
            var diasDuplicados = trabajadorCrearDTO.Horarios
                .GroupBy(h => h.DiaSemana)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (diasDuplicados.Any())
                return BadRequest($"No se puede tener horarios duplicados para los días: {string.Join(", ", diasDuplicados)}");

            // Mapear trabajador
            var trabajador = _mapper.Map<Trabajador>(trabajadorCrearDTO);

            // Guardar el trabajador primero para generar su Id
            await _db.Trabajadores.AddAsync(trabajador);
            await _db.SaveChangesAsync();

            // Eliminar horarios previos si existieran (para evitar duplicados)
            var horariosExistentes = _db.Horarios.Where(h => h.IdTrabajador == trabajador.IdTrabajador);
            _db.Horarios.RemoveRange(horariosExistentes);
            await _db.SaveChangesAsync();

            // Agregar los nuevos horarios
            foreach (var horarioDTO in trabajadorCrearDTO.Horarios)
            {
                var horario = _mapper.Map<Horario>(horarioDTO);
                horario.IdTrabajador = trabajador.IdTrabajador; // asignar FK correcta
                await _db.Horarios.AddAsync(horario);
            }

            // Registrar histórico de creación (si conocemos el usuario)
            var idUsuarioActual = ObtenerIdUsuarioActual();
            if (idUsuarioActual is not null)
            {
                var historico = new HistoricoTrabajador
                {
                    IdTrabajador = trabajador.IdTrabajador,
                    UsuarioModificoId = idUsuarioActual.Value,
                    FechaModificacion = DateTime.Now,
                    Accion = "CREAR",
                    Campo = "Todos",
                    ValorAnterior = null,
                    ValorActual = "Trabajador creado con horarios"
                };

                await _db.HistoricosTrabajador.AddAsync(historico);
            }


            await _db.SaveChangesAsync();

            var trabajadorDTO = _mapper.Map<TrabajadoresListarDTO>(trabajador);
            return Ok(trabajadorDTO);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> EliminarTrabajador(int id)
        {
            var trabajador = await _db.Trabajadores
                .Include(t => t.Contratos)
                .Include(t => t.TrabajadorPlanillas)
                .FirstOrDefaultAsync(t => t.IdTrabajador == id);

            if (trabajador is null)
                return NotFound("Trabajador no encontrado");

            var idUsuarioActual = ObtenerIdUsuarioActual();
            if (idUsuarioActual is null)
                return Unauthorized("No se pudo identificar al usuario que quiere modificar.");

            // OPCIONAL: reglas de negocio para proteger datos históricos
            if (trabajador.Contratos != null && trabajador.Contratos.Any())
            {
                return BadRequest("No se puede inactivar el trabajador porque tiene contratos registrados.");
            }

            if (trabajador.TrabajadorPlanillas != null && trabajador.TrabajadorPlanillas.Any())
            {
                return BadRequest("No se puede inactivar el trabajador porque está asociado a planillas.");
            }

            // Soft delete
            var estadoAnterior = trabajador.EstadoTrabajador;
            trabajador.EstadoTrabajador = false;

            var historico = new HistoricoTrabajador
            {
                IdTrabajador = trabajador.IdTrabajador,
                UsuarioModificoId = idUsuarioActual.Value,
                FechaModificacion = DateTime.Now,
                Accion = "INACTIVAR",
                Campo = "EstadoTrabajador",
                ValorAnterior = estadoAnterior.ToString(),
                ValorActual = trabajador.EstadoTrabajador.ToString()
            };

            await _db.HistoricosTrabajador.AddAsync(historico);
            await _db.SaveChangesAsync();

            return Ok("Trabajador inactivado correctamente");
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> ActualizarTrabajador(int id, TrabajadorCrearDTO trabajadorActualizarDTO)
        {
            if (trabajadorActualizarDTO is null)
                return BadRequest("El trabajador no puede ser nulo");

            // Buscar el trabajador existente
            var trabajador = await _db.Trabajadores
                .Include(t => t.Horarios)
                .FirstOrDefaultAsync(t => t.IdTrabajador == id);

            if (trabajador == null)
                return NotFound("Trabajador no encontrado");

            var idUsuarioActual = ObtenerIdUsuarioActual();
            if (idUsuarioActual is null)
                return Unauthorized("No se pudo identificar al usuario que modifica.");

            // Guardar estado ANTES
            var antes = new
            {
                trabajador.IdPersona,
                trabajador.HaberBasico,
                trabajador.FechaIngreso,
                trabajador.IdCargo,
                Horarios = trabajador.Horarios
                    .Select(h => new { h.DiaSemana, h.HoraEntrada, h.HoraSalida })
                    .OrderBy(h => h.DiaSemana)
                    .ToList()
            };

            // Validar duplicados en los horarios del DTO
            var diasDuplicados = trabajadorActualizarDTO.Horarios
                .GroupBy(h => h.DiaSemana)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (diasDuplicados.Any())
                return BadRequest($"No se puede tener horarios duplicados para los días: {string.Join(", ", diasDuplicados)}");

            trabajador.IdPersona = trabajadorActualizarDTO.IdPersona;
            trabajador.HaberBasico = trabajadorActualizarDTO.HaberBasico;
            trabajador.FechaIngreso = trabajadorActualizarDTO.FechaIngreso;
            trabajador.IdCargo = trabajadorActualizarDTO.IdCargo;

            if (trabajador.Horarios.Any())
                _db.Horarios.RemoveRange(trabajador.Horarios);

            // Agregar los nuevos horarios
            var horariosNuevos = new List<Horario>();
            foreach (var horarioDTO in trabajadorActualizarDTO.Horarios)
            {
                var horario = _mapper.Map<Horario>(horarioDTO);
                horario.IdTrabajador = trabajador.IdTrabajador;
                horariosNuevos.Add(horario);
                await _db.Horarios.AddAsync(horario);
            }

            var historicos = new List<HistoricoTrabajador>();

            if (antes.IdPersona != trabajador.IdPersona)
                historicos.Add(new HistoricoTrabajador
                {
                    IdTrabajador = trabajador.IdTrabajador,
                    UsuarioModificoId = idUsuarioActual.Value,
                    FechaModificacion = DateTime.Now,
                    Accion = "ACTUALIZAR",
                    Campo = "IdPersona",
                    ValorAnterior = antes.IdPersona.ToString(),
                    ValorActual = trabajador.IdPersona.ToString()
                });

            if (antes.HaberBasico != trabajador.HaberBasico)
                historicos.Add(new HistoricoTrabajador
                {
                    IdTrabajador = trabajador.IdTrabajador,
                    UsuarioModificoId = idUsuarioActual.Value,
                    FechaModificacion = DateTime.Now,
                    Accion = "ACTUALIZAR",
                    Campo = "HaberBasico",
                    ValorAnterior = antes.HaberBasico.ToString("0.00"),
                    ValorActual = trabajador.HaberBasico.ToString("0.00")
                });

            if (antes.FechaIngreso != trabajador.FechaIngreso)
                historicos.Add(new HistoricoTrabajador
                {
                    IdTrabajador = trabajador.IdTrabajador,
                    UsuarioModificoId = idUsuarioActual.Value,
                    FechaModificacion = DateTime.Now,
                    Accion = "ACTUALIZAR",
                    Campo = "FechaIngreso",
                    ValorAnterior = antes.FechaIngreso.ToShortDateString(),
                    ValorActual = trabajador.FechaIngreso.ToShortDateString()
                });

            if (antes.IdCargo != trabajador.IdCargo)
                historicos.Add(new HistoricoTrabajador
                {
                    IdTrabajador = trabajador.IdTrabajador,
                    UsuarioModificoId = idUsuarioActual.Value,
                    FechaModificacion = DateTime.Now,
                    Accion = "ACTUALIZAR",
                    Campo = "IdCargo",
                    ValorAnterior = antes.IdCargo.ToString(),
                    ValorActual = trabajador.IdCargo.ToString()
                });

            // Registro resumen de cambio de horarios
            var cantAntes = antes.Horarios.Count;
            var cantDespues = horariosNuevos.Count;
            if (cantAntes != cantDespues ||
                !antes.Horarios.SequenceEqual(
                    horariosNuevos
                        .Select(h => new { h.DiaSemana, h.HoraEntrada, h.HoraSalida })
                        .OrderBy(h => h.DiaSemana)))
            {
                historicos.Add(new HistoricoTrabajador
                {
                    IdTrabajador = trabajador.IdTrabajador,
                    UsuarioModificoId = idUsuarioActual.Value,
                    FechaModificacion = DateTime.Now,
                    Accion = "ACTUALIZAR",
                    Campo = "Horarios",
                    ValorAnterior = $"{cantAntes} horarios",
                    ValorActual = $"{cantDespues} horarios"
                });
            }

            if (historicos.Any())
            {
                await _db.HistoricosTrabajador.AddRangeAsync(historicos);
            }



            await _db.SaveChangesAsync();

            var trabajadorDTO = _mapper.Map<TrabajadoresListarDTO>(trabajador);
            return Ok(trabajadorDTO);
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

    }
}

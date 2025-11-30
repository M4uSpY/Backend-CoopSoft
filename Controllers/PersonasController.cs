using AutoMapper;
using System.Security.Claims;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Personas;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,Casual,Consejo")]
    [ApiController]
    public class PersonasController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        public PersonasController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerTodasPeronas()
        {
            var personas = await _db.Personas.Include(p => p.Nacionalidad).ToListAsync();
            var listaPersonas = _mapper.Map<List<PersonasListarDTO>>(personas);
            return Ok(listaPersonas);
        }
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerPersonaId(int id)
        {
            var persona = await _db.Personas
            .Include(p => p.Nacionalidad)
            .Include(p => p.Trabajador!)
                .ThenInclude(t => t.FormacionesAcademicas)
            .Include(p => p.Trabajador!)
                .ThenInclude(t => t.Capacitaciones)
                .FirstOrDefaultAsync(p => p.IdPersona == id);

            if (persona is null)
            {
                return NotFound("Persona no encontrada");
            }
            var personaDTO = _mapper.Map<PersonasListarDTO>(persona);
            return Ok(personaDTO);
        }
        [HttpPost]
        public async Task<IActionResult> CrearPersona(PersonaCrearDTO personaCrearDTO)
        {
            if (personaCrearDTO is null)
            {
                return BadRequest("La persona no puede ser nula");
            }

            // VALIDACION DE TAMANIO DE IMAGEN

            const int maxFotoBytes = 2 * 1024 * 1024; // 2 MB
            if (personaCrearDTO.Foto != null && personaCrearDTO.Foto.Length > maxFotoBytes)
                return BadRequest("La foto no debe superar los 2 MB.");

            var persona = _mapper.Map<Persona>(personaCrearDTO);
            await _db.Personas.AddAsync(persona);
            await _db.SaveChangesAsync();

            if (personaCrearDTO.Huella != null && personaCrearDTO.Huella.Length > 0)
            {
                var huella = new HuellaDactilar
                {
                    IdPersona = persona.IdPersona,
                    Huella = personaCrearDTO.Huella
                };
                await _db.HuellasDactilares.AddAsync(huella);
                await _db.SaveChangesAsync();
            }

            var idUsuarioActual = ObtenerIdUsuarioActual();
            if (idUsuarioActual is not null)
            {
                var historico = new HistoricoPersona
                {
                    IdPersona = persona.IdPersona,
                    UsuarioModificoId = idUsuarioActual.Value,
                    FechaModificacion = DateTime.Now,
                    Accion = "CREAR",
                    Campo = "Todos",
                    ValorAnterior = null,
                    ValorActual = "Persona creada"
                };

                await _db.HistoricosPersona.AddAsync(historico);
                await _db.SaveChangesAsync();
            }
            var personaCreada = _mapper.Map<PersonasListarDTO>(persona);
            return CreatedAtAction(nameof(ObtenerPersonaId), new { id = persona.IdPersona }, personaCreada);
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> ActualizarPersona(int id, PersonaCrearDTO personaActualizarDTO)
        {
            var persona = await _db.Personas.FirstOrDefaultAsync(p => p.IdPersona == id);
            if (persona is null)
                return NotFound("Persona no encontrada");

            var idUsuarioActual = ObtenerIdUsuarioActual();
            if (idUsuarioActual is null)
                return Unauthorized("No se pudo identificar al usuario que modifica.");

            const int maxFotoBytes = 2 * 1024 * 1024; // 2 MB
            if (personaActualizarDTO.Foto != null && personaActualizarDTO.Foto.Length > maxFotoBytes)
                return BadRequest("La foto no debe superar los 2 MB.");

            var antes = new
            {
                persona.PrimerNombre,
                persona.SegundoNombre,
                persona.ApellidoPaterno,
                persona.ApellidoMaterno,
                persona.CarnetIdentidad,
                persona.Direccion,
                persona.Telefono,
                persona.Email
            };

            // Actualizar los datos de la persona
            _mapper.Map(personaActualizarDTO, persona);


            // Actualizar la huella si viene en el DTO
            if (personaActualizarDTO.Huella != null && personaActualizarDTO.Huella.Length > 0)
            {
                // Buscar si ya existe una huella para esa persona
                var huella = await _db.HuellasDactilares.FirstOrDefaultAsync(h => h.IdPersona == id);

                if (huella != null)
                {
                    // Actualizar huella existente
                    huella.Huella = personaActualizarDTO.Huella;
                    _db.HuellasDactilares.Update(huella);
                }
                else
                {
                    // Crear nueva huella
                    huella = new HuellaDactilar
                    {
                        IdPersona = id,
                        Huella = personaActualizarDTO.Huella
                    };
                    await _db.HuellasDactilares.AddAsync(huella);
                }
            }

            var historicos = new List<HistoricoPersona>();

            if (antes.PrimerNombre != persona.PrimerNombre)
                historicos.Add(new HistoricoPersona
                {
                    IdPersona = persona.IdPersona,
                    UsuarioModificoId = idUsuarioActual.Value,
                    FechaModificacion = DateTime.Now,
                    Accion = "ACTUALIZAR",
                    Campo = "PrimerNombre",
                    ValorAnterior = antes.PrimerNombre,
                    ValorActual = persona.PrimerNombre
                });

            if (antes.SegundoNombre != persona.SegundoNombre)
                historicos.Add(new HistoricoPersona
                {
                    IdPersona = persona.IdPersona,
                    UsuarioModificoId = idUsuarioActual.Value,
                    FechaModificacion = DateTime.Now,
                    Accion = "ACTUALIZAR",
                    Campo = "SegundoNombre",
                    ValorAnterior = antes.SegundoNombre,
                    ValorActual = persona.SegundoNombre
                });

            if (antes.ApellidoPaterno != persona.ApellidoPaterno)
                historicos.Add(new HistoricoPersona
                {
                    IdPersona = persona.IdPersona,
                    UsuarioModificoId = idUsuarioActual.Value,
                    FechaModificacion = DateTime.Now,
                    Accion = "ACTUALIZAR",
                    Campo = "ApellidoPaterno",
                    ValorAnterior = antes.ApellidoPaterno,
                    ValorActual = persona.ApellidoPaterno
                });

            if (antes.ApellidoMaterno != persona.ApellidoMaterno)
                historicos.Add(new HistoricoPersona
                {
                    IdPersona = persona.IdPersona,
                    UsuarioModificoId = idUsuarioActual.Value,
                    FechaModificacion = DateTime.Now,
                    Accion = "ACTUALIZAR",
                    Campo = "ApellidoMaterno",
                    ValorAnterior = antes.ApellidoMaterno,
                    ValorActual = persona.ApellidoMaterno
                });

            if (antes.CarnetIdentidad != persona.CarnetIdentidad)
                historicos.Add(new HistoricoPersona
                {
                    IdPersona = persona.IdPersona,
                    UsuarioModificoId = idUsuarioActual.Value,
                    FechaModificacion = DateTime.Now,
                    Accion = "ACTUALIZAR",
                    Campo = "CarnetIdentidad",
                    ValorAnterior = antes.CarnetIdentidad,
                    ValorActual = persona.CarnetIdentidad
                });

            if (antes.Direccion != persona.Direccion)
                historicos.Add(new HistoricoPersona
                {
                    IdPersona = persona.IdPersona,
                    UsuarioModificoId = idUsuarioActual.Value,
                    FechaModificacion = DateTime.Now,
                    Accion = "ACTUALIZAR",
                    Campo = "Direccion",
                    ValorAnterior = antes.Direccion,
                    ValorActual = persona.Direccion
                });

            if (antes.Telefono != persona.Telefono)
                historicos.Add(new HistoricoPersona
                {
                    IdPersona = persona.IdPersona,
                    UsuarioModificoId = idUsuarioActual.Value,
                    FechaModificacion = DateTime.Now,
                    Accion = "ACTUALIZAR",
                    Campo = "Telefono",
                    ValorAnterior = antes.Telefono,
                    ValorActual = persona.Telefono
                });

            if (antes.Email != persona.Email)
                historicos.Add(new HistoricoPersona
                {
                    IdPersona = persona.IdPersona,
                    UsuarioModificoId = idUsuarioActual.Value,
                    FechaModificacion = DateTime.Now,
                    Accion = "ACTUALIZAR",
                    Campo = "Email",
                    ValorAnterior = antes.Email,
                    ValorActual = persona.Email
                });

            if (historicos.Any())
            {
                await _db.HistoricosPersona.AddRangeAsync(historicos);
            }
            await _db.SaveChangesAsync();
            return NoContent();
        }



        [HttpDelete("{id:int}")]
        public async Task<IActionResult> EliminarPersona(int id)
        {
            var persona = await _db.Personas.FirstOrDefaultAsync(p => p.IdPersona == id);
            if (persona is null)
            {
                return NotFound("Persona no encontrada");
            }

            var idUsuarioActual = ObtenerIdUsuarioActual();
            if (idUsuarioActual is null)
            {
                return Unauthorized("No se pudo identificar al usuario que quiere modificar.");
            }

            // Registrar histórico ANTES de borrar
            var historico = new HistoricoPersona
            {
                IdPersona = persona.IdPersona,
                UsuarioModificoId = idUsuarioActual.Value,
                FechaModificacion = DateTime.Now,
                Accion = "INACTIVAR",
                Campo = "Persona",
                ValorAnterior = "Registrada",
                ValorActual = "Eliminada"
            };

            await _db.HistoricosPersona.AddAsync(historico);

            _db.Personas.Remove(persona);
            await _db.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet("{id:int}/huella")]
        public async Task<IActionResult> ObtenerHuellaPersona(int id)
        {
            var huella = await _db.HuellasDactilares
                                  .Where(h => h.IdPersona == id)
                                  .Select(h => h.Huella)
                                  .FirstOrDefaultAsync();

            if (huella == null)
                return NotFound("Huella no registrada");

            return Ok(huella);
        }

        private int? ObtenerIdUsuarioActual()
        {
            var claimSub = User.FindFirst(JwtRegisteredClaimNames.Sub);
            var claimNameId = User.FindFirst(ClaimTypes.NameIdentifier); // verificar using
            var claim = claimSub ?? claimNameId;

            if (claim is null)
                return null;

            return int.TryParse(claim.Value, out var idUsuario)
                ? idUsuario
                : (int?)null;
        }


    }
}

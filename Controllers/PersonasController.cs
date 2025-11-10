using AutoMapper;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Personas;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,Casual")]
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
            var persona = await _db.Personas.Include(p => p.Nacionalidad).FirstOrDefaultAsync(p => p.IdPersona == id);
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
            var personaCreada = _mapper.Map<PersonasListarDTO>(persona);
            return CreatedAtAction(nameof(ObtenerPersonaId), new { id = persona.IdPersona }, personaCreada);
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> ActualizarPersona(int id, PersonaCrearDTO personaActualizarDTO)
        {
            var persona = await _db.Personas.FirstOrDefaultAsync(p => p.IdPersona == id);
            if (persona is null)
                return NotFound("Persona no encontrada");

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


    }
}

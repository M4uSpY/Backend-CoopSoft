using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Huella;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class HuellasController : ControllerBase
    {
        private readonly AppDbContext _db;
        public HuellasController(AppDbContext db) => _db = db;

        // Guardar huella
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] HuellaDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.TemplateXml))
                return BadRequest("No puede ser nulo la huella");

            if (dto.IndiceDedo is < 1 or > 2)
                return BadRequest("El índice de dedo debe ser 1 o 2.");

            // Verificar que la persona exista
            var personaExiste = await _db.Personas
                .AnyAsync(p => p.IdPersona == dto.IdPersona);

            if (!personaExiste)
                return NotFound("Persona no encontrada");

            // Traer huellas actuales de esa persona
            var huellasPersona = await _db.HuellasDactilares
                .Where(h => h.IdPersona == dto.IdPersona)
                .ToListAsync();

            // Si ya tiene 2 huellas y no estamos sobreescribiendo una de ellas → error
            if (huellasPersona.Count >= 2 &&
                !huellasPersona.Any(h => h.IndiceDedo == dto.IndiceDedo))
            {
                return BadRequest("La persona ya tiene registradas las 2 huellas permitidas.");
            }

            // Buscar si ya existe huella para este índice (1 o 2)
            var huellaExistente = huellasPersona
                .FirstOrDefault(h => h.IndiceDedo == dto.IndiceDedo);

            if (huellaExistente != null)
            {
                // Actualizar huella existente (re-enrolar)
                huellaExistente.Huella = dto.TemplateXml;
                _db.HuellasDactilares.Update(huellaExistente);
                await _db.SaveChangesAsync();

                return Ok($"Huella {dto.IndiceDedo} actualizada correctamente.");
            }
            else
            {
                // Crear nueva huella
                var huella = new HuellaDactilar
                {
                    IdPersona = dto.IdPersona,
                    IndiceDedo = dto.IndiceDedo,
                    Huella = dto.TemplateXml
                };

                _db.HuellasDactilares.Add(huella);
                await _db.SaveChangesAsync();

                return Ok($"Huella {dto.IndiceDedo} registrada correctamente.");
            }
        }

        // Listar todas las huellas
        [HttpGet("listar")]
        public async Task<IActionResult> Listar()
        {
            var lista = await _db.HuellasDactilares
                        .Where(h => h.Persona.Trabajador != null)
                        .Select(h => new HuellaRespuesta
                        {
                            IdPersona = h.IdPersona,
                            IdTrabajador = h.Persona.Trabajador!.IdTrabajador,
                            PrimerNombre = h.Persona.PrimerNombre,
                            SegundoNombre = h.Persona.SegundoNombre ?? string.Empty,
                            ApellidoPaterno = h.Persona.ApellidoPaterno,
                            ApellidoMaterno = h.Persona.ApellidoMaterno,
                            CI = h.Persona.CarnetIdentidad,
                            Cargo = h.Persona.Usuario.Rol.NombreRol,
                            Foto = h.Persona.Foto,
                            TemplateXml = h.Huella,
                            IndiceDedo = h.IndiceDedo
                        }).ToListAsync();


            return Ok(lista);
        }

        // Obtener huellas de una persona (las 1 o 2)
        [HttpGet("obtener/{idPersona:int}")]
        public async Task<IActionResult> Obtener(int idPersona)
        {
            var huellas = await _db.HuellasDactilares
                .Where(h => h.IdPersona == idPersona)
                .Select(h => new HuellaPersonaDTO
                {
                    IdHuella = h.IdHuella,
                    IdPersona = h.IdPersona,
                    IndiceDedo = h.IndiceDedo,
                    TemplateXml = h.Huella
                })
                .ToListAsync();

            if (!huellas.Any())
                return NotFound("Huella no encontrada");

            return Ok(huellas);
        }


    }
}

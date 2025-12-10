using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Huella;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class HuellasController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly string _marcadorApiKey;


        public HuellasController(AppDbContext db, IConfiguration configuration)
        {
            _db = db;
            _marcadorApiKey = configuration["Marcador:ApiKey"] ?? string.Empty;
        }

        private bool EsMarcadorValido()
        {
            if (!Request.Headers.TryGetValue("X-Marcador-Key", out var headerValue))
                return false;

            return string.Equals(headerValue.ToString(), _marcadorApiKey, StringComparison.Ordinal);
        }

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
        [AllowAnonymous]
        [HttpGet("listar")]
        public async Task<IActionResult> Listar()
        {
            if (!EsMarcadorValido())
                return Unauthorized("Marcador no autorizado.");

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
                            Cargo = h.Persona.Trabajador.Cargo.NombreCargo,
                            Foto = h.Persona.Foto,
                            TemplateXml = h.Huella,
                            IndiceDedo = h.IndiceDedo
                        }).ToListAsync();


            return Ok(lista);
        }


        [AllowAnonymous]
        [HttpGet("obtener/{idPersona:int}")]
        public async Task<IActionResult> Obtener(int idPersona)
        {
            if (!EsMarcadorValido())
                return Unauthorized("Marcador no autorizado.");

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

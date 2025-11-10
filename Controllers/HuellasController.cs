using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Huella;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
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

            var huella = new HuellaDactilar
            {
                IdPersona = dto.IdPersona,
                Huella = dto.TemplateXml
            };

            _db.HuellasDactilares.Add(huella);
            await _db.SaveChangesAsync();

            return Ok("Huella registrada correctamente");
        }

        // Listar todas las huellas
        [HttpGet("listar")]
        public async Task<IActionResult> Listar()
        {
            var lista = await _db.HuellasDactilares
                .Select(h => new HuellaRespuesta
                {
                    IdPersona = h.IdPersona,
                    PrimerNombre = h.Persona.PrimerNombre,
                    SegundoNombre = h.Persona.SegundoNombre ?? string.Empty,
                    ApellidoPaterno = h.Persona.ApellidoPaterno,
                    ApellidoMaterno = h.Persona.ApellidoMaterno,
                    TemplateXml = h.Huella
                }).ToListAsync();

            return Ok(lista);
        }

        // Obtener huella de una persona
        [HttpGet("obtener/{idPersona:int}")]
        public async Task<IActionResult> Obtener(int idPersona)
        {
            var huella = await _db.HuellasDactilares.FirstOrDefaultAsync(h => h.IdPersona == idPersona);
            if (huella == null) return NotFound("Huella no encontrada");

            return Ok(new HuellaRespuesta
            {
                IdPersona = huella.IdPersona,
                TemplateXml = huella.Huella
            });
        }
    }
}

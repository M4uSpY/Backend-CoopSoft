using AutoMapper;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.InformacionPersonal.Contratacion;
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
    public class ContratosController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public ContratosController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet("trabajador/{idTrabajador:int}")]
        public async Task<IActionResult> ObtenerContratosPorTrabajador(int idTrabajador)
        {
            var contratos = await _db.Contratos
                .Where(c => c.IdTrabajador == idTrabajador)
                .ToListAsync();

            var dto = _mapper.Map<List<ContratoDTO>>(contratos);
            return Ok(dto);
        }
        [HttpGet("trabajador/ultimo/{idTrabajador:int}")]
        public async Task<ActionResult<ContratoDTO>> ObtenerUltimoContrato(int idTrabajador)
        {
            var contrato = await _db.Contratos
                .Include(c => c.TipoContrato)
                .Include(c => c.PeriodoPago)
                .Where(c => c.IdTrabajador == idTrabajador)
                .OrderByDescending(c => c.FechaInicio)
                .FirstOrDefaultAsync();

            if (contrato == null)
                return NotFound("El trabajador no tiene contratos.");

            return Ok(_mapper.Map<ContratoDTO>(contrato));
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerContratoPorId(int id)
        {
            var contrato = await _db.Contratos
                .FirstOrDefaultAsync(c => c.IdContrato == id);

            if (contrato is null)
                return NotFound("Formación académica no encontrada");

            var dto = _mapper.Map<ContratoActualizarDTO>(contrato);
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CrearContrato([FromBody] ContratoCrearDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.ArchivoPdf == null || dto.ArchivoPdf.Length == 0)
                return BadRequest("El archivo PDF es obligatorio.");

            // Validar trabajador
            var existeTrabajador = await _db.Trabajadores
                .AnyAsync(t => t.IdTrabajador == dto.IdTrabajador);

            if (!existeTrabajador)
                return BadRequest("El trabajador no existe.");

            var contrato = _mapper.Map<Contrato>(dto);

            await _db.Contratos.AddAsync(contrato);
            await _db.SaveChangesAsync();

            var dtoResp = _mapper.Map<ContratoDTO>(contrato);

            return CreatedAtAction(nameof(ObtenerContratoPorId),
                new { id = contrato.IdContrato },
                dtoResp);
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> ActualizarContrato(int id, [FromBody] ContratoActualizarDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var contrato = await _db.Contratos
                .FirstOrDefaultAsync(c => c.IdContrato == id);

            if (contrato is null)
                return NotFound("Contrato no encontrado");

            _mapper.Map(dto, contrato);

            // Si viene PDF → reemplazarlo
            if (dto.ArchivoPdf != null && dto.ArchivoPdf.Length > 0)
            {
                contrato.ArchivoPdf = dto.ArchivoPdf;
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id:int}/pdf")]
        public async Task<IActionResult> DescargarPdf(int id)
        {
            var contrato = await _db.Contratos.FindAsync(id);

            if (contrato == null)
                return NotFound("Contrato no encontrado.");

            if (contrato.ArchivoPdf == null || contrato.ArchivoPdf.Length == 0)
                return NotFound("No hay PDF registrado.");

            return File(contrato.ArchivoPdf, "application/pdf", $"Contrato_{id}.pdf");
        }


    }
}

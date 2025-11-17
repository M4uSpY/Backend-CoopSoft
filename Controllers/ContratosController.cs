using AutoMapper;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.InformacionPersonal.Contratacion;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
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
        [HttpGet("trabajador/ultimoContrato/{idTrabajador:int}")]
        public async Task<ActionResult<ContratoDTO>> ObtenerUltimoContratoPorTrabajador(int idTrabajador)
        {
            var contrato = await _db.Contratos
                .Where(c => c.IdTrabajador == idTrabajador)
                .OrderByDescending(c => c.FechaInicio) // o FechaFin, según tu lógica
                .FirstOrDefaultAsync();

            if (contrato is null)
                return NotFound("El trabajador no tiene contratos");

            var contratoDTO = _mapper.Map<ContratoDTO>(contrato);
            return Ok(contratoDTO);
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

        // [HttpPost]
        // public async Task<IActionResult> CrearFormacion([FromBody] FormacionAcademicaCrearDTO dto)
        // {
        //     if (!ModelState.IsValid)
        //         return BadRequest(ModelState);

        //     // validar que exista el trabajador
        //     var existeTrabajador = await _db.Trabajadores
        //         .AnyAsync(t => t.IdTrabajador == dto.IdTrabajador);

        //     if (!existeTrabajador)
        //         return BadRequest("El trabajador no existe");

        //     var formacion = _mapper.Map<FormacionAcademica>(dto);

        //     // por seguridad, aseguramos que EF la trate como nueva (IDENTITY)
        //     formacion.IdFormacion = 0;

        //     await _db.FormacionesAcademicas.AddAsync(formacion);
        //     await _db.SaveChangesAsync();

        //     var resumen = _mapper.Map<FormacionAcademicaResumenDTO>(formacion);

        //     return CreatedAtAction(nameof(ObtenerFormacionPorId),
        //         new { id = formacion.IdFormacion },
        //         resumen);
        // }

        // PUT api/formacionesacademicas/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> ActualizarContrato(int id, [FromBody] ContratoActualizarDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var contrato = await _db.Contratos
                .FirstOrDefaultAsync(c => c.IdContrato == id);

            if (contrato is null)
                return NotFound("Formación académica no encontrada");

            if (dto.IdContrato != 0 && dto.IdContrato != id)
                return BadRequest("El Id de la formación no coincide con la ruta");

            _mapper.Map(dto, contrato);

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // // DELETE api/formacionesacademicas/5
        // [HttpDelete("{id:int}")]
        // public async Task<IActionResult> EliminarFormacion(int id)
        // {
        //     var formacion = await _db.FormacionesAcademicas
        //         .FirstOrDefaultAsync(f => f.IdFormacion == id);

        //     if (formacion is null)
        //         return NotFound("Formación académica no encontrada");

        //     _db.FormacionesAcademicas.Remove(formacion);
        //     await _db.SaveChangesAsync();
        //     return NoContent();
        // }
    }
}

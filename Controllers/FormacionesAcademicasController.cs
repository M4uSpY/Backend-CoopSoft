using AutoMapper;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.InformacionPersonal.FormacionAcademica;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,Casual")]
    [ApiController]
    public class FormacionesAcademicasController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public FormacionesAcademicasController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // GET api/formacionesacademicas/trabajador/3
        [HttpGet("trabajador/{idTrabajador:int}")]
        public async Task<IActionResult> ObtenerFormacionesPorTrabajador(int idTrabajador)
        {
            var formaciones = await _db.FormacionesAcademicas
                .Where(f => f.IdTrabajador == idTrabajador)
                .ToListAsync();

            var dto = _mapper.Map<List<FormacionAcademicaResumenDTO>>(formaciones);
            return Ok(dto);
        }

        // GET api/formacionesacademicas/5
        // Devuelve DTO para EDITAR
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObtenerFormacionPorId(int id)
        {
            var formacion = await _db.FormacionesAcademicas
                .FirstOrDefaultAsync(f => f.IdFormacion == id);

            if (formacion is null)
                return NotFound("Formación académica no encontrada");

            var dto = _mapper.Map<FormacionAcademicaEditarDTO>(formacion);
            return Ok(dto);
        }

        // POST api/formacionesacademicas
        [HttpPost]
        public async Task<IActionResult> CrearFormacion([FromBody] FormacionAcademicaCrearDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            const int maxCertBytes = 2 * 1024 * 1024; // 2 MB
            if (dto.ArchivoPdf != null && dto.ArchivoPdf.Length > maxCertBytes)
                return BadRequest("El archivo del certificado no debe superar los 2 MB.");

            // validar que exista el trabajador
            var existeTrabajador = await _db.Trabajadores
                .AnyAsync(t => t.IdTrabajador == dto.IdTrabajador);

            if (!existeTrabajador)
                return BadRequest("El trabajador no existe");

            var formacion = _mapper.Map<FormacionAcademica>(dto);

            // por seguridad, aseguramos que EF la trate como nueva (IDENTITY)
            formacion.IdFormacion = 0;

            await _db.FormacionesAcademicas.AddAsync(formacion);
            await _db.SaveChangesAsync();

            var resumen = _mapper.Map<FormacionAcademicaResumenDTO>(formacion);

            return CreatedAtAction(nameof(ObtenerFormacionPorId),
                new { id = formacion.IdFormacion },
                resumen);
        }

        // PUT api/formacionesacademicas/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> ActualizarFormacion(int id, [FromBody] FormacionAcademicaEditarDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            const int maxCertBytes = 2 * 1024 * 1024; // 2 MB
            if (dto.ArchivoPdf != null && dto.ArchivoPdf.Length > maxCertBytes)
                return BadRequest("El archivo del certificado no debe superar los 2 MB.");

            var formacion = await _db.FormacionesAcademicas
                .FirstOrDefaultAsync(f => f.IdFormacion == id);

            if (formacion is null)
                return NotFound("Formación académica no encontrada");

            // (opcional) validar que lo que venga en el body coincide con la ruta
            if (dto.IdFormacion != 0 && dto.IdFormacion != id)
                return BadRequest("El Id de la formación no coincide con la ruta");

            // actualiza campos (sin tocar el Id)
            _mapper.Map(dto, formacion);

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/formacionesacademicas/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> EliminarFormacion(int id)
        {
            var formacion = await _db.FormacionesAcademicas
                .FirstOrDefaultAsync(f => f.IdFormacion == id);

            if (formacion is null)
                return NotFound("Formación académica no encontrada");

            _db.FormacionesAcademicas.Remove(formacion);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}

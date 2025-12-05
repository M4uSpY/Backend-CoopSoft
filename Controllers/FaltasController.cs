using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Faltas;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,Consejo")]
    [ApiController]
    public class FaltasController : ControllerBase
    {
        private readonly AppDbContext _db;


        private readonly IMapper _mapper;
        public FaltasController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerFaltas()
        {
            var faltas = await _db.Faltas
                .Where(f => f.EstadoFalta)
                .Include(f => f.Trabajador).ThenInclude(t => t.Persona)
                .Include(f => f.TipoFalta).ToListAsync();
            var faltasDTO = _mapper.Map<List<ListarFaltasDTO>>(faltas);
            return Ok(faltasDTO);
        }

        [HttpPost("{id}/justificativo")]
        public async Task<IActionResult> SubirJustificativo(int id, [FromForm] ArchivoJustificativoDTO dto)
        {
            var falta = await _db.Faltas.FindAsync(id);
            if (falta is null)
            {
                return NotFound("La falta no fue encontrada");
            }

            var idUsuarioActual = ObtenerIdUsuarioActual();
            if (idUsuarioActual is null)
            {
                return Unauthorized("No se pudo identificar al usuario que modifica.");
            }

            var teniaArchivoAntes = falta.ArchivoJustificativo != null && falta.ArchivoJustificativo.Length > 0;


            using var ms = new MemoryStream();
            await dto.Archivo.CopyToAsync(ms);
            falta.ArchivoJustificativo = ms.ToArray();

            var historico = new HistoricoFalta
            {
                IdFalta = falta.IdFalta,
                UsuarioModificoId = idUsuarioActual.Value,
                FechaModificacion = DateTime.Now,
                Accion = teniaArchivoAntes ? "ACTUALIZAR" : "CREAR",
                Campo = "ArchivoJustificativo",
                ValorAnterior = teniaArchivoAntes ? "ConArchivo" : "SinArchivo",
                ValorActual = "ConArchivo"
            };

            await _db.HistoricosFalta.AddAsync(historico);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id}/justificativo")]
        public async Task<IActionResult> DescargarJustificativo(int id)
        {
            var falta = await _db.Faltas.FindAsync(id);
            if (falta is null)
            {
                return NotFound("La falta no fue encontrada");
            }

            if (falta.ArchivoJustificativo == null || falta.ArchivoJustificativo.Length == 0)
            {
                return NotFound("La falta no tienen archivo justificativo");
            }
            var fileName = $"Justificativo_Falta_{id}.pdf";
            return File(falta.ArchivoJustificativo, "application/octet-stream", fileName);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> EliminarFalta(int id)
        {
            var falta = await _db.Faltas.FindAsync(id);
            if (falta is null)
                return NotFound("Falta no encontrada");

            var idUsuarioActual = ObtenerIdUsuarioActual();
            if (idUsuarioActual is null)
                return Unauthorized("No se pudo identificar al usuario que modifica.");

            // Estado anterior (por si quieres auditarlo)
            var estadoAnterior = falta.EstadoFalta;

            // 👉 Solo se inactiva
            falta.EstadoFalta = false;

            // Registrar histórico de "inactivación"
            var historico = new HistoricoFalta
            {
                IdFalta = falta.IdFalta,
                UsuarioModificoId = idUsuarioActual.Value,
                FechaModificacion = DateTime.Now,
                Accion = "INACTIVAR",
                Campo = "EstadoFalta",
                ValorAnterior = estadoAnterior.ToString(),
                ValorActual = falta.EstadoFalta.ToString()
            };

            await _db.HistoricosFalta.AddAsync(historico);

            await _db.SaveChangesAsync();

            return NoContent();
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

using AutoMapper;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Asistencia;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AsistenciasController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public AsistenciasController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerAsistencias()
        {
            var asistencias = await _db.Asistencias
                .Include(a => a.Trabajador)
                    .ThenInclude(t => t.Persona)
                .Include(a => a.Trabajador)
                    .ThenInclude(t => t.Cargo)
                        .ThenInclude(c => c.Oficina)
                .ToListAsync();

            var listaAsistencias = _mapper.Map<List<AsistenciaListaDTO>>(asistencias);
            return Ok(listaAsistencias);
        }

    }
}

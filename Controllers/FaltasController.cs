using AutoMapper;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Faltas;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
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
            var faltas = await _db.Faltas.Include(f => f.Trabajador).ThenInclude(t => t.Persona).ToListAsync();
            var faltasDTO = _mapper.Map<List<ListarFaltasDTO>>(faltas);
            return Ok(faltasDTO);
        }
    }
}

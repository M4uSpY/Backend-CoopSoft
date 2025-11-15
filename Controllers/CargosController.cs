using AutoMapper;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Extras;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CargosController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public CargosController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerCargos()
        {
            var cargos = await _db.Cargos.ToListAsync();
            var cargosDTO = _mapper.Map<List<CargoDTO>>(cargos);
            return Ok(cargosDTO);
        }
    }
}

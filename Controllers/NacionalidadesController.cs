using AutoMapper;
using BackendCoopSoft.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NacionalidadesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public NacionalidadesController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerNacionalidades()
        {
            var nacionalidades = await _db.Clasificadores.Where(c => c.Categoria == "nacionalidad").Select(c => new
            {
                c.IdClasificador,
                c.ValorCategoria,
                c.Descripcion
            }).ToListAsync();
            return Ok(nacionalidades);
        }
    }
}

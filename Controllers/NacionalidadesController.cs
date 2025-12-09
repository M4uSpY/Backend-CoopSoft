using AutoMapper;
using BackendCoopSoft.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class NacionalidadesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public NacionalidadesController(AppDbContext db)
        {
            _db = db;
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

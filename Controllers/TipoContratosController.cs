using BackendCoopSoft.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TipoContratosController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TipoContratosController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTiposContrato()
        {
            var tiposContrato = await _db.Clasificadores.Where(c => c.Categoria == "TipoContrato").Select(c => new
            {
                c.IdClasificador,
                c.ValorCategoria,
                c.Descripcion
            }).ToListAsync();
            return Ok(tiposContrato);
        }
    }
}

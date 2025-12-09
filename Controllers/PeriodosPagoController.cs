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
    public class PeriodosPagoController : ControllerBase
    {
        private readonly AppDbContext _db;

        public PeriodosPagoController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPeriodosPago()
        {
            var periodosPago = await _db.Clasificadores.Where(c => c.Categoria == "TipoPeriodoPago").Select(c => new
            {
                c.IdClasificador,
                c.ValorCategoria,
                c.Descripcion
            }).ToListAsync();
            return Ok(periodosPago);
        }
    }
}

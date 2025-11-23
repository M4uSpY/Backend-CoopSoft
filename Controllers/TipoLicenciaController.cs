using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Licencias;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TipoLicenciaController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TipoLicenciaController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var tipos = await _db.Clasificadores
                .Where(c => c.Categoria == "TipoLicencia")
                .OrderBy(c => c.ValorCategoria)
                .Select(c => new TipoLicenciaDTO
                {
                    IdClasificador = c.IdClasificador,
                    ValorCategoria = c.ValorCategoria
                })
                .ToListAsync();

            return Ok(tipos);
        }
    }
}

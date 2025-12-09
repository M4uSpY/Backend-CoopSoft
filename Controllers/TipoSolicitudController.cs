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
    public class TipoSolicitudController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TipoSolicitudController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTiposSolicitud()
        {
            var nacionalidades = await _db.Clasificadores.Where(c => c.Categoria == "TipoSolicitud").Select(c => new
            {
                c.IdClasificador,
                c.ValorCategoria,
                c.Descripcion
            }).ToListAsync();
            return Ok(nacionalidades);
        }
    }
}

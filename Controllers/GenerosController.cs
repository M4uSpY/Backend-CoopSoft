using BackendCoopSoft.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class GenerosController : ControllerBase
    {
        private readonly AppDbContext _db;
        public GenerosController(AppDbContext db)
        {
            _db = db;
        }
        [HttpGet]
        public IActionResult ObtenerGeneros()
        {
            var generos = new List<bool>{ true, false };
            return Ok(generos);
        }
    }
}

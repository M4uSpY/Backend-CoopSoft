using AutoMapper;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Extras;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsAccesoController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public LogsAccesoController(AppDbContext db, IMapper map)
        {
            _db = db;
            _mapper = map;
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerLogsAcceso()
        {
            var logs = await _db.LogAccesos
                .Include(l => l.Usuario)
                    .ThenInclude(u => u.Persona)
                .ToListAsync();
            
            var listaLogs = _mapper.Map<List<LogsAccesoDTO>>(logs);

            return Ok(listaLogs);
        }

    }
}

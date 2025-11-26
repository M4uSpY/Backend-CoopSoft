using AutoMapper;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Historicos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoricosController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        public HistoricosController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet("historicoFaltas")]
        public async Task<IActionResult> ObtenerHistoricoFaltas()
        {
            var historicoFaltas = await _db.HistoricosFalta.OrderByDescending(h => h.FechaModificacion).Include(hf => hf.UsuarioModifico).ToListAsync();

            var dtoF = _mapper.Map<List<HistoricoFaltaListarDTO>>(historicoFaltas);
            return Ok(dtoF);
        }

        [HttpGet("historicoUsuarios")]
        public async Task<IActionResult> ObtenerHistoricoUsuarios()
        {
            var historicoUsuarios = await _db.HistoricosUsuario.OrderByDescending(h => h.FechaModificacion).Include(hf => hf.UsuarioModifico).ToListAsync();

            var dtoU = _mapper.Map<List<HistoricoUsuarioListarDTO>>(historicoUsuarios);
            return Ok(dtoU);
        }

        [HttpGet("historicoPersonas")]
        public async Task<IActionResult> ObtenerHistoricoPersonas()
        {
            var historicoPersonas = await _db.HistoricosPersona.OrderByDescending(h => h.FechaModificacion).Include(hf => hf.UsuarioModifico).ToListAsync();

            var dtoP = _mapper.Map<List<HistoricoPersonaListarDTO>>(historicoPersonas);
            return Ok(dtoP);
        }

        [HttpGet("historicoTrabajadores")]
        public async Task<IActionResult> ObtenerHistoricoTrabajadores()
        {
            var historicoTrabajadores = await _db.HistoricosTrabajador.OrderByDescending(h => h.FechaModificacion).Include(hf => hf.UsuarioModifico).ToListAsync();

            var dtoT = _mapper.Map<List<HistoricoTrabajadorListarDTO>>(historicoTrabajadores);
            return Ok(dtoT);
        }
    }
}

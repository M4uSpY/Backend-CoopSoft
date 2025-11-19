using AutoMapper;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs;
using BackendCoopSoft.DTOs.Extras;
using BackendCoopSoft.DTOs.Usuarios;
using BackendCoopSoft.Models;
using BackendCoopSoft.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // recursos a necesitar
        private readonly AppDbContext _db;
        private readonly AuthServices _authService;
        private readonly IMapper _mapper;

        public AuthController(AppDbContext db, AuthServices authServices, IMapper mapper)
        {
            _db = db;
            _authService = authServices;
            _mapper = mapper;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginSolicitudDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var usuario = await _authService.ValidateUserAsync(dto.NombreUsuario, dto.Password);
            if (usuario is null)
            {
                return Unauthorized("Credenciales inválidas");
            }
            var fechaNow = DateTime.Today;
            var horaNow = DateTime.Now.TimeOfDay;

            var log = new LogAcceso
            {
                IdUsuario = usuario.IdUsuario,
                FechaLogin = fechaNow,
                HoraLogin = horaNow,
                FechaLogout = null,
                HoraLogout = null
            };

            await _db.LogAccesos.AddAsync(log);
            await _db.SaveChangesAsync();

            var token = _authService.GenerateJwtToken(usuario);
            var usuarioDTO = _mapper.Map<UsuarioListarDTO>(usuario);
            return Ok(new AuthRespuestaDTO { Token = token, NombreUsuario = usuarioDTO.NombreUsuario, NombreCompleto = usuarioDTO.NombreCompleto, Rol = usuarioDTO.Rol, IdPersona = usuario.IdPersona, IdUsuario = usuario.IdUsuario });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutSolicitudDTO dto)
        {
            // Buscar el último registro de login que aún no tenga logout
            var log = await _db.LogAccesos
                .Where(l => l.IdUsuario == dto.IdUsuario && l.FechaLogout == null)
                .OrderByDescending(l => l.IdLog)
                .FirstOrDefaultAsync();

            if (log is null)
                return NotFound("No existe una sesión activa para cerrar.");

            log.FechaLogout = DateTime.Today;
            log.HoraLogout = DateTime.Now.TimeOfDay;

            await _db.SaveChangesAsync();

            return Ok("Sesión cerrada exitosamente.");
        }
        [HttpGet]
        public async Task<ActionResult<List<LogsAccesoDTO>>> LogsAcceso()
        {
            var logs = await _db.LogAccesos
                .Include(l => l.Usuario)
                    .ThenInclude(u => u.Persona)
                .ToListAsync();

            var logsDTO = _mapper.Map<List<LogsAccesoDTO>>(logs);

            return Ok(logsDTO);
        }

    }
}

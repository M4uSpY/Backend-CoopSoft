using AutoMapper;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs;
using BackendCoopSoft.DTOs.Usuarios;
using BackendCoopSoft.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            if(usuario is null)
            {
                return Unauthorized("Credenciales inválidas");
            }
            var token = _authService.GenerateJwtToken(usuario);
            var usuarioDTO = _mapper.Map<UsuarioListarDTO>(usuario);
            return Ok(new AuthRespuestaDTO{ Token = token, NombreUsuario = usuarioDTO.NombreUsuario, NombreCompleto = usuarioDTO.NombreCompleto, Rol = usuarioDTO.Rol, IdPersona = usuario.IdPersona });
        }
    }
}

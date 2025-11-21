using AutoMapper;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs;
using BackendCoopSoft.DTOs.Usuarios;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;
        public UsuariosController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        // Todos los usuarios
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var usuarios = await _db.Usuarios.Include(u => u.Persona).Include(u => u.Rol).ToListAsync();
            var list = _mapper.Map<List<UsuarioListarDTO>>(usuarios);
            return Ok(list);
        }


        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var usuario = await _db.Usuarios.Include(u => u.Persona).Include(u => u.Rol).FirstOrDefaultAsync(u => u.IdUsuario == id);
            if (usuario is null)
            {
                return NotFound();
            }
            var usuarioDTO = _mapper.Map<UsuarioListarDTO>(usuario);
            return Ok(usuarioDTO);
        }


        [HttpPost]
        public async Task<IActionResult> CrearUsuario([FromBody] UsuarioCrearDTO usuarioDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = _mapper.Map<Usuario>(usuarioDTO);

            // Hasheo de password
            usuario.Password = BCrypt.Net.BCrypt.HashPassword(usuarioDTO.Password);

            await _db.Usuarios.AddAsync(usuario);
            await _db.SaveChangesAsync();

            var usuarioCreado = _mapper.Map<UsuarioListarDTO>(usuario);
            return CreatedAtAction(nameof(GetById), new { id = usuario.IdUsuario }, usuarioCreado);
        }


        [HttpPut("{id:int}")]
        public async Task<IActionResult> ActualizarUsuario(int id, [FromBody] UsuarioActualizarDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);
            if (usuario is null)
            {
                return NotFound();
            }

            _mapper.Map(dto, usuario);

            // Si vino una nueva contraseña entonces se procede al hasheo
            if (!string.IsNullOrWhiteSpace(dto.PasswordNueva))
            {
                usuario.Password = BCrypt.Net.BCrypt.HashPassword(dto.PasswordNueva);
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);
            if (usuario is null)
            {
                return NotFound();
            }
            _db.Usuarios.Remove(usuario);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}

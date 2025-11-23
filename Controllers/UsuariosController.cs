using System.Security.Claims;
using AutoMapper;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs;
using BackendCoopSoft.DTOs.Usuarios;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;

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
            var usuarios = await _db.Usuarios.Where(u => u.EstadoUsuario == true).Include(u => u.Persona).Include(u => u.Rol).ToListAsync();
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


            // Después de tener IdUsuario
            var idUsuarioActual = ObtenerIdUsuarioActual();
            if (idUsuarioActual is not null)
            {
                var historico = new HistoricoUsuario
                {
                    IdUsuario = usuario.IdUsuario,
                    UsuarioModificoId = idUsuarioActual.Value,
                    FechaModificacion = DateTime.Now,
                    Accion = "CREAR",
                    ApartadosModificados = "Todos los campos"
                };

                await _db.HistoricosUsuario.AddAsync(historico);
                await _db.SaveChangesAsync();
            }

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

            var idUsuarioActual = ObtenerIdUsuarioActual();
            if (idUsuarioActual is null)
            {
                return Unauthorized("No se pudo identificar al usuario que realiza la modificación.");
            }


            var antiguoNombreUsuario = usuario.NombreUsuario;
            var antiguoEstado = usuario.EstadoUsuario;
            var antiguoRol = usuario.IdRol;

            _mapper.Map(dto, usuario);

            // Si vino una nueva contraseña entonces se procede al hasheo
            if (!string.IsNullOrWhiteSpace(dto.PasswordNueva))
            {
                usuario.Password = BCrypt.Net.BCrypt.HashPassword(dto.PasswordNueva);
            }

            // Detectar cambios
            List<string> cambios = new();

            if (antiguoNombreUsuario != usuario.NombreUsuario)
                cambios.Add("NombreUsuario");

            if (antiguoEstado != usuario.EstadoUsuario)
                cambios.Add("EstadoUsuario");

            if (!string.IsNullOrWhiteSpace(dto.PasswordNueva))
                cambios.Add("Password");

            if (antiguoRol != usuario.IdRol)
                cambios.Add("Rol");

            // Registrar histórico SOLO SI CAMBIÓ ALGO
            if (cambios.Count > 0)
            {
                var historico = new HistoricoUsuario
                {
                    IdUsuario = usuario.IdUsuario,
                    UsuarioModificoId = idUsuarioActual.Value,
                    FechaModificacion = DateTime.Now,
                    Accion = "ACTUALIZAR",
                    ApartadosModificados = string.Join(", ", cambios)
                };

                await _db.HistoricosUsuario.AddAsync(historico);
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

            if (!usuario.EstadoUsuario)
            {
                return BadRequest("El usuario se encuentra inactivo");
            }

            var idUsuarioActual = ObtenerIdUsuarioActual();
            if (idUsuarioActual is null)
            {
                return Unauthorized("No se pudo identificar al usuario que quiere modificar.");
            }

            usuario.EstadoUsuario = false;

            var historico = new HistoricoUsuario
            {
                IdUsuario = usuario.IdUsuario,
                UsuarioModificoId = idUsuarioActual.Value,
                FechaModificacion = DateTime.Now,
                Accion = "INACTIVAR",
                ApartadosModificados = "EstadoUsuario"
            };

            await _db.HistoricosUsuario.AddAsync(historico);


            await _db.SaveChangesAsync();
            return NoContent();
        }

        private int? ObtenerIdUsuarioActual()
        {
            // 1) Intentar encontrar el claim "sub"
            var claimSub = User.FindFirst(JwtRegisteredClaimNames.Sub);

            // 2) Si no está, intentar con NameIdentifier (mapeo por defecto)
            var claimNameId = User.FindFirst(ClaimTypes.NameIdentifier); // verificar using

            var claim = claimSub ?? claimNameId;

            if (claim is null)
                return null;

            return int.TryParse(claim.Value, out var idUsuario)
                ? idUsuario
                : (int?)null;
        }

        // HISTORIAL DE UN USUARIO
        [HttpGet("{id:int}/historico")]
        public async Task<IActionResult> GetHistoricoUsuario(int id)
        {
            var historicos = await _db.HistoricosUsuario
                .Where(h => h.IdUsuario == id)
                .Include(h => h.UsuarioModifico)
                .OrderByDescending(h => h.FechaModificacion)
                .ToListAsync();

            // Podrías mapear a un DTO HistoricoUsuarioDTO si no quieres exponer todo
            return Ok(historicos);
        }

    }
}

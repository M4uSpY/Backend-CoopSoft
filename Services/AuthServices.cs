using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackendCoopSoft.Data;
using BackendCoopSoft.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BackendCoopSoft.Services
{
    public class AuthServices
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _cfg;

        public AuthServices(AppDbContext db, IConfiguration configuration)
        {
            _db = db;
            _cfg = configuration;
        }

        public async Task<Usuario?> ValidateUserAsync(string username, string password)
        {
            // COLOCAR QUE EXACTAMENTE SE IGUAL EL NOMBREDE USUARIO
            var user = await _db.Usuarios
                .Include(u => u.Persona)
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.NombreUsuario == username);

            if (user is null)
                return null;

            // Verificacion de usuario
            if (string.Equals(user.NombreUsuario, username, StringComparison.Ordinal) &&
                BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return user;
            }

            return null;
        }


        public string GenerateJwtToken(Usuario user)
        {
            var key = _cfg["Jwt:Key"];
            var keyBytes = Encoding.UTF8.GetBytes(key!);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, user.NombreUsuario),
                new Claim("fullname", $"{user.Persona.PrimerNombre} {user.Persona.ApellidoPaterno}"),
                new Claim(ClaimTypes.Role, user.Rol.NombreRol)
            };

            var creds = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(4),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

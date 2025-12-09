using AutoMapper;
using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Extras;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public RolesController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> ObtenerRoles()
        {
            var roles = await _db.Roles.ToListAsync();
            var rolesDTO = _mapper.Map<List<RolDTO>>(roles);
            return Ok(rolesDTO);
        }
    }
}

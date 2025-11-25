using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Planillas;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanillaAportesPatronalesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public PlanillaAportesPatronalesController(AppDbContext db)
        {
            _db = db;
        }

        // GET api/PlanillaAportesPatronales/5
        // Usa la MISMA planilla de sueldos (idPlanilla) y desde ahí
        // calcula los aportes patronales en memoria.
        [HttpGet("{idPlanilla:int}")]
        public async Task<ActionResult<List<PlanillaAportesFilaDTO>>> GetPorPlanilla(int idPlanilla)
        {
            var registros = await _db.TrabajadorPlanillas
                .Where(tp => tp.IdPlanilla == idPlanilla)
                .Include(tp => tp.Trabajador)
                    .ThenInclude(t => t.Persona)
                .Include(tp => tp.Trabajador)
                    .ThenInclude(t => t.Cargo)
                .Include(tp => tp.TrabajadorPlanillaValors)
                    .ThenInclude(v => v.Concepto)
                .OrderBy(tp => tp.Trabajador.Persona.ApellidoPaterno)
                .ThenBy(tp => tp.Trabajador.Persona.ApellidoMaterno)
                .ThenBy(tp => tp.Trabajador.Persona.PrimerNombre)
                .ThenBy(tp => tp.Trabajador.Persona.SegundoNombre)
                .ToListAsync();

            if (!registros.Any())
                return NotFound("No existen trabajadores asociados a esta planilla.");

            decimal GetValor(TrabajadorPlanilla tp, string codigo) =>
                tp.TrabajadorPlanillaValors
                    .Where(v => v.Concepto.Codigo == codigo)
                    .Sum(v => v.Valor);

            var lista = registros
                .Select((tp, index) =>
                {
                    var persona = tp.Trabajador.Persona;

                    // === Total ganado tomado de la planilla de sueldos ===
                    var haberBasico = GetValor(tp, "HABER_BASICO");
                    var bonoAnt = GetValor(tp, "BONO_ANT");
                    var bonoProd = GetValor(tp, "BONO_PROD");       // puede ser 0
                    var apCoop = GetValor(tp, "AP_COOP_334");

                    var totalGanado = haberBasico + bonoAnt + bonoProd + apCoop;

                    // === Aportes patronales ===
                    var cps = Math.Round(totalGanado * 0.10m, 2);      // 10 %
                    var riesgo = Math.Round(totalGanado * 0.0171m, 2); // 1.71 %
                    var provivienda = Math.Round(totalGanado * 0.02m, 2);  // 2 %
                    var apSolidario = Math.Round(totalGanado * 0.035m, 2); // 3.5 %

                    var totalAportes = cps + riesgo + provivienda + apSolidario;

                    return new PlanillaAportesFilaDTO
                    {
                        Id = index + 1,
                        CarnetIdentidad = persona.CarnetIdentidad,
                        ApellidosNombres = string.Join(" ",
                            persona.ApellidoPaterno,
                            persona.ApellidoMaterno,
                            persona.PrimerNombre,
                            persona.SegundoNombre ?? string.Empty).Trim(),
                        Nacionalidad = persona.Nacionalidad?.ValorCategoria ?? "BOL",
                        FechaNacimiento = persona.FechaNacimiento,
                        Sexo = persona.Genero ? "M" : "F",
                        Ocupacion = tp.Trabajador.Cargo.NombreCargo,
                        FechaIngreso = tp.Trabajador.FechaIngreso,
                        DiasPagados = tp.DiasTrabajados,

                        TotalGanado = Math.Round(totalGanado, 2),
                        Cps10 = cps,
                        RiesgoPrima171 = riesgo,
                        Provivienda2 = provivienda,
                        AporteSolidario35 = apSolidario,
                        TotalAportes = Math.Round(totalAportes, 2)
                    };
                })
                .ToList();

            return Ok(lista);
        }
    }
}

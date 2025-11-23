using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Planillas;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanillaSueldosSalariosController : ControllerBase
    {
        private readonly AppDbContext _db;

        public PlanillaSueldosSalariosController(AppDbContext db)
        {
            _db = db;
        }

        // ================================
        // 0) RESUMEN DE UNA PLANILLA
        // ================================
        [HttpGet("resumen/{idPlanilla:int}")]
        public async Task<ActionResult<PlanillaResumenDTO>> GetResumen(int idPlanilla)
        {
            var p = await _db.Planillas.FindAsync(idPlanilla);
            if (p == null) return NotFound("No existe la planilla.");

            var dto = new PlanillaResumenDTO
            {
                IdPlanilla = p.IdPlanilla,
                IdTipoPlanilla = p.IdTipoPlanilla,
                Gestion = p.Gestion,
                Mes = p.Mes,
                PeriodoDesde = p.PeriodoDesde,
                PeriodoHasta = p.PeriodoHasta,
                EstaCerrada = p.EstaCerrada
            };

            return Ok(dto);
        }

        // ================================
        // 1) CREAR PLANILLA (encabezado)
        // ================================
        [HttpPost]
        public async Task<ActionResult<PlanillaResumenDTO>> CrearPlanilla([FromBody] PlanillaCrearDTO dto)
        {
            if (dto == null)
                return BadRequest("Datos inválidos.");

            bool yaExiste = await _db.Planillas.AnyAsync(p =>
                p.IdTipoPlanilla == dto.IdTipoPlanilla &&
                p.Gestion == dto.Gestion &&
                p.Mes == dto.Mes);

            if (yaExiste)
                return Conflict("Ya existe una planilla para esa gestión, mes y tipo.");

            var planilla = new Planilla
            {
                IdTipoPlanilla = dto.IdTipoPlanilla,
                Gestion = dto.Gestion,
                Mes = dto.Mes,
                PeriodoDesde = dto.PeriodoDesde,
                PeriodoHasta = dto.PeriodoHasta,
                EstaCerrada = false
            };

            _db.Planillas.Add(planilla);
            await _db.SaveChangesAsync();

            var resp = new PlanillaResumenDTO
            {
                IdPlanilla = planilla.IdPlanilla,
                IdTipoPlanilla = planilla.IdTipoPlanilla,
                Gestion = planilla.Gestion,
                Mes = planilla.Mes,
                PeriodoDesde = planilla.PeriodoDesde,
                PeriodoHasta = planilla.PeriodoHasta,
                EstaCerrada = planilla.EstaCerrada
            };

            return CreatedAtAction(nameof(GetResumen), new { idPlanilla = planilla.IdPlanilla }, resp);
        }

        // ==========================================
        // 2) GENERAR Trabajador_Planilla automáticamente
        // ==========================================
        [HttpPost("{idPlanilla:int}/generar-trabajadores")]
        public async Task<IActionResult> GenerarTrabajadores(int idPlanilla)
        {
            var planilla = await _db.Planillas
                .FirstOrDefaultAsync(p => p.IdPlanilla == idPlanilla);

            if (planilla == null)
                return NotFound("No existe la planilla.");

            if (planilla.EstaCerrada)
                return BadRequest("La planilla está cerrada y no se puede modificar.");

            var trabajadores = await _db.Trabajadores
                .Include(t => t.Cargo)
                .Where(t => t.EstadoTrabajador)
                .ToListAsync();

            var existentes = await _db.TrabajadorPlanillas
                .Where(tp => tp.IdPlanilla == idPlanilla)
                .ToListAsync();

            var nuevos = new List<TrabajadorPlanilla>();

            foreach (var t in trabajadores)
            {
                if (existentes.Any(e => e.IdTrabajador == t.IdTrabajador))
                    continue;

                int diasPeriodo = (planilla.PeriodoHasta - planilla.PeriodoDesde).Days + 1;
                if (diasPeriodo < 0) diasPeriodo = 0;

                int antigMeses = CalcularMesesAntiguedad(t.FechaIngreso, planilla.PeriodoHasta);

                var fila = new TrabajadorPlanilla
                {
                    IdPlanilla = planilla.IdPlanilla,
                    IdTrabajador = t.IdTrabajador,
                    EsAportante = true,
                    AfiliadoGestora = true,
                    AfiliadoCaja = true,
                    AfiliadoProvivienda = true,

                    NombreCargoMes = t.Cargo.NombreCargo,
                    HaberBasicoMes = t.HaberBasico,
                    DiasTrabajados = diasPeriodo,
                    HorasTrabajadas = diasPeriodo * 8,
                    AntiguedadMeses = antigMeses
                };

                nuevos.Add(fila);
            }

            if (nuevos.Count == 0)
                return Ok("No hay trabajadores nuevos para agregar.");

            _db.TrabajadorPlanillas.AddRange(nuevos);
            await _db.SaveChangesAsync();

            return Ok($"Se generaron {nuevos.Count} filas de Trabajador_Planilla.");
        }

        private static int CalcularMesesAntiguedad(DateTime fechaIngreso, DateTime hasta)
        {
            if (hasta < fechaIngreso) return 0;

            int meses = (hasta.Year - fechaIngreso.Year) * 12 +
                        (hasta.Month - fechaIngreso.Month);

            if (hasta.Day < fechaIngreso.Day)
                meses--;

            return meses < 0 ? 0 : meses;
        }

        // ==========================================
        // 3) CALCULAR PLANILLA (tu método)
        // ==========================================
        [HttpPost("{idPlanilla:int}/calcular")]
        public async Task<IActionResult> CalcularPlanilla(int idPlanilla)
        {
            var planilla = await _db.Planillas
                .Include(p => p.TrabajadorPlanillas)
                    .ThenInclude(tp => tp.TrabajadorPlanillaValors)
                .Include(p => p.TrabajadorPlanillas)
                    .ThenInclude(tp => tp.Trabajador)
                .FirstOrDefaultAsync(p => p.IdPlanilla == idPlanilla);

            if (planilla == null)
                return NotFound("No existe la planilla.");

            if (planilla.EstaCerrada)
                return BadRequest("La planilla está cerrada y no se puede recalcular.");

            if (!planilla.TrabajadorPlanillas.Any())
                return BadRequest("No hay trabajadores en esta planilla. Primero ejecuta /generar-trabajadores.");

            var conceptos = await _db.Conceptos.ToDictionaryAsync(c => c.Codigo);

            foreach (var tp in planilla.TrabajadorPlanillas)
            {
                var manuales = tp.TrabajadorPlanillaValors
                    .Where(v => v.EsManual)
                    .ToList();

                var autos = tp.TrabajadorPlanillaValors
                    .Where(v => !v.EsManual)
                    .ToList();

                if (autos.Any())
                    _db.TrabajadorPlanillaValores.RemoveRange(autos);

                decimal totalIngresos = 0m;
                decimal totalDescuentos = 0m;

                TrabajadorPlanillaValor Add(string codigo, decimal valor)
                {
                    if (!conceptos.TryGetValue(codigo, out var c))
                        return null!;

                    var v = new TrabajadorPlanillaValor
                    {
                        IdTrabajadorPlanilla = tp.IdTrabajadorPlanilla,
                        IdConcepto = c.IdConcepto,
                        Valor = Math.Round(valor, 2),
                        EsManual = false
                    };
                    _db.TrabajadorPlanillaValores.Add(v);
                    return v;
                }

                // ========= INGRESOS =========
                var haberBasico = tp.HaberBasicoMes;
                if (conceptos.ContainsKey("HABER_BASICO"))
                {
                    Add("HABER_BASICO", haberBasico);
                    totalIngresos += haberBasico;
                }

                decimal bonoAnt = 0m;
                if (conceptos.ContainsKey("BONO_ANT"))
                {
                    decimal porcAnt = ObtenerPorcentajeAntiguedad(tp.AntiguedadMeses);
                    bonoAnt = tp.HaberBasicoMes * porcAnt;
                    Add("BONO_ANT", bonoAnt);
                    totalIngresos += bonoAnt;
                }

                decimal bonoProdManual = manuales
                    .Where(m => m.Concepto.Codigo == "BONO_PROD")
                    .Sum(m => m.Valor);
                totalIngresos += bonoProdManual;

                decimal apCoop = 0m;
                if (conceptos.ContainsKey("AP_COOP_334"))
                {
                    apCoop = tp.HaberBasicoMes * 0.0334m;
                    Add("AP_COOP_334", apCoop);
                    totalIngresos += apCoop;
                }

                var totalGanado = totalIngresos;

                // ========= DESCUENTOS =========
                decimal gestora = 0m;
                if (conceptos.ContainsKey("GESTORA_1221"))
                {
                    gestora = totalGanado * 0.1221m;
                    Add("GESTORA_1221", gestora);
                    totalDescuentos += gestora;
                }

                decimal rcIvaManual = manuales
                    .Where(m => m.Concepto.Codigo == "RC_IVA_13")
                    .Sum(m => m.Valor);
                totalDescuentos += rcIvaManual;

                decimal apSol = 0m;
                if (conceptos.ContainsKey("AP_SOL_05"))
                {
                    apSol = totalGanado * 0.005m;
                    Add("AP_SOL_05", apSol);
                    totalDescuentos += apSol;
                }

                decimal otros668 = 0m;
                if (conceptos.ContainsKey("OTROS_DESC_668"))
                {
                    otros668 = tp.HaberBasicoMes * 0.0668m;
                    Add("OTROS_DESC_668", otros668);
                    totalDescuentos += otros668;
                }

                decimal otrosDescManual = manuales
                    .Where(m => m.Concepto.Codigo == "OTROS_DESC")
                    .Sum(m => m.Valor);
                totalDescuentos += otrosDescManual;

                foreach (var m in manuales)
                {
                    if (_db.Entry(m).State == EntityState.Detached)
                        _db.TrabajadorPlanillaValores.Attach(m);
                }
            }

            await _db.SaveChangesAsync();
            return Ok("Planilla calculada correctamente para Sueldos y Salarios.");
        }

        private static decimal ObtenerPorcentajeAntiguedad(int antiguedadMeses)
        {
            int años = antiguedadMeses / 12;

            if (años < 2) return 0m;
            if (años < 5) return 0.05m;
            if (años < 8) return 0.11m;
            if (años < 11) return 0.18m;
            if (años < 15) return 0.26m;
            if (años < 20) return 0.34m;
            if (años < 25) return 0.42m;
            return 0.50m;
        }

        // ==========================================
        // 4) OBTENER FILAS TIPO EXCEL
        // ==========================================
        [HttpGet("{idPlanilla:int}")]
        public async Task<ActionResult<List<PlanillaSueldosFilaDTO>>> GetPorPlanilla(int idPlanilla)
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

                    var haberBasico = tp.HaberBasicoMes;
                    var bonoAnt = GetValor(tp, "BONO_ANT");
                    var bonoProd = GetValor(tp, "BONO_PROD");
                    var apCoop = GetValor(tp, "AP_COOP_334");

                    var totalGanado = haberBasico + bonoAnt + bonoProd + apCoop;

                    var gestora = GetValor(tp, "GESTORA_1221");
                    var rcIva = GetValor(tp, "RC_IVA_13");
                    var apSol = GetValor(tp, "AP_SOL_05");
                    var otros668 = GetValor(tp, "OTROS_DESC_668");
                    var otrosDesc = GetValor(tp, "OTROS_DESC");

                    var totalDesc = gestora + rcIva + apSol + otros668 + otrosDesc;
                    var liquido = totalGanado - totalDesc;

                    return new PlanillaSueldosFilaDTO
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

                        HaberBasico = Math.Round(haberBasico, 2),
                        BonoAntiguedad = Math.Round(bonoAnt, 2),
                        BonoProduccion = Math.Round(bonoProd, 2),
                        AporteCoop334 = Math.Round(apCoop, 2),
                        TotalGanado = Math.Round(totalGanado, 2),

                        Gestora1221 = Math.Round(gestora, 2),
                        RcIva13 = Math.Round(rcIva, 2),
                        AporteSolidario05 = Math.Round(apSol, 2),
                        OtrosDesc668 = Math.Round(otros668, 2),
                        OtrosDescuentos = Math.Round(otrosDesc, 2),
                        TotalDescuentos = Math.Round(totalDesc, 2),

                        LiquidoPagable = Math.Round(liquido, 2),
                        FirmaEmpleado = string.Empty
                    };
                })
                .ToList();

            return Ok(lista);
        }

        // ==========================================
        // 5) CERRAR PLANILLA
        // ==========================================
        [HttpPut("{idPlanilla:int}/cerrar")]
        public async Task<IActionResult> CerrarPlanilla(int idPlanilla)
        {
            var planilla = await _db.Planillas.FirstOrDefaultAsync(p => p.IdPlanilla == idPlanilla);

            if (planilla == null)
                return NotFound("No existe la planilla.");

            if (planilla.EstaCerrada)
                return BadRequest("La planilla ya estaba cerrada.");

            planilla.EstaCerrada = true;
            planilla.FechaCierre = DateTime.Now.Date;

            await _db.SaveChangesAsync();
            return Ok("Planilla cerrada correctamente.");
        }
    }
}

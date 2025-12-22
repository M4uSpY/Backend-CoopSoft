using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.Planillas;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;


namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador,Consejo")]
    public class PlanillaSueldosSalariosController : ControllerBase
    {
        private readonly AppDbContext _db;

        // SMN vigente (ajusta cuando cambie)
        private const decimal SMN_2025 = 2750m;

        // BASE para el bono de antigüedad = 3 x SMN
        private const decimal BASE_BONO_ANT = SMN_2025 * 3m; // 8250 Bs

        private const string CATEGORIA_TIPO_FALTA = "TipoFalta";

        private const int DIAS_MES_PLANILLA = 30;
        private const int HORAS_POR_DIA = 8;

        private const string VALOR_INASISTENCIA_LABORAL = "Inasistencia laboral";

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


                // Antigüedad en meses (si quieres seguir guardando este campo)
                int antigMeses = CalcularMesesAntiguedad(t.FechaIngreso, planilla.PeriodoHasta);

                var diasIniciales = DIAS_MES_PLANILLA;

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

                    DiasTrabajados = diasIniciales,
                    HorasTrabajadas = diasIniciales * HORAS_POR_DIA,

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

        // Antigüedad en meses (puede servir para reportes históricos)
        private static int CalcularMesesAntiguedad(DateTime fechaIngreso, DateTime hasta)
        {
            if (hasta < fechaIngreso) return 0;

            int meses = (hasta.Year - fechaIngreso.Year) * 12 +
                        (hasta.Month - fechaIngreso.Month);

            if (hasta.Day < fechaIngreso.Day)
                meses--;

            return meses < 0 ? 0 : meses;
        }

        // Años completos de antigüedad (11.8 -> 11)
        private static int CalcularAniosAntiguedad(DateTime fechaIngreso, DateTime hasta)
        {
            int anios = hasta.Year - fechaIngreso.Year;

            if (hasta.Month < fechaIngreso.Month ||
               (hasta.Month == fechaIngreso.Month && hasta.Day < fechaIngreso.Day))
            {
                anios--;
            }

            return anios < 0 ? 0 : anios;
        }

        // Porcentaje de bono antigüedad según tabla (años)
        private static decimal ObtenerPorcentajeAntiguedad(int anios)
        {
            if (anios < 2) return 0m;      // de 0 a 2
            if (anios < 5) return 0.05m;   // de 2 a 5
            if (anios < 8) return 0.11m;   // de 5 a 8
            if (anios < 11) return 0.18m;  // de 8 a 11
            if (anios < 15) return 0.26m;  // de 11 a 15
            if (anios < 20) return 0.34m;  // de 15 a 20
            if (anios < 25) return 0.42m;  // de 20 a 25
            return 0.50m;                  // más de 25
        }


        // ==========================================
        // 3) CALCULAR PLANILLA
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

            var idTipoInasistenciaLaboral = await _db.Clasificadores
                .Where(c => c.Categoria == CATEGORIA_TIPO_FALTA &&
                            c.ValorCategoria == VALOR_INASISTENCIA_LABORAL)
                .Select(c => c.IdClasificador)
                .FirstOrDefaultAsync();

            bool puedeDescontarInasistencias = idTipoInasistenciaLaboral != 0;

            var conceptos = await _db.Conceptos.ToDictionaryAsync(c => c.Codigo);

            // 👉 Traemos una sola vez las faltas de inasistencia ACTIVAS en el periodo
            List<Falta> faltasInasistenciaActivas = new();
            if (puedeDescontarInasistencias)
            {
                faltasInasistenciaActivas = await _db.Faltas
                    .Where(f =>
                        f.IdTipoFalta == idTipoInasistenciaLaboral &&
                        f.EstadoFalta &&
                        (f.ArchivoJustificativo == null || f.ArchivoJustificativo.Length == 0) &&
                        f.Fecha.Date >= planilla.PeriodoDesde.Date &&
                        f.Fecha.Date <= planilla.PeriodoHasta.Date)
                    .ToListAsync();
            }

            foreach (var tp in planilla.TrabajadorPlanillas)
            {
                // ==========================================
                // a) Base = 30 días
                // ==========================================
                int diasTrabajados = DIAS_MES_PLANILLA;

                if (puedeDescontarInasistencias)
                {
                    // Días con falta de inasistencia activa para este trabajador
                    var diasConFaltaActiva = faltasInasistenciaActivas
                        .Where(f => f.IdTrabajador == tp.IdTrabajador)
                        .Select(f => f.Fecha.Date)
                        .Distinct()
                        .Count();

                    diasTrabajados = DIAS_MES_PLANILLA - diasConFaltaActiva;
                    if (diasTrabajados < 0)
                        diasTrabajados = 0;
                }

                tp.DiasTrabajados = diasTrabajados;
                tp.HorasTrabajadas = diasTrabajados * HORAS_POR_DIA;

                // ==========================================
                // LIMPIEZA DE VALORES AUTOMÁTICOS
                // ==========================================
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
                // 👉 Divisor normativo fijo: 30 días
                var diasPeriodo = DIAS_MES_PLANILLA;
                var diasPagados = tp.DiasTrabajados;
                if (diasPagados > diasPeriodo)
                    diasPagados = diasPeriodo;

                var haberBasicoMensual = tp.HaberBasicoMes;

                // HABER BÁSICO GANADO = HB / 30 * días pagados
                var haberBasico = Math.Round(
                    diasPeriodo > 0 ? (haberBasicoMensual / diasPeriodo * diasPagados) : 0m,
                    2);

                if (conceptos.ContainsKey("HABER_BASICO"))
                {
                    Add("HABER_BASICO", haberBasico);
                    totalIngresos += haberBasico;
                }

                // BONO DE ANTIGÜEDAD = 3 x SMN x % según años
                decimal bonoAnt = 0m;
                if (conceptos.ContainsKey("BONO_ANT"))
                {
                    int aniosAnt = CalcularAniosAntiguedad(
                        tp.Trabajador.FechaIngreso,
                        planilla.PeriodoHasta);

                    decimal porcAnt = ObtenerPorcentajeAntiguedad(aniosAnt);
                    bonoAnt = BASE_BONO_ANT * porcAnt;

                    Add("BONO_ANT", bonoAnt);
                    totalIngresos += bonoAnt;
                }

                // BONO PRODUCCIÓN: solo manual (ya viene en "manuales")
                // decimal bonoProdManual = manuales
                //     .Where(m => m.Concepto.Codigo == "BONO_PROD")
                //     .Sum(m => m.Valor);
                // totalIngresos += bonoProdManual;

                // APORTE COOP 3.34% sobre HABER BÁSICO GANADO
                decimal apCoop = 0m;
                if (conceptos.ContainsKey("AP_COOP_334"))
                {
                    apCoop = Math.Round(haberBasico * 0.0334m, 2);
                    Add("AP_COOP_334", apCoop);
                    totalIngresos += apCoop;
                }

                var totalGanado = totalIngresos;

                // ========= DESCUENTOS =========
                // GESTORA 12.21% sobre TOTAL GANADO
                decimal gestora = 0m;
                if (conceptos.ContainsKey("GESTORA_1221"))
                {
                    gestora = totalGanado * 0.1221m;
                    Add("GESTORA_1221", gestora);
                    totalDescuentos += gestora;
                }

                // RC-IVA: siempre manual (no se recalcula aquí)
                // decimal rcIvaManual = manuales
                //     .Where(m => m.Concepto.Codigo == "RC_IVA_13")
                //     .Sum(m => m.Valor);
                // totalDescuentos += rcIvaManual;

                // APORTE SOLIDARIO 0.5% sobre TOTAL GANADO
                decimal apSol = 0m;
                if (conceptos.ContainsKey("AP_SOL_05"))
                {
                    apSol = totalGanado * 0.005m;
                    Add("AP_SOL_05", apSol);
                    totalDescuentos += apSol;
                }

                // OTROS DESC. 6.68% (por ejemplo CPS) = 2 * apCoop, como lo tenías
                decimal otros668 = 0m;
                if (conceptos.ContainsKey("OTROS_DESC_668"))
                {
                    otros668 = apCoop * 2m;
                    Add("OTROS_DESC_668", otros668);
                    totalDescuentos += otros668;
                }

                // Mantener manuales adjuntos al contexto
                foreach (var m in manuales)
                {
                    if (_db.Entry(m).State == EntityState.Detached)
                        _db.TrabajadorPlanillaValores.Attach(m);
                }
            }

            var idUsuarioActual = ObtenerIdUsuarioActual();
            if (idUsuarioActual != null)
            {
                var historico = new HistoricoPlanilla
                {
                    IdPlanilla = planilla.IdPlanilla,
                    UsuarioModificoId = idUsuarioActual.Value,
                    FechaModificacion = DateTime.Now,
                    Accion = "RECALCULAR",
                    Campo = "CALCULO_AUTOMATICO",
                    ValorAnterior = null,
                    ValorActual = "Planilla de sueldos y salarios recalculada."
                };

                await _db.HistoricosPlanilla.AddAsync(historico);
            }

            await _db.SaveChangesAsync();
            return Ok("Planilla calculada correctamente para Sueldos y Salarios.");
        }




        [HttpPut("trabajador-planilla/{idTrabajadorPlanilla:int}/rc-iva")]
        public async Task<IActionResult> ActualizarRcIva(int idTrabajadorPlanilla, [FromBody] RcIvaActualizarDTO dto)
        {
            if (dto == null)
                return BadRequest("Datos inválidos.");

            var idUsuarioActual = ObtenerIdUsuarioActual();
            if (idUsuarioActual is null)
                return Unauthorized("No se pudo identificar al usuario que realiza la modificación.");

            // 1) Buscar la fila de TrabajadorPlanilla
            var tp = await _db.TrabajadorPlanillas
                .Include(x => x.TrabajadorPlanillaValors)
                    .ThenInclude(v => v.Concepto)
                .FirstOrDefaultAsync(x => x.IdTrabajadorPlanilla == idTrabajadorPlanilla);

            if (tp == null)
                return NotFound("No existe el registro Trabajador_Planilla.");

            // 2) Obtener el concepto RC_IVA_13
            var conceptoRcIva = await _db.Conceptos
                .FirstOrDefaultAsync(c => c.Codigo == "RC_IVA_13");

            if (conceptoRcIva == null)
                return BadRequest("No existe el concepto RC_IVA_13 en la tabla Concepto.");

            // 3) Buscar si ya hay un valor manual para RC_IVA_13
            var existente = tp.TrabajadorPlanillaValors
                .FirstOrDefault(v =>
                    v.EsManual &&
                    v.Concepto != null &&
                    v.Concepto.Codigo == "RC_IVA_13");

            var monto = Math.Round(dto.MontoRcIva, 2);

            // ------- para histórico --------
            decimal valorAnterior = existente?.Valor ?? 0m;
            decimal valorNuevo;

            if (existente == null)
            {
                if (monto <= 0)
                {
                    // Nada que crear
                    return Ok("RC-IVA no registrado (monto <= 0).");
                }

                // Crear un nuevo registro manual
                var nuevo = new TrabajadorPlanillaValor
                {
                    IdTrabajadorPlanilla = tp.IdTrabajadorPlanilla,
                    IdConcepto = conceptoRcIva.IdConcepto,
                    Valor = monto,
                    EsManual = true
                };
                _db.TrabajadorPlanillaValores.Add(nuevo);
                valorNuevo = monto;
            }
            else
            {
                if (monto <= 0)
                {
                    // Si quieres eliminarlo:
                    // _db.TrabajadorPlanillaValores.Remove(existente);

                    // O dejar el registro y ponerlo en 0:
                    existente.Valor = 0m;
                    valorNuevo = 0m;
                }
                else
                {
                    existente.Valor = monto;
                    valorNuevo = monto;
                }
            }

            // Si hubo cambio real, registramos en el histórico
            if (valorAnterior != valorNuevo)
            {
                var historico = new HistoricoTrabajadorPlanilla
                {
                    IdTrabajadorPlanilla = tp.IdTrabajadorPlanilla,
                    UsuarioModificoId = idUsuarioActual.Value,
                    FechaModificacion = DateTime.Now,
                    Accion = "ACTUALIZAR",
                    Campo = "RC_IVA_13",
                    ValorAnterior = valorAnterior.ToString(CultureInfo.InvariantCulture),
                    ValorActual = valorNuevo.ToString(CultureInfo.InvariantCulture)
                };

                await _db.HistoricosTrabajadorPlanilla.AddAsync(historico);
            }

            await _db.SaveChangesAsync();
            return Ok("RC-IVA actualizado correctamente (valor manual).");
        }



        [HttpPut("trabajador-planilla/{idTrabajadorPlanilla:int}/otros-desc")]
        public async Task<IActionResult> ActualizarOtrosDesc(int idTrabajadorPlanilla, [FromBody] OtrosDescActualizarDTO dto)
        {
            if (dto == null)
                return BadRequest("Datos inválidos.");

            var idUsuarioActual = ObtenerIdUsuarioActual();
            if (idUsuarioActual is null)
                return Unauthorized("No se pudo identificar al usuario que realiza la modificación.");


            // 1) Buscar la fila de TrabajadorPlanilla
            var tp = await _db.TrabajadorPlanillas
                .Include(x => x.TrabajadorPlanillaValors)
                    .ThenInclude(v => v.Concepto)
                .FirstOrDefaultAsync(x => x.IdTrabajadorPlanilla == idTrabajadorPlanilla);

            if (tp == null)
                return NotFound("No existe el registro Trabajador_Planilla.");

            // 2) Obtener el concepto OTROS_DESC
            var conceptoOtrosDesc = await _db.Conceptos
                .FirstOrDefaultAsync(c => c.Codigo == "OTROS_DESC");

            if (conceptoOtrosDesc == null)
                return BadRequest("No existe el concepto OTROS_DESC en la tabla Concepto.");

            // 3) Buscar si ya hay un valor manual para OTROS_DESC
            var existente = tp.TrabajadorPlanillaValors
                .FirstOrDefault(v =>
                    v.EsManual &&
                    v.Concepto != null &&
                    v.Concepto.Codigo == "OTROS_DESC");

            var monto = Math.Round(dto.MontoOtrosDesc, 2);

            // ------- para histórico --------
            decimal valorAnterior = existente?.Valor ?? 0m;
            decimal valorNuevo;

            if (existente == null)
            {
                if (monto <= 0)
                {
                    // Nada que crear
                    return Ok("Otros descuentos no registrados (monto <= 0).");
                }

                // Crear un nuevo registro manual
                var nuevo = new TrabajadorPlanillaValor
                {
                    IdTrabajadorPlanilla = tp.IdTrabajadorPlanilla,
                    IdConcepto = conceptoOtrosDesc.IdConcepto,
                    Valor = monto,
                    EsManual = true
                };
                _db.TrabajadorPlanillaValores.Add(nuevo);
                valorNuevo = monto;
            }
            else
            {
                if (monto <= 0)
                {
                    // Igual que en RC-IVA: puedes eliminar o dejar en 0, aquí lo dejo en 0
                    // _db.TrabajadorPlanillaValores.Remove(existente);
                    existente.Valor = 0m;
                    valorNuevo = 0m;
                }
                else
                {
                    existente.Valor = monto;
                    valorNuevo = monto;
                }
            }

            if (valorAnterior != valorNuevo)
            {
                var historico = new HistoricoTrabajadorPlanilla
                {
                    IdTrabajadorPlanilla = tp.IdTrabajadorPlanilla,
                    UsuarioModificoId = idUsuarioActual.Value,
                    FechaModificacion = DateTime.Now,
                    Accion = "ACTUALIZAR",
                    Campo = "OTROS_DESC",
                    ValorAnterior = valorAnterior.ToString(CultureInfo.InvariantCulture),
                    ValorActual = valorNuevo.ToString(CultureInfo.InvariantCulture)
                };

                await _db.HistoricosTrabajadorPlanilla.AddAsync(historico);
            }

            await _db.SaveChangesAsync();
            return Ok("Otros descuentos (OTROS_DESC) actualizados correctamente (valor manual).");
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

                    var haberBasico = GetValor(tp, "HABER_BASICO");
                    var bonoAnt = GetValor(tp, "BONO_ANT");
                    var bonoProd = GetValor(tp, "BONO_PROD");
                    var apCoop = GetValor(tp, "AP_COOP_334");

                    var totalGanado = haberBasico + bonoAnt + bonoProd + apCoop;

                    var gestora = GetValor(tp, "GESTORA_1221");
                    var rcIvaReal = GetValor(tp, "RC_IVA_13");
                    var apSol = GetValor(tp, "AP_SOL_05");
                    var otros668 = GetValor(tp, "OTROS_DESC_668");
                    var otrosDesc = GetValor(tp, "OTROS_DESC");

                    // REGLA DE UMBRAL
                    var rcIva = rcIvaReal;

                    var totalDesc = gestora + rcIva + apSol + otros668 + otrosDesc;
                    var liquido = totalGanado - totalDesc;

                    return new PlanillaSueldosFilaDTO
                    {
                        Id = index + 1,
                        IdTrabajadorPlanilla = tp.IdTrabajadorPlanilla,

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

            var idUsuarioActual = ObtenerIdUsuarioActual();
            if (idUsuarioActual is null)
                return Unauthorized("No se pudo identificar al usuario que realiza la operación.");

            var estadoAnterior = planilla.EstaCerrada;

            planilla.EstaCerrada = true;
            planilla.FechaCierre = DateTime.Now.Date;

            var historico = new HistoricoPlanilla
            {
                IdPlanilla = planilla.IdPlanilla,
                UsuarioModificoId = idUsuarioActual.Value,
                FechaModificacion = DateTime.Now,
                Accion = "CERRAR",
                Campo = "EstaCerrada / FechaCierre",
                ValorAnterior = $"EstaCerrada={estadoAnterior}",
                ValorActual = $"EstaCerrada={planilla.EstaCerrada}; FechaCierre={planilla.FechaCierre:yyyy-MM-dd}"
            };

            await _db.HistoricosPlanilla.AddAsync(historico);

            await _db.SaveChangesAsync();
            return Ok("Planilla cerrada correctamente.");
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<PlanillaResumenDTO>> BuscarPorPeriodo([FromQuery] int gestion, [FromQuery] int mes, [FromQuery] int idTipoPlanilla = 31)   // 31 = Sueldos y Salarios
        {
            var p = await _db.Planillas
                .FirstOrDefaultAsync(x =>
                    x.IdTipoPlanilla == idTipoPlanilla &&
                    x.Gestion == gestion &&
                    x.Mes == mes);

            if (p == null)
                return NotFound("No existe una planilla de Sueldos y Salarios para esa gestión y mes.");

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


        private int? ObtenerIdUsuarioActual()
        {
            // 1) Intentar encontrar el claim "sub"
            var claimSub = User.FindFirst(JwtRegisteredClaimNames.Sub);

            // 2) Si no está, intentar con NameIdentifier
            var claimNameId = User.FindFirst(ClaimTypes.NameIdentifier);

            var claim = claimSub ?? claimNameId;

            if (claim is null)
                return null;

            return int.TryParse(claim.Value, out var idUsuario)
                ? idUsuario
                : (int?)null;
        }

    }
}

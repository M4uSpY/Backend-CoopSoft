using BackendCoopSoft.Data;
using BackendCoopSoft.DTOs.BoletasPago;
using BackendCoopSoft.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;

namespace BackendCoopSoft.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador,Casual")]
    [ApiController]
    public class BoletasPagoController : ControllerBase
    {
        private readonly AppDbContext _db;
        public BoletasPagoController(AppDbContext db)
        {
            _db = db;
        }

        // =============== 1) LISTA DE BOLETAS POR TRABAJADOR =====================
        [HttpGet("trabajador/{idTrabajador:int}")]
        public async Task<ActionResult<List<BoletaPagoListarDTO>>> GetPorTrabajador(int idTrabajador)
        {
            var filas = await _db.TrabajadorPlanillas
                .Include(tp => tp.Planilla)
                .Include(tp => tp.Trabajador)
                    .ThenInclude(t => t.Persona)
                .Include(tp => tp.Trabajador)
                    .ThenInclude(t => t.Cargo)
                .Include(tp => tp.TrabajadorPlanillaValors)
                    .ThenInclude(v => v.Concepto)
                .Where(tp => tp.IdTrabajador == idTrabajador &&
                             tp.Planilla.EstaCerrada)
                .OrderByDescending(tp => tp.Planilla.Gestion)
                .ThenByDescending(tp => tp.Planilla.Mes)
                .ToListAsync();

            if (!filas.Any())
                return Ok(new List<BoletaPagoListarDTO>());

            decimal GetValor(TrabajadorPlanilla tp, string codigo) =>
                tp.TrabajadorPlanillaValors
                    .Where(v => v.Concepto.Codigo == codigo)
                    .Sum(v => v.Valor);

            var cultura = new CultureInfo("es-ES");
            var lista = new List<BoletaPagoListarDTO>();

            foreach (var tp in filas)
            {
                var persona = tp.Trabajador.Persona;

                var haberBasico = GetValor(tp, "HABER_BASICO");
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

                lista.Add(new BoletaPagoListarDTO
                {
                    IdPlanilla = tp.IdPlanilla,
                    Gestion = tp.Planilla.Gestion,
                    Mes = tp.Planilla.Mes,
                    MesNombre = cultura.DateTimeFormat.GetMonthName(tp.Planilla.Mes).ToUpper(),

                    NombreCompleto = string.Join(" ",
                        persona.ApellidoPaterno,
                        persona.ApellidoMaterno,
                        persona.PrimerNombre,
                        persona.SegundoNombre ?? string.Empty).Trim(),

                    Cargo = tp.Trabajador.Cargo.NombreCargo,
                    DiasTrabajados = tp.DiasTrabajados,
                    TotalGanado = Math.Round(totalGanado, 2),
                    LiquidoPagable = Math.Round(liquido, 2)
                });
            }

            return Ok(lista);
        }

        // =============== 2) GENERAR PDF DE UNA BOLETA =====================
        [HttpGet("trabajador/{idTrabajador:int}/planilla/{idPlanilla:int}/pdf")]
        public async Task<IActionResult> GetBoletaPdf(int idTrabajador, int idPlanilla)
        {
            var tp = await _db.TrabajadorPlanillas
                .Include(tp => tp.Planilla)
                .Include(tp => tp.Trabajador)
                    .ThenInclude(t => t.Persona)
                .Include(tp => tp.Trabajador)
                    .ThenInclude(t => t.Cargo)
                .Include(tp => tp.TrabajadorPlanillaValors)
                    .ThenInclude(v => v.Concepto)
                .FirstOrDefaultAsync(tp => tp.IdTrabajador == idTrabajador &&
                                           tp.IdPlanilla == idPlanilla);

            if (tp == null)
                return NotFound("No se encontró la boleta para ese trabajador y planilla.");

            var dto = ConstruirDetalleBoleta(tp);

            // PDF con QuestPDF
            var pdfBytes = GenerarPdfBoleta(dto);

            var fileName = $"Boleta_{dto.Gestion}_{dto.Mes:D2}_{LimpiarFileName(dto.NombreCompleto)}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }

        // --------- Helpers de negocio ---------
        private static BoletaPagoDetalleDTO ConstruirDetalleBoleta(TrabajadorPlanilla tp)
        {
            decimal GetValor(string codigo) =>
                tp.TrabajadorPlanillaValors
                    .Where(v => v.Concepto.Codigo == codigo)
                    .Sum(v => v.Valor);

            var persona = tp.Trabajador.Persona;

            // Sueldo de contrato (mensual completo)
            var sueldoBasicoContrato = tp.HaberBasicoMes;

            // Haber básico REAL del mes (prorrateado) desde la planilla
            var haberBasicoProrrateado = GetValor("HABER_BASICO");

            var bonoAnt = GetValor("BONO_ANT");
            var bonoProd = GetValor("BONO_PROD");
            var apCoop = GetValor("AP_COOP_334");

            // OJO: total ganado debe usar el HABER BÁSICO PRORRATEADO
            var totalGanado = haberBasicoProrrateado + bonoAnt + bonoProd + apCoop;

            var gestora = GetValor("GESTORA_1221");
            var rcIva = GetValor("RC_IVA_13");
            var apSol = GetValor("AP_SOL_05");
            var otros668 = GetValor("OTROS_DESC_668");
            var otrosDesc = GetValor("OTROS_DESC");

            var otrosFinal = otros668 + otrosDesc;
            var totalDesc = gestora + rcIva + apSol + otrosFinal;
            var liquido = totalGanado - totalDesc;

            var cultura = new CultureInfo("es-ES");

            return new BoletaPagoDetalleDTO
            {
                NombreCompleto = string.Join(" ",
                    persona.ApellidoPaterno,
                    persona.ApellidoMaterno,
                    persona.PrimerNombre,
                    persona.SegundoNombre ?? string.Empty).Trim(),
                Cargo = tp.Trabajador.Cargo.NombreCargo,
                Lugar = "La Paz",
                Gestion = tp.Planilla.Gestion,
                Mes = tp.Planilla.Mes,
                MesNombre = cultura.DateTimeFormat.GetMonthName(tp.Planilla.Mes).ToUpper(),
                FechaIngreso = tp.Trabajador.FechaIngreso,
                DiasTrabajados = tp.DiasTrabajados,

                // Aquí mostramos ambos:
                SueldoBasico = sueldoBasicoContrato,           // sueldo del contrato
                SbPorDiasTrabajados = haberBasicoProrrateado,  // lo realmente ganado por días trabajados
                BonoAntiguedad = bonoAnt,
                OtrosPagos = bonoProd,
                OIAporteInstitucional = apCoop,
                TotalGanado = totalGanado,

                Anticipos = 0m,
                Iva = rcIva,
                AporteGestora = gestora,
                AporteProvivienda = 0m,
                AporteSolidario = apSol,
                OtrosDescuentos = otrosFinal,
                TotalDescuentos = totalDesc,
                LiquidoPagable = liquido
            };
        }


        private static string LimpiarFileName(string nombre)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                nombre = nombre.Replace(c, '_');
            return nombre;
        }

        // --------- Generar PDF (QuestPDF) ---------
        private static byte[] GenerarPdfBoleta(BoletaPagoDetalleDTO d)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img/Logo_Cooperativa.png");
            var logoBytes = System.IO.File.ReadAllBytes(logoPath);

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A4);

                    page.Header().Row(row =>
                    {
                        row.ConstantItem(70)
                            .Image(logoBytes);  // ← AQUI EL LOGO

                        row.RelativeItem()
                            .Text("PAPELETA DE PAGO")
                            .FontSize(18)
                            .Bold()
                            .AlignCenter();
                    });

                    page.Content().Column(col =>
                    {
                        // ENCABEZADO (sin Grid obsoleto)
                        col.Item().PaddingBottom(10).Row(row =>
                        {
                            // Columna izquierda
                            row.RelativeItem().Column(inner =>
                            {
                                inner.Item().Text($"Nombre: {d.NombreCompleto}");
                                inner.Item().Text($"Cargo: {d.Cargo}");
                                inner.Item().Text($"Status: INDEFINIDO");
                            });

                            // Columna derecha
                            row.RelativeItem().Column(inner =>
                            {
                                inner.Item().Text($"Lugar: {d.Lugar}");
                                inner.Item().Text($"Mes: {d.MesNombre}");
                                inner.Item().Text($"Año: {d.Gestion}");
                                inner.Item().Text($"Ingreso: {d.FechaIngreso:dd-MM-yyyy}");
                                inner.Item().Text($"Días Trab.: {d.DiasTrabajados}");
                            });
                        });


                        // TABLA PRINCIPAL (Ingresos / Descuentos)
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(160); // Concepto ingreso
                                columns.ConstantColumn(80);  // Monto ingreso
                                columns.ConstantColumn(30);  // separador
                                columns.ConstantColumn(160); // Concepto descuento
                                columns.ConstantColumn(80);  // Monto descuento
                            });

                            // ENCABEZADOS
                            table.Header(header =>
                            {
                                header.Cell().AlignCenter().Text("INGRESOS").Bold();
                                header.Cell().AlignCenter().Text("Monto").Bold();
                                header.Cell().Text(string.Empty);
                                header.Cell().AlignCenter().Text("DESCUENTOS").Bold();
                                header.Cell().AlignCenter().Text("Monto").Bold();
                            });

                            void Fila(string ci, decimal mi, string cd, decimal md)
                            {
                                table.Cell().Text(ci);
                                table.Cell().AlignRight().Text(mi == 0 ? "-" : mi.ToString("N2"));
                                table.Cell().Text(string.Empty);
                                table.Cell().Text(cd);
                                table.Cell().AlignRight().Text(md == 0 ? "-" : md.ToString("N2"));
                            }

                            // filas
                            Fila("Sueldo Básico:", d.SueldoBasico, "Anticipos:", d.Anticipos);
                            Fila("SB por días trabajados:", d.SbPorDiasTrabajados, "RC-IVA:", d.Iva);
                            Fila("Bono Antigüedad:", d.BonoAntiguedad, "Aporte Gestora:", d.AporteGestora);
                            Fila("Otros pagos:", d.OtrosPagos, "Aporte Provivienda:", d.AporteProvivienda);
                            Fila("O.I. Aporte Institucional:", d.OIAporteInstitucional, "Aporte Solidario:", d.AporteSolidario);
                            Fila("", 0m, "Otros Descuentos:", d.OtrosDescuentos);

                            // fila totales
                            table.Cell().Text("TOTAL GANADO:").Bold();
                            table.Cell().AlignRight().Text(d.TotalGanado.ToString("N2")).Bold();
                            table.Cell().Text(string.Empty);
                            table.Cell().Text("TOTAL DESCUENTOS:").Bold();
                            table.Cell().AlignRight().Text(d.TotalDescuentos.ToString("N2")).Bold();
                        });

                        col.Item().PaddingTop(10).AlignRight()
                            .Text($"LÍQUIDO PAGABLE: {d.LiquidoPagable:N2}")
                            .Bold();

                        col.Item().PaddingTop(30).Row(row =>
                        {
                            row.RelativeItem().AlignCenter().Text("Sello Cooperativa");
                            row.RelativeItem().AlignCenter().Text("Firma del Empleado(a)\nRecibí conforme");
                        });
                    });
                });
            });

            using var ms = new MemoryStream();
            doc.GeneratePdf(ms);
            return ms.ToArray();
        }
    }
}

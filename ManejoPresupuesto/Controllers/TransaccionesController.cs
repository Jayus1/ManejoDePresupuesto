using AutoMapper;
using ClosedXML.Excel;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Data;
using System.Reflection;

namespace ManejoPresupuesto.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IServicioUsuario servicioUsuario;
        private readonly IRepositorioCategorias repositorioCategorias;
        private readonly IMapper mapper;
        private readonly IServicioReportes servicioReportes;

        public TransaccionesController(IRepositorioTransacciones repositorioTransacciones, 
            IRepositorioCuentas repositorioCuentas, IServicioUsuario servicioUsuario, 
            IRepositorioCategorias repositorioCategorias, IMapper mapper, IServicioReportes servicioReportes)
        {
            this.repositorioTransacciones = repositorioTransacciones;
            this.repositorioCuentas = repositorioCuentas;
            this.servicioUsuario = servicioUsuario;
            this.repositorioCategorias = repositorioCategorias;
            this.mapper = mapper;
            this.servicioReportes = servicioReportes;
        }

        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
            return View(modelo);
        }
        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCreacionViewModel modelo)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }
            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);

            if (cuenta is null)
                return RedirectToAction("NoEncontrado", "Home");

            var categoria = await repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);
            if(categoria is null)
                return RedirectToAction("NoEncontrado", "Home");

            modelo.UsuarioId = usuarioId;

            if(modelo.TipoOperacionId == TipoOperacion.Gastos)
            {
                modelo.Monto *= -1;
            }

            await repositorioTransacciones.Crear(modelo);

            return RedirectToAction("Index");

        }
        public async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuenta = await repositorioCuentas.Buscar(usuarioId);
            return cuenta.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        public async Task<IActionResult> Index(int mes, int año)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var modelo = await servicioReportes.ObtenerReporteTransaccionesDetalladas(usuarioId, mes, año, ViewBag);
            return View(modelo);
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias
            (int usuarioId, TipoOperacion tipoOperacion)
        {
            var categorias = await repositorioCategorias.Obtener(usuarioId, tipoOperacion);
            return categorias.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerCategorias([FromBody] TipoOperacion tipoOperacion)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var categorias = await ObtenerCategorias(usuarioId, tipoOperacion);
            return Ok(categorias);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id, string urlRetorno=null)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var transaccion = await repositorioTransacciones.ObtenerPorId(id, usuarioId);

            if (transaccion is null)
                return RedirectToAction("NoEncontrado", "Home");

            var modelo = mapper.Map<TransaccionActualizacionViewModel>(transaccion);

            modelo.MontoAnterior = modelo.Monto;

            if (modelo.TipoOperacionId == TipoOperacion.Gastos)
            {
                modelo.MontoAnterior = modelo.Monto * -1;
            }

            modelo.CuentaAnteriorId = transaccion.CuentaId;
            modelo.Categorias = await ObtenerCategorias(usuarioId, transaccion.TipoOperacionId);
            modelo.Cuentas = await ObtenerCuentas(usuarioId);
            modelo.urlRetorno=urlRetorno;
            return View(modelo);
        }

        [HttpPost]
        public async Task<IActionResult> Editar (TransaccionActualizacionViewModel modelo)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();

            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }
            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);
             
            if (cuenta is null)
                return RedirectToAction("NoEncontrado", "Home");

            var categoria = await repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);
            if (categoria is null)
                return RedirectToAction("NoEncontrado", "Home");

            var transaccion = mapper.Map<Transaccion>(modelo);

            if (modelo.TipoOperacionId == TipoOperacion.Gastos)
            {
                transaccion.Monto *= -1;
            }

            await repositorioTransacciones.Actualizar(transaccion,modelo.MontoAnterior,modelo.CuentaAnteriorId);

            if (string.IsNullOrEmpty(modelo.urlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(modelo.urlRetorno);
            }

        }

        [HttpPost]
        public async Task<IActionResult> Borrar(int id, string urlRetorno = null)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var transacciones = await repositorioTransacciones.ObtenerPorId(id,usuarioId);

            if (transacciones is null)
                return RedirectToAction("NoEncontrado", "Home");
            
            await repositorioTransacciones.Borrar(id);
            if (string.IsNullOrEmpty(urlRetorno))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return LocalRedirect(urlRetorno);
            }
        }

        public async Task<IActionResult> Semanal(int mes, int año)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            IEnumerable<ResultadoObtenerPorSemana> transaccionesPorSemana = 
                await servicioReportes.ObtenerReporteSemanal(usuarioId, mes, año, ViewBag);

            var agrupado = transaccionesPorSemana.GroupBy(x => x.Semana).Select(x => 
            new ResultadoObtenerPorSemana()
            {
                Semana=x.Key,
                Ingresos=x.Where(x=>x.TipoOperacionId == TipoOperacion.Ingresos)
                .Select(x => x.Monto).FirstOrDefault(),
                Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gastos)
                .Select(x => x.Monto).FirstOrDefault(),
            }).ToList();

            if (año== 0 || mes == 0)
            {
                var hoy = DateTime.Today;
                año = hoy.Year;
                mes = hoy.Month;
            }

            var fechaReferencia = new DateTime(año, mes, 1);
            var diasDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);

            var diasSegmentados = diasDelMes.Chunk(7).ToList();

            for (int i = 0; i < diasSegmentados.Count(); i++)
            {
                var semana = i + 1;
                var fechaInicio = new DateTime(año, mes, diasSegmentados[i].First());
                var fechaFin = new DateTime(año, mes, diasSegmentados[i].Last());
                var gruposSemana = agrupado.FirstOrDefault(x => x.Semana == semana);

                if(gruposSemana == null)
                {
                    agrupado.Add(new ResultadoObtenerPorSemana()
                    {
                        Semana = semana,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin

                    });
                }
                else
                {
                    gruposSemana.FechaInicio = fechaInicio;
                    gruposSemana.FechaFin = fechaFin;
                }
            }

            agrupado = agrupado.OrderByDescending(x => x.Semana).ToList();
            var modelo = new ReporteSemanalViewModel();
            modelo.TransaccionesPorSemana = agrupado;
            modelo.FechaReferencia = fechaReferencia;


            return View();
        }

        public async Task<IActionResult> Mensual(int año)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();

            if (año == null)
            {
                año = DateTime.Today.Year;
            }

            var transaccionesPorMes = await repositorioTransacciones.ObtenerPorMes(usuarioId, año);

            var transaccionesAgrupadas = transaccionesPorMes.GroupBy(x => x.Mes).Select(x => new ResultadoObtenerPorMes()
            {
                Mes = x.Key,
                Ingreso = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingresos).Select(x => x.Monto).FirstOrDefault(),
                Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gastos).Select(x => x.Monto).FirstOrDefault()
            }).ToList();

            for (int mes = 0; mes <= 12; mes++)
            {
                var transacciones = transaccionesAgrupadas.FirstOrDefault(x => x.Mes == mes);
                var fechaReferencia = new DateTime(año, mes, 1);

                if(transacciones == null)
                {
                    transaccionesAgrupadas.Add(new ResultadoObtenerPorMes()
                    {
                        Mes = mes,
                        FechaReferencia = fechaReferencia
                    });
                }
                else
                {
                    transacciones.FechaReferencia = fechaReferencia;
                }
            }

            transaccionesAgrupadas = transaccionesAgrupadas.OrderByDescending(x => x.Mes).ToList();
            var modelo = new ReporteMensualViewModel();
            modelo.Año = año;
            modelo.TransaccionesPorMes = transaccionesAgrupadas;
            return View(modelo);
        }
        public IActionResult Calendario()
        {
            return View();
        }

        public async Task<JsonResult> ObtenerTransaccionesCalendario(DateTime start, DateTime end)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(new ParametroObtenerPorUsuario()
            {
                UsuarioId=usuarioId,
                FechaInicio=start,
                FechaFin=end
            });


            var eventosCalendarios = transacciones.Select(transaccion => new EventoCalendario()
            {
                Title = transaccion.Monto.ToString("N"),
                Start = transaccion.FechaTransaccion.ToString("yyyy-MMMM-dd"),
                End = transaccion.FechaTransaccion.ToString("yyyy-MMMM-dd"),
                Color = (transaccion.TipoOperacionId == TipoOperacion.Gastos) ? "Red" : null
            }); ;

            return Json(eventosCalendarios);
        }

        public async Task<JsonResult> ObtenerTransaccionesPorFecha(DateTime fecha)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(new ParametroObtenerPorUsuario()
            {
                UsuarioId = usuarioId,
                FechaInicio = fecha,
                FechaFin = fecha
            });

            return Json(transacciones);


        }


        public IActionResult ExcelReporte()
        {




            return View();
        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelPorMes(int mes, int año)
        {
            var fechaInicio = new DateTime(año, mes, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
            var usuarioId = servicioUsuario.ObtenerUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(new ParametroObtenerPorUsuario()
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            });

            var nombreArchivo = $"Manejo Presupuesto - {fechaInicio.ToString("MMM yyy")}.xlsx";

            return GenerarExcel(nombreArchivo, transacciones);

        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelPorAño(int year)
        {
            var fechaInicio = new DateTime(year, 1, 1);
            var fechaFin = fechaInicio.AddYears(1).AddDays(-1);
            var usuarioId = servicioUsuario.ObtenerUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(new ParametroObtenerPorUsuario()
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            });

            var nombreArchivo = $"Manejo de Presupuesto - {fechaInicio.ToString("yyyy")}.xlsx";
            return GenerarExcel(nombreArchivo, transacciones);

        }

        [HttpGet]
        public async Task<FileResult> ExportarExcelTodo()
        {
            var fechaInicio = DateTime.Today.AddYears(-100);
            var fechaFin = DateTime.Today.AddYears(1000);
            var usuarioId = servicioUsuario.ObtenerUsuarioId();

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(new ParametroObtenerPorUsuario()
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            });

            var nombreArchivo = $"Manejo de Presupuesto - {DateTime.Today.ToString("dd-MM-yy")}.xlsx";

            return GenerarExcel(nombreArchivo, transacciones);


        }


        private FileResult GenerarExcel(string nombreArchivo, IEnumerable<Transaccion> transacciones)
        {
            DataTable dataTable = new DataTable("Transacciones");
            dataTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Fecha"),
                new DataColumn("Cuenta"),
                new DataColumn("Categoria"),
                new DataColumn("Nota"),
                new DataColumn("Monto"),
                new DataColumn("Ingreso/Gastos"),

            });  

            foreach(var transaccion in transacciones)
            {
                dataTable.Rows.Add(
                    transaccion.FechaTransaccion,
                    transaccion.Cuenta,
                    transaccion.Categoria,
                    transaccion.Nota,
                    transaccion.Monto,
                    transaccion.TipoOperacionId);
            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dataTable);

                using(MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                        nombreArchivo);
                }
            }

        }
    }
}

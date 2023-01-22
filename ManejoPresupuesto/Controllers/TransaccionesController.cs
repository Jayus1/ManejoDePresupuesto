using AutoMapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public TransaccionesController(IRepositorioTransacciones repositorioTransacciones, 
            IRepositorioCuentas repositorioCuentas, IServicioUsuario servicioUsuario, 
            IRepositorioCategorias repositorioCategorias, IMapper mapper)
        {
            this.repositorioTransacciones = repositorioTransacciones;
            this.repositorioCuentas = repositorioCuentas;
            this.servicioUsuario = servicioUsuario;
            this.repositorioCategorias = repositorioCategorias;
            this.mapper = mapper;
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
            DateTime fechaInicio, fechaFin;

            if (mes <= 0 || mes > 12 || año <= 1900)
            {
                var hoy = DateTime.Today;
                fechaInicio = new DateTime(hoy.Year, hoy.Month, 1);
            }
            else
            {
                fechaInicio = new DateTime(año, mes, 1);
            }
            fechaFin = fechaInicio.AddMonths(1).AddDays(-1);

            var parametro = new ParametroObtenerPorUsuario()
            {
               UsuarioId=usuarioId,
               FechaInicio=fechaInicio,
               FechaFin=fechaFin
            };

            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(parametro);

            var modelo = new ReporteTransaccionesDetalladas();
            var transaccionesPorFecha = transacciones.OrderByDescending(x => x.FechaTransaccion)
                .GroupBy(x => x.FechaTransaccion).Select(grupo => new ReporteTransaccionesDetalladas.TransaccionesPorFechas()
                {
                    FechaTransaccion = grupo.Key,
                    Transacciones = grupo.AsEnumerable()
                });
            modelo.TransaccioneAgrupadas = transaccionesPorFecha;
            modelo.FechaInicio = fechaInicio;
            modelo.FechaFin = fechaFin;

            ViewBag.mesAnterior = fechaInicio.AddMonths(-1);
            ViewBag.añoAnterior = fechaInicio.AddYears(-1);

            ViewBag.mesPosterior = fechaInicio.AddMonths(+1);
            ViewBag.añoPosterior = fechaInicio.AddYears(+1);
            ViewBag.urlRetorno = HttpContext.Request.Path + HttpContext.Request.QueryString;


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
    }
}

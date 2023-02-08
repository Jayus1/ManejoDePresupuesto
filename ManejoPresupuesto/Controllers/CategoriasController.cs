using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly IRepositorioCategorias repositorioCategorias;
        private readonly IServicioUsuario servicioUsuario;

        public CategoriasController(IRepositorioCategorias repositorioCategorias,IServicioUsuario servicioUsuario)
        {
            this.repositorioCategorias = repositorioCategorias;
            this.servicioUsuario = servicioUsuario;
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Crear(Categoria categoria)
        {
            if(!ModelState.IsValid)
            {
                return View(categoria);
            }

            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            categoria.UsuarioId = usuarioId;
            await repositorioCategorias.Crear(categoria);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index(PaginacionViewModel paginacion)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var categoria = await repositorioCategorias.Obtener(usuarioId, paginacion);
            var totalCategorias = await repositorioCategorias.Contar(usuarioId);

            var respuestaVm = new PaginacionRespuesta <Categoria>
            {
                Elementos = categoria,
                Pagina = paginacion.Pagina,
                RecordsPorPagina = paginacion.RecordsPorPagina,
                CantidadTotalDeRecords = totalCategorias,
                BaseURL = "/Categorias"
            };

            return View(respuestaVm);
        }
        
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var categoria = await repositorioCategorias.ObtenerPorId(id, usuarioId);

            if(categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Categoria categoriaEditar)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var categoria = repositorioCategorias.ObtenerPorId(categoriaEditar.Id, usuarioId);

            if (!ModelState.IsValid)
            {
                return View(categoriaEditar);
            }

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            categoriaEditar.UsuarioId = usuarioId;

            await repositorioCategorias.Actualizar(categoriaEditar);
            return RedirectToAction("Index", "Categorias");

        }

        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var categoria = await repositorioCategorias.ObtenerPorId(id, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            return View(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> BorrarCategoria(int id)
        {
            var usuarioId = servicioUsuario.ObtenerUsuarioId();
            var categoria = repositorioCategorias.ObtenerPorId(id, usuarioId);

            if (categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            await repositorioCategorias.Borrar(id);
            return RedirectToAction("Index", "Categorias");
        }

    }
}
;
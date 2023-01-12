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


            return View();
        }
    }
}
;
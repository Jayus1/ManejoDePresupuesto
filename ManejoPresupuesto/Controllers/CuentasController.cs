using Microsoft.AspNetCore.Mvc;

namespace ManejoPresupuesto.Controllers
{
    public class CuentasController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

using CatalogoWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogoWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly CatalogoWebContext _contex;
        public HomeController(CatalogoWebContext context)
        { 
            _contex = context;
            
        }
        public async Task<IActionResult> Index()
        {
            var productos = await _contex.Producto.ToListAsync();
            return View(productos);
        }
    }
}

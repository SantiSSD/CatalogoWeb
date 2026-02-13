using CatalogoWeb.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CatalogoWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly CatalogoWebContext _context;
        public HomeController(CatalogoWebContext context)
        { 
            _context = context;
            
        }
        public async Task<IActionResult> Index(int? CategoriaId, string? SearchString)
        {

            if (_context.Producto == null)
            {
                return Problem("No se encontraron Productos");

            }
            var categorias = await _context.Categoria.ToListAsync();
            ViewBag.Categorias = new SelectList(categorias,
                "Id",
                "Nombre",
                CategoriaId);
            var productos = _context.Producto.AsQueryable();

            if (CategoriaId.HasValue)
            {
                productos = productos.Where(p => p.CategoriaId == CategoriaId.Value);

            }
            if (!string.IsNullOrEmpty(SearchString))
            {
                productos = productos.Where(p => p.Nombre.ToUpper().Contains(SearchString.ToUpper()));


            }
            return View(await productos.Include(p => p.Categoria).ToListAsync());
        }
        public async Task<IActionResult> PruebaCarousel()
        {
            var productos = await _context.Producto.ToListAsync();
            return View(productos);
        }
    }
}

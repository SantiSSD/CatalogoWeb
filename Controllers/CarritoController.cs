using CatalogoWeb.Data;
using CatalogoWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel;
using System.Text.Json;

namespace CatalogoWeb.Controllers
{
    public class CarritoController : Controller
    {
        private readonly CatalogoWebContext _context;
        private const string CarritoSessionKey = "Carrito";


        public CarritoController(CatalogoWebContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var carrito = ObtenerCarrito();
            return View(carrito);
        }
        public IActionResult Agregar(int id)
        {
            var carrito = ObtenerCarrito();
            var producto = _context.Producto.FirstOrDefault(p => p.Id == id);
            var item = carrito.FirstOrDefault(c => c.ProductoId == id);

            if (item == null)
            {
                carrito.Add(new CarritoItem
                {
                    ProductoId = producto.Id,
                    NombreProducto = producto.Nombre,
                    PrecioUnitario = producto.Precio,
                    Cantidad = 1
                });
            }
            else
            {
                item.Cantidad++;
            }
            GuardarCarrito(carrito);
            return RedirectToAction("Index");
        }



        private List<CarritoItem> ObtenerCarrito()
        {
            var carritoJson = HttpContext.Session.GetString(CarritoSessionKey);
            if (string.IsNullOrEmpty(carritoJson))
                return new List<CarritoItem>();

            return JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson);
        }

        private void GuardarCarrito(List<CarritoItem> carrito)
        {
            var carritoJson = JsonSerializer.Serialize(carrito);
            HttpContext.Session.SetString(CarritoSessionKey, carritoJson);

        }
    }
}

using CatalogoWeb.Data;
using CatalogoWeb.Interfaces;
using CatalogoWeb.Models;
using CatalogoWeb.Models.Dtos;
using CatalogoWeb.Service;
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
        private readonly IPedidoService _pedidoService;

        public CarritoController(CatalogoWebContext context, IPedidoService pedidoService)
        {
            _context = context;
            _pedidoService = pedidoService;
        }



        [HttpPost]
        public async Task<IActionResult> FinalizarCompra() 
        {
            var carrito = ObtenerCarrito();

            if(carrito == null || !carrito.Any())
                return RedirectToAction("Index");
            var dto = new CrearPedidoDto
            {
                Items = carrito.Select(c => new ItemPedidoDto
                {
                    ProductoId = c.ProductoId,
                    Cantidad = c.Cantidad,

                }).ToList()
            };

            var pedidoId = await _pedidoService.CrearPedidoAsync(dto);
            HttpContext.Session.Remove(CarritoSessionKey);

            return RedirectToAction("CompraExitosa", "Pedido", new {id = pedidoId });
        
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
        public IActionResult Restar(int id)
        {
            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(c => c.ProductoId == id);

            if (item != null)
            {
                if (item.Cantidad > 1)
                {

                    item.Cantidad--;
                }

                else
                {
                    carrito.Remove(item);
                }
            }
            GuardarCarrito(carrito);
            return RedirectToAction("Index");
        }
        public IActionResult Eliminar(int id) 
        {

            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(c => c.ProductoId == id);
            carrito.Remove(item);
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

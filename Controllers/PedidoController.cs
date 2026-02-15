using CatalogoWeb.Data;
using CatalogoWeb.Interfaces;
using CatalogoWeb.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatalogoWeb.Controllers
{
    public class PedidoController : Controller
    {
        private readonly IPedidoService _pedidoService;

        public PedidoController(IPedidoService pedidoService)
        {
            _pedidoService = pedidoService;
        }



        public async Task<IActionResult> Index()
        {
            var pedidos = await _pedidoService.ObtenerTodos();
            return View(pedidos);

        }

        public async Task<IActionResult> Details(int id)
        {
          var pedido = await _pedidoService.ObtenerPorId(id);
            if (pedido == null) 
            {
                return NotFound();
            }
            return View(pedido);
        }

        public async Task<IActionResult> CompraExitosa(int id)
        {
            var pedido = await _pedidoService.ObtenerPorId(id);

            if (pedido == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(pedido);
        }

    }
}

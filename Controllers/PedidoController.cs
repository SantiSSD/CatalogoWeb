using CatalogoWeb.Data;
using CatalogoWeb.Interfaces;
using CatalogoWeb.Models;
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

        public async Task<IActionResult> CompraFallida(int id, string? error)
        {
            var pedido = await _pedidoService.ObtenerPorId(id);
            if (pedido == null)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = error ?? "El pago fue rechazado";
            return View(pedido);
        }

        public async Task<IActionResult> ReintentarPagoForm(int id)
        {
            var pedido = await _pedidoService.ObtenerPorId(id);
            if (pedido == null)
            {
                return NotFound();
            }
            return View(pedido);
        }

        [HttpPost]
        public async Task<IActionResult> ReintentarPago(int pedidoId, string MetodoPago,
            string NumeroTarjeta, string MesExpiracion, string AnoExpiracion,
            string CVV, string NombreTitular)
        {
            var datosPago = new DatosPagoDto
            {
                MetodoPago = MetodoPago,
                NumeroTarjeta = NumeroTarjeta ?? string.Empty,
                MesExpiracion = MesExpiracion ?? string.Empty,
                AnoExpiracion = AnoExpiracion ?? string.Empty,
                CVV = CVV ?? string.Empty,
                NombreTitular = NombreTitular ?? string.Empty
            };

            var resultado = await _pedidoService.ProcesarPagoAsync(pedidoId, datosPago);

            if (resultado.Exito)
            {
                return RedirectToAction("CompraExitosa", new { id = pedidoId });
            }
            else
            {
                return RedirectToAction("CompraFallida", new { id = pedidoId, error = resultado.MensajeError });
            }
        }

        public async Task<IActionResult> Pagar(int id)
        {
            var pedido = await _pedidoService.PagarPedido(id);
            return RedirectToAction("Details", new { id });
        }

        public async Task<IActionResult> MarcarEnPreparacion(int id)
        {
            await _pedidoService.MarcarEnPreparacion(id);
            return RedirectToAction("Details", new { id });
        }

        public async Task<IActionResult> Enviar(int id, string? numeroTracking)
        {
            await _pedidoService.MarcarEnviado(id, numeroTracking ?? string.Empty);
            return RedirectToAction("Details", new { id });
        }

        public async Task<IActionResult> MarcarEntregado(int id)
        {
            await _pedidoService.MarcarEntregado(id);
            return RedirectToAction("Details", new { id });
        }

        public async Task<IActionResult> MarcarCompletado(int id)
        {
            await _pedidoService.MarcarCompletado(id);
            return RedirectToAction("Details", new { id });
        }

        public async Task<IActionResult> Cancelar(int id)
        {
            await _pedidoService.CancelarPedido(id);
            return RedirectToAction("Details", new { id });
        }
    }
}

using CatalogoWeb.Data;
using CatalogoWeb.Interfaces;
using CatalogoWeb.Models;
using CatalogoWeb.Models.Dtos;
using Microsoft.EntityFrameworkCore;

namespace CatalogoWeb.Service
{
    public class PedidoService : IPedidoService
    {
        private readonly CatalogoWebContext _context;

        public PedidoService(CatalogoWebContext context)
        {
            _context = context;
        }

        public async Task<List<Pedido>> ObtenerTodos()
        {
            return await _context.Pedido
                .Include(p => p.Detalles)
                .Include(p => p.Usuario)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();
        }

        public async Task<Pedido?> ObtenerPorId(int id)
        {
            return await _context.Pedido
                .Include(p => p.Detalles)
                .Include(p => p.Usuario)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<int> CrearPedidoAsync(CrearPedidoDto dto)
        {
            var pedido = new Pedido
            {
                Fecha = DateTime.Now,
                Estado = EstadoPedido.Pendiente,
                Detalles = new List<PedidoDetalle>(),
                Total = 0,
                Nombre = dto.Nombre ?? string.Empty,
                Direccion = dto.Direccion ?? string.Empty,
                Telefono = dto.Telefono ?? string.Empty,
                Ciudad = dto.Ciudad ?? string.Empty,
                CodigoPostal = dto.CodigoPostal ?? string.Empty,
                UsuarioId = dto.UsuarioId
            };

            foreach (var item in dto.Items)
            {
                var producto = await _context.Producto.FirstOrDefaultAsync(p => p.Id == item.ProductoId);
                if (producto == null)
                {
                    throw new Exception("Producto no encontrado");
                }
                var detalle = new PedidoDetalle
                {
                    NombreProducto = producto.Nombre,
                    PrecioUnitario = producto.Precio,
                    Cantidad = item.Cantidad,
                    DescripcionProducto = producto.Descripcion,
                    SubTotal = producto.Precio * item.Cantidad
                };

                pedido.Total += detalle.SubTotal;
                pedido.Detalles.Add(detalle);
            }

            _context.Pedido.Add(pedido);
            await _context.SaveChangesAsync();

            return pedido.Id;
        }

        public async Task<ResultadoPagoDto> ProcesarPagoAsync(int pedidoId, DatosPagoDto datosPago)
        {
            var pedido = await _context.Pedido.FindAsync(pedidoId);
            if (pedido == null)
                return new ResultadoPagoDto { Exito = false, MensajeError = "Pedido no encontrado" };
            if (pedido.Estado != EstadoPedido.Pendiente && pedido.Estado != EstadoPedido.PagoRechazado)
                return new ResultadoPagoDto { Exito = false, MensajeError = "El pedido no puede ser pagado en su estado actual" };

            var resultadoGateway = await ProcesarConGatewayAsync(datosPago, pedido.Total);

            if (resultadoGateway.Exito)
            {
                pedido.Estado = EstadoPedido.Pagado;
                pedido.FechaPago = DateTime.Now;
                pedido.MetodoPago = datosPago.MetodoPago;
                pedido.MensajeErrorPago = null;
                await _context.SaveChangesAsync();

                await NotificarVendedor(pedidoId);
                await EnviarEmailConfirmacion(pedidoId);

                return new ResultadoPagoDto { Exito = true, PedidoId = pedidoId };
            }
            else
            {
                pedido.Estado = EstadoPedido.PagoRechazado;
                pedido.MensajeErrorPago = resultadoGateway.MensajeError;
                await _context.SaveChangesAsync();

                return new ResultadoPagoDto { Exito = false, MensajeError = resultadoGateway.MensajeError, PedidoId = pedidoId };
            }
        }

        private async Task<ResultadoPagoDto> ProcesarConGatewayAsync(DatosPagoDto datosPago, decimal monto)
        {
            await Task.Delay(100);
            return new ResultadoPagoDto { Exito = true };
        }

        public async Task<int> PagarPedido(int pedidoId)
        {
            var pedido = await _context.Pedido.FindAsync(pedidoId);
            if (pedido == null)
                throw new Exception("Pedido no encontrado");
            if (pedido.Estado != EstadoPedido.Pendiente)
                throw new Exception("Solo se pueden pagar pedidos pendientes");

            pedido.Estado = EstadoPedido.Pagado;
            await _context.SaveChangesAsync();
            return pedido.Id;
        }

        public async Task<int> RechazarPago(int pedidoId, string mensajeError)
        {
            var pedido = await _context.Pedido.FindAsync(pedidoId);
            if (pedido == null)
                throw new Exception("Pedido no encontrado");

            pedido.Estado = EstadoPedido.PagoRechazado;
            pedido.MensajeErrorPago = mensajeError;
            await _context.SaveChangesAsync();
            return pedido.Id;
        }

        public async Task<int> MarcarEnPreparacion(int pedidoId)
        {
            var pedido = await _context.Pedido.FindAsync(pedidoId);
            if (pedido == null)
                throw new Exception("Pedido no encontrado");
            if (pedido.Estado != EstadoPedido.Pagado)
                throw new Exception("Solo se pueden preparar pedidos pagados");

            pedido.Estado = EstadoPedido.EnPreparacion;
            await _context.SaveChangesAsync();
            return pedido.Id;
        }

        public async Task<int> EnviarPedido(int pedidoId)
        {
            return await MarcarEnviado(pedidoId, string.Empty);
        }

        public async Task<int> MarcarEnviado(int pedidoId, string numeroTracking)
        {
            var pedido = await _context.Pedido.FindAsync(pedidoId);
            if (pedido == null)
                throw new Exception("Pedido no encontrado");
            if (pedido.Estado != EstadoPedido.Pagado && pedido.Estado != EstadoPedido.EnPreparacion)
                throw new Exception("Solo se pueden enviar pedidos pagados o en preparación");

            pedido.Estado = EstadoPedido.Enviado;
            pedido.NumeroTracking = numeroTracking;
            pedido.FechaEnvio = DateTime.Now;
            await _context.SaveChangesAsync();

            await EnviarEmailEnvio(pedidoId);

            return pedido.Id;
        }

        public async Task<int> MarcarEntregado(int pedidoId)
        {
            var pedido = await _context.Pedido.FindAsync(pedidoId);
            if (pedido == null)
                throw new Exception("Pedido no encontrado");
            if (pedido.Estado != EstadoPedido.Enviado)
                throw new Exception("Solo se pueden marcar como entregados pedidos enviados");

            pedido.Estado = EstadoPedido.Entregado;
            await _context.SaveChangesAsync();

            await EnviarEmailEntrega(pedidoId);

            return pedido.Id;
        }

        public async Task<int> MarcarCompletado(int pedidoId)
        {
            var pedido = await _context.Pedido.FindAsync(pedidoId);
            if (pedido == null)
                throw new Exception("Pedido no encontrado");
            if (pedido.Estado != EstadoPedido.Entregado)
                throw new Exception("Solo se pueden completar pedidos entregados");

            pedido.Estado = EstadoPedido.Completado;
            await _context.SaveChangesAsync();
            return pedido.Id;
        }

        public async Task<int> CancelarPedido(int pedidoId)
        {
            var pedido = await _context.Pedido.FindAsync(pedidoId);
            if (pedido == null)
                throw new Exception("Pedido no encontrado");

            if (pedido.Estado == EstadoPedido.Enviado || pedido.Estado == EstadoPedido.Entregado)
                throw new Exception("No se puede cancelar un pedido enviado o entregado");

            pedido.Estado = EstadoPedido.Cancelado;
            await _context.SaveChangesAsync();
            return pedido.Id;
        }

        public async Task<int> NotificarVendedor(int pedidoId)
        {
            var pedido = await _context.Pedido.FindAsync(pedidoId);
            if (pedido == null)
                throw new Exception("Pedido no encontrado");

            pedido.FechaNotificacionVendedor = DateTime.Now;
            await _context.SaveChangesAsync();
            return pedidoId;
        }

        public async Task<int> EnviarEmailConfirmacion(int pedidoId)
        {
            var pedido = await _context.Pedido.FindAsync(pedidoId);
            if (pedido == null)
                throw new Exception("Pedido no encontrado");

            pedido.FechaEmailConfirmacion = DateTime.Now;
            await _context.SaveChangesAsync();
            return pedidoId;
        }

        public async Task<int> EnviarEmailEnvio(int pedidoId)
        {
            var pedido = await _context.Pedido.FindAsync(pedidoId);
            if (pedido == null)
                throw new Exception("Pedido no encontrado");

            pedido.FechaEmailEnvio = DateTime.Now;
            await _context.SaveChangesAsync();
            return pedidoId;
        }

        public async Task<int> EnviarEmailEntrega(int pedidoId)
        {
            var pedido = await _context.Pedido.FindAsync(pedidoId);
            if (pedido == null)
                throw new Exception("Pedido no encontrado");

            pedido.FechaEmailEntrega = DateTime.Now;
            await _context.SaveChangesAsync();
            return pedidoId;
        }

        public async Task<int> EnviarEmailValoracion(int pedidoId)
        {
            var pedido = await _context.Pedido.FindAsync(pedidoId);
            if (pedido == null)
                throw new Exception("Pedido no encontrado");

            pedido.FechaEmailValoracion = DateTime.Now;
            await _context.SaveChangesAsync();
            return pedidoId;
        }
    }
}

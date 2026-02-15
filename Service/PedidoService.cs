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
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();
        }

        public async Task<Pedido?> ObtenerPorId(int id)
        {
            return await _context.Pedido
                .Include(p => p.Detalles)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<int> CrearPedidoAsync(CrearPedidoDto dto) 
        {
            var pedido = new Pedido
            {
                Fecha = DateTime.Now,
                Detalles = new List<PedidoDetalle>(),
                Total = 0
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
    }
}

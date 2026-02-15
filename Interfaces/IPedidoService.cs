using CatalogoWeb.Models;
using CatalogoWeb.Models.Dtos;

namespace CatalogoWeb.Interfaces
{
    public interface IPedidoService
    {
        Task<int> CrearPedidoAsync(CrearPedidoDto dto);
        Task<List<Pedido>> ObtenerTodos();
        Task<Pedido?> ObtenerPorId(int id);
    }
}

using CatalogoWeb.Models;
using CatalogoWeb.Models.Dtos;

namespace CatalogoWeb.Interfaces
{
    public interface IPedidoService
    {
        Task<int> CrearPedidoAsync(CrearPedidoDto dto);
        Task<int> CancelarPedido(int pedidoId);
        Task<int> PagarPedido(int pedidoId);
        Task<int> EnviarPedido(int pedidoId);
        Task<List<Pedido>> ObtenerTodos();
        Task<Pedido?> ObtenerPorId(int id);

        Task<ResultadoPagoDto> ProcesarPagoAsync(int pedidoId, DatosPagoDto datosPago);
        Task<int> MarcarEnPreparacion(int pedidoId);
        Task<int> MarcarEnviado(int pedidoId, string numeroTracking);
        Task<int> MarcarEntregado(int pedidoId);
        Task<int> MarcarCompletado(int pedidoId);
        Task<int> RechazarPago(int pedidoId, string mensajeError);
        Task<int> NotificarVendedor(int pedidoId);
        Task<int> EnviarEmailConfirmacion(int pedidoId);
        Task<int> EnviarEmailEnvio(int pedidoId);
        Task<int> EnviarEmailEntrega(int pedidoId);
        Task<int> EnviarEmailValoracion(int pedidoId);
    }

    public class ResultadoPagoDto
    {
        public bool Exito { get; set; }
        public string? MensajeError { get; set; }
        public int? PedidoId { get; set; }
    }

    public class DatosPagoDto
    {
        public string MetodoPago { get; set; } = string.Empty;
        public string NumeroTarjeta { get; set; } = string.Empty;
        public string MesExpiracion { get; set; } = string.Empty;
        public string AnoExpiracion { get; set; } = string.Empty;
        public string CVV { get; set; } = string.Empty;
        public string NombreTitular { get; set; } = string.Empty;
    }
}

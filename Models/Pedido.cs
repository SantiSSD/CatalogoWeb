namespace CatalogoWeb.Models
{
    public class Pedido
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public List<PedidoDetalle> Detalles { get; set; }

        public EstadoPedido Estado { get; set; }

        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public string Nombre { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string CodigoPostal { get; set; } = string.Empty;

        public string MetodoPago { get; set; } = string.Empty;
        public DateTime? FechaPago { get; set; }
        public string? MensajeErrorPago { get; set; }

        public string? NumeroTracking { get; set; }
        public DateTime? FechaEnvio { get; set; }

        public DateTime? FechaNotificacionVendedor { get; set; }
        public DateTime? FechaEmailConfirmacion { get; set; }
        public DateTime? FechaEmailEnvio { get; set; }
        public DateTime? FechaEmailEntrega { get; set; }
        public DateTime? FechaEmailValoracion { get; set; }
    }
}

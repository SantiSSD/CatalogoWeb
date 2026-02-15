namespace CatalogoWeb.Models
{
    public class PedidoDetalle
    {
        public int Id { get; set; }
        public int PedidoId { get; set; }
        public Pedido Pedido { get; set; }

        public string NombreProducto { get; set; }
        public string DescripcionProducto { get; set; }

        public decimal PrecioUnitario { get; set; }

        public int Cantidad { get; set; }

        public decimal SubTotal { get; set; }
    }
}

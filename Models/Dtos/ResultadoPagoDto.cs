namespace CatalogoWeb.Models.Dtos
{
    public class ResultadoPagoDto
    {
        public bool Exito { get; set; }
        public string? MensajeError { get; set; }
        public int? PedidoId { get; set; }
    }
}

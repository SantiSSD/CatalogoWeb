namespace CatalogoWeb.Models.Dtos
{
    public class CrearPedidoDto
    {
        public List<ItemPedidoDto> Items { get; set; } = new List<ItemPedidoDto>();

        public int? UsuarioId { get; set; }
        public string? Nombre { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Ciudad { get; set; }
        public string? CodigoPostal { get; set; }
    }
}

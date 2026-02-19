namespace CatalogoWeb.Models.Dtos
{
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

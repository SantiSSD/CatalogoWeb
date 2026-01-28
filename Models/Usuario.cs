namespace CatalogoWeb.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;


        public string PasswordHash { get; set; } 

        public int RolId { get; set; }
        public Rol Rol { get; set; } = null!;

        public DateTime FechaRegistro { get; set; }
    }
}

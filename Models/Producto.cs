using System.ComponentModel.DataAnnotations.Schema;

namespace CatalogoWeb.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }= string.Empty;
        public string Descripcion { get; set; }= string.Empty;
        
        [Column(TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string ImagenUrl { get; set; }= string.Empty;

        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; } 

    }
}

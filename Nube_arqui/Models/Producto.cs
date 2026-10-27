using System.ComponentModel.DataAnnotations;
namespace Nube_arqui.Models;

public record Producto(int Id, string Nombre, string? Descripcion, decimal Precio, int Stock);

public class ProductoRequest
{
    [Required, StringLength(100)]
    public string Nombre { get; set; } = string.Empty;
    [StringLength(255)]
    public string? Descripcion { get; set; }
    [Required, Range(typeof(decimal), "-99999999.99", "99999999.99")]
    public decimal? Precio { get; set; }
    [Required]
    public int? Stock { get; set; }
}

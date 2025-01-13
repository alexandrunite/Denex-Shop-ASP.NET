using static ProductsApp.Models.ProductCarts;
using System.ComponentModel.DataAnnotations;

namespace ProductsApp.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<ProductCart>? ProductCarts { get; set; }
    }
}

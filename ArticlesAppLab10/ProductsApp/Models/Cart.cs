using static ProductsApp.Models.ProductCarts;
using System.ComponentModel.DataAnnotations;

namespace ProductsApp.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        // Relația one-to-one cu ApplicationUser
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        // Relația many-to-many dintre Product și Cart
        public virtual ICollection<ProductCart>? ProductCarts { get; set; }
    }
}

using static ProductsApp.Models.ProductCarts;
using System.ComponentModel.DataAnnotations;

namespace ProductsApp.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele colecției este obligatoriu")]
        public string Name { get; set; }

        // O colecție este creată de către un user
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        // Relația many-to-many dintre Product și Cart
        public virtual ICollection<ProductCart>? ProductCarts { get; set; }
    }
}

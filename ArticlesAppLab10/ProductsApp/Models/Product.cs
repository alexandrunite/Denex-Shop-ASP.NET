// Models/Product.cs
using ProductsApp.Validations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static ProductsApp.Models.ProductCarts;

namespace ProductsApp.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Titlul este obligatoriu")]
        [StringLength(100, ErrorMessage = "Titlul nu poate avea mai mult de 100 de caractere")]
        [MinLength(5, ErrorMessage = "Titlul trebuie să aibă mai mult de 5 caractere")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Conținutul produsului este obligatoriu")]
        public string Content { get; set; }

        public DateTime Date { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; } // Eliminăm [Required]

        public string? UserId { get; set; }

        public virtual Category? Category { get; set; }

        public virtual ApplicationUser? User { get; set; }

        public virtual ICollection<Review>? Reviews { get; set; }

        public virtual ICollection<ProductCart>? ProductCarts { get; set; }

        [NotMapped]
        public IEnumerable<SelectListItem>? Categ { get; set; }

        // Noi câmpuri
        [Required(ErrorMessage = "Prețul este obligatoriu")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Prețul trebuie să fie mai mare decât zero")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stocul este obligatoriu")]
        [Range(0, int.MaxValue, ErrorMessage = "Stocul trebuie să fie zero sau mai mare")]
        public int Stock { get; set; }

        [Range(1, 5, ErrorMessage = "Ratingul trebuie să fie între 1 și 5")]
        public double? Rating { get; set; }

        public bool IsApproved { get; set; } = false;

        public string? ImageUrl { get; set; }

        // Metodă pentru calcularea ratingului mediu
        public void CalculateRating()
        {
            if (Reviews != null && Reviews.Any(r => r.Rating.HasValue))
            {
                Rating = Reviews.Where(r => r.Rating.HasValue).Average(r => r.Rating.Value);
            }
            else
            {
                Rating = null;
            }
        }
        public virtual ICollection<ProductRequest>? ProductRequests { get; set; }

    }
}

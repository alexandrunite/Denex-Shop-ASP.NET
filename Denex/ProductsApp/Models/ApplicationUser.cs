using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductsApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Un user poate posta mai multe comentarii
        public virtual ICollection<Review>? Reviews { get; set; }

        // Un user poate posta mai multe produse
        public virtual ICollection<Product>? Products { get; set; }

        // Relația one-to-one cu Cart
        public virtual Cart? Cart { get; set; }

        // Atribute suplimentare adăugate pentru user
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        // Variabilă în care vom reține rolurile existente în baza de date
        // pentru popularea unui dropdown list
        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }
    }
}

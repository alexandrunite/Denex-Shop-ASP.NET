using System.ComponentModel.DataAnnotations;

namespace ProductsApp.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Numele categoriei este obligatoriu")]
        public string CategoryName { get; set; }

        // proprietatea virtuala - dintr-o categorie fac parte mai multe produse
        public virtual ICollection<Product>? Products { get; set; }
    }

}

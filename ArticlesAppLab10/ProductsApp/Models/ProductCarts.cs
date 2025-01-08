using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductsApp.Models
{
    public class ProductCarts
    {
        public class ProductCart
        {
            [Key]
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
            public int Id { get; set; }

            [Required]
            public int ProductId { get; set; }

            [Required]
            public int CartId { get; set; }

            public virtual Product? Product { get; set; }
            public virtual Cart? Cart { get; set; }

            public DateTime CartDate { get; set; }

            [Range(1, int.MaxValue, ErrorMessage = "Cantitatea trebuie să fie mai mare decât zero")]
            public int Quantity { get; set; }
        }
    }
}

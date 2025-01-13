using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProductsApp.Models.ViewModels
{
    public class ProductEditViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Titlul produsului este obligatoriu.")]
        [StringLength(100, ErrorMessage = "Titlul produsului nu poate depăși 100 de caractere.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Conținutul produsului este obligatoriu.")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Selectați o categorie.")]
        [Display(Name = "Categorie")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Prețul este obligatoriu.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Prețul trebuie să fie mai mare decât zero.")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stocul este obligatoriu.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stocul trebuie să fie zero sau mai mare.")]
        public int Stock { get; set; }

        [Display(Name = "Imagine")]
        public IFormFile? ImageFile { get; set; }

        public string? ImageUrl { get; set; }

        [BindNever]
        public IEnumerable<SelectListItem> Categories { get; set; }
    }
}

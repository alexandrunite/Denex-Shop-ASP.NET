// Models/ProductRequest.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProductsApp.Models.Enums;

namespace ProductsApp.Models
{
    public class ProductRequest
    {
        [Key]
        public int Id { get; set; }

        public RequestType RequestType { get; set; }
        public RequestStatus Status { get; set; }
        public string CollaboratorId { get; set; }
        public DateTime DateCreated { get; set; }

        // Proprietăți propuse pentru cereri de tip Add și Edit
        public string? ProposedTitle { get; set; }
        public string? ProposedContent { get; set; } // Marcat ca nullable
        public decimal? ProposedPrice { get; set; }
        public int? ProposedStock { get; set; }
        public string? ProposedImageUrl { get; set; } // Marcat ca nullable
        public int? ProposedCategoryId { get; set; }

        // Relații
        [ForeignKey("CollaboratorId")]
        public ApplicationUser Collaborator { get; set; }

        [ForeignKey("ProposedCategoryId")]
        public Category? ProposedCategory { get; set; }

        // Optional: dacă gestionezi cereri de editare sau ștergere
        public int? ProductId { get; set; }
        public Product? Product { get; set; }
    }
}

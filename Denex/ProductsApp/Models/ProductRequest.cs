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

        public string? ProposedTitle { get; set; }
        public string? ProposedContent { get; set; }
        public decimal? ProposedPrice { get; set; }
        public int? ProposedStock { get; set; }
        public string? ProposedImageUrl { get; set; }
        public int? ProposedCategoryId { get; set; }

        [ForeignKey("CollaboratorId")]
        public ApplicationUser Collaborator { get; set; }

        [ForeignKey("ProposedCategoryId")]
        public Category? ProposedCategory { get; set; }

        public int? ProductId { get; set; }
        public Product? Product { get; set; }
    }
}

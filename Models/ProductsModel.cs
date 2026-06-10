using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MYGROCER.Models
{
    // ═══════════════════════════════════════════════════════════
    // MODEL LAYER — ProductsModel
    // Represents one row in the Products table in the database.
    // This is the DATABASE LAYER of the 3-layer architecture.
    // ═══════════════════════════════════════════════════════════
    public class ProductsModel
    {
        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [MaxLength(100)]
        [Display(Name = "Product Name")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(250)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Price (RM)")]
        [Range(0.01, 99999.99, ErrorMessage = "Price must be greater than 0.")]
        public decimal BasePrice { get; set; }

        [Display(Name = "Stock Quantity")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative.")]
        public int StockQuantity { get; set; } = 0;

        [MaxLength(200)]
        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        // Helper property — not stored in DB
        [NotMapped]
        public bool IsInStock => StockQuantity > 0;
    }
}

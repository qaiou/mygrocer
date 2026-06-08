using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MYGROCER.Models
{
    public class ProductsModel
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Name { get; set; }

        [Required]
        [MaxLength(250)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        [Column(TypeName = "decimal(20,2)")]
        public decimal BasePrice { get; set; }


    }
}

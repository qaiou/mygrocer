using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MYGROCER.Models
{
    public class CartItemModel
    {
        [Key]
        public int CartItemId { get; set; }

        public int CartId { get; set; }
        [ForeignKey(nameof(CartId))]
        public CartModel? Cart { get; set; }

        public int ProductId { get; set; }

        public string? Name { get; set; }

        public decimal PricePerUnit { get; set; }

        public decimal Quantity { get; set; }
    }
}

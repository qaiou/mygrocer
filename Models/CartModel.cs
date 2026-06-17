using System.ComponentModel.DataAnnotations;

namespace MYGROCER.Models
{
    public class CartModel
    {
        [Key]
        public int CartId { get; set; }

        public int CustomerId { get; set; }

        public List<CartItemModel> CartItems { get; set; } = new List<CartItemModel>();

        public decimal TotalPrice { get; set; }
    }
}
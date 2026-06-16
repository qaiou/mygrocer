using System.ComponentModel.DataAnnotations;

namespace MYGROCER.Models
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public string? ProductName { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Quantity { get; set; }
    }
}

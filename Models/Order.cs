using System.ComponentModel.DataAnnotations;

namespace MYGROCER.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public int CustomerId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public decimal TotalAmount { get; set; }

        public string? PaymentMethod { get; set; }

        public string? TransactionId { get; set; }

        public List<OrderItem>? Items { get; set; }
    }
}

namespace MYGROCER.Models
{
    public class CartItemModel
    {
        public int ProductId { get; set; }

        public string? Name { get; set; }

        public decimal PricePerUnit { get; set; }

        public decimal Quantity { get; set; }
    }
}

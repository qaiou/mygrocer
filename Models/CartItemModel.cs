namespace MYGROCER.Models
{
    public class CartItemModel
    {
        public int ProductId { get; set; }

        public string? Name { get; set; }

        public decimal BasePrice { get; set; }

        public decimal Quantity { get; set; }
    }
}

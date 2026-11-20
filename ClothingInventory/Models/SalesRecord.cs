namespace ClothingInventory.Models
{
    public class SaleRecord
    {
        public int SaleId { get; set; }
        public string SoldAt { get; set; } = "";
        public string Item { get; set; } = "";
        public string Size { get; set; } = "";
        public string Color { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}
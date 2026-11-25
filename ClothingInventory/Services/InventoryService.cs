using ClothingInventory.Models;

namespace ClothingInventory.Services
{
    public class InventoryService
    {
        public const int LowStockLevel = 5;
        public const int MaxStock = 10000;

        public bool IsAvailable(Product product) => product.Quantity > 0;

        public string GetStockStatus(Product product)
        {
            if (product.Quantity <= 0) return "Out of stock";
            if (product.Quantity <= LowStockLevel) return $"Low stock ({product.Quantity} left)";
            return $"In stock ({product.Quantity} available)";
        }
    }
}
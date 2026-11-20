using ClothingInventory.Data;
using ClothingInventory.Models;

namespace ClothingInventory.Services
{
    public class SalesService
    {
        private readonly ProductRepository _repo = new ProductRepository();

        public (bool Success, string Message) RecordSale(Product product, int quantity)
        {
            if (quantity <= 0)
                return (false, "Quantity must be at least 1.");

            bool ok = _repo.RecordSale(product.Id, quantity);

            return ok
                ? (true, $"Sale recorded: {quantity} x {product.Name}. Total: {(product.Price * quantity):C}")
                : (false, "Sale failed: not enough stock.");
        }

        public List<SaleRecord> GetSales(DateTime? day) => _repo.GetSales(day);
    }
}
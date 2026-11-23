using ClothingInventory.Data;
using ClothingInventory.Models;

namespace ClothingInventory.Services
{
    public class ProductService
    {
        private readonly ProductRepository _repo;

        public ProductService()
        {
            _repo = new ProductRepository();
            _repo.Initialize();
        }

        public List<Product> SearchByName(string name)
        {
            name = name?.Trim() ?? "";
            if (name.Length == 0)
                return new List<Product>();

            return _repo.SearchByName(name);
        }

        public List<Product> GetAll() => _repo.GetAll();

        public (bool Success, string Message) AddProduct(Product p)
        {
            string? error = Validate(p);
            if (error != null) return (false, error);

            if (_repo.Exists(p.Name, p.Size, p.Color))
                return (false, $"{p.Name} ({p.Size}, {p.Color}) already exists. Select it and use Restock instead.");

            _repo.AddProduct(p);
            return (true, $"Added {p.Name}.");
        }

        public (bool Success, string Message) UpdateProduct(Product p)
        {
            string? error = Validate(p);
            if (error != null) return (false, error);

            if (_repo.Exists(p.Name, p.Size, p.Color, p.Id))
                return (false, "Another item with the same name, size and colour already exists.");

            return _repo.UpdateProduct(p)
                ? (true, $"Updated {p.Name}.")
                : (false, "Item not found.");
        }

        public List<Product> GetLowStock() => _repo.GetLowStock(InventoryService.LowStockLevel);

        public (bool Success, string Message) Restock(int productId, int amount)
        {
            if (amount <= 0) return (false, "Restock amount must be at least 1.");

            return _repo.Restock(productId, amount)
                ? (true, $"Added {amount} to stock.")
                : (false, "Item not found.");
        }

        private static string? Validate(Product p)
        {
            if (string.IsNullOrWhiteSpace(p.Name)) return "Name is required.";
            if (p.Price < 0) return "Price cannot be negative.";
            if (p.Quantity < 0) return "Quantity cannot be negative.";
            return null;
        }
    }
}
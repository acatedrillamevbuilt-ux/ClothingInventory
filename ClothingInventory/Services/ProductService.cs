using ClothingInventory.Data;
using ClothingInventory.Models;

namespace ClothingInventory.Services
{
    public class ProductService
    {
        private readonly ProductRepository _repo = new ProductRepository();

        public ProductService()
        {
            _repo.Initialize();
        }

        public List<Product> SearchByName(string name, bool includeDelisted = false)
        {
            name = name?.Trim() ?? "";
            if (name.Length == 0)
                return new List<Product>();

            return _repo.SearchByName(name, includeDelisted);
        }

        public List<Product> GetAll(bool includeDelisted = false) => _repo.GetAll(includeDelisted);

        public List<Product> GetLowStock() => _repo.GetLowStock(InventoryService.LowStockLevel);

        public (bool Success, string Message) AddProduct(Product p)
        {
            string? error = Validate(p);
            if (error != null) return (false, error);

            if (_repo.Exists(p.Name, p.Size, p.Color))
                return (false, $"{p.Name} ({p.Size}, {p.Color}) already exists. Select it and use Restock instead (turn on Show delisted if you can't see it).");

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

        public (bool Success, string Message) Restock(int productId, int amount)
        {
            if (amount <= 0) return (false, "Restock amount must be at least 1.");
            if (amount > InventoryService.MaxStock)
                return (false, $"You can restock at most {InventoryService.MaxStock:N0} units at a time.");

            return _repo.Restock(productId, amount, InventoryService.MaxStock) switch
            {
                RestockOutcome.Done => (true, $"Added {amount:N0} to stock."),
                RestockOutcome.OverLimit => (false, $"That would take stock above {InventoryService.MaxStock:N0} units."),
                _ => (false, "Item not found.")
            };
        }

        public (bool Success, string Message) Delist(Product p) =>
            _repo.SetActive(p.Id, false)
                ? (true, $"{p.Name} is delisted. Customers can no longer see it.")
                : (false, "Item not found.");

        public (bool Success, string Message) Relist(Product p) =>
            _repo.SetActive(p.Id, true)
                ? (true, $"{p.Name} is listed again.")
                : (false, "Item not found.");

        // Only items that were never sold can be deleted
        public (bool Success, string Message) Delete(Product p)
        {
            if (_repo.HasSales(p.Id))
                return (false, $"{p.Name} has sales history and can't be deleted. Delist it instead.");

            if (!_repo.Delete(p.Id))
                return (false, "Item could not be deleted.");

            ImageStore.Delete(p.ImagePath);
            return (true, $"Deleted {p.Name}.");
        }

        private static string? Validate(Product p)
        {
            if (string.IsNullOrWhiteSpace(p.Name)) return "Name is required.";
            if (string.IsNullOrWhiteSpace(p.Category)) return "Category is required.";
            if (string.IsNullOrWhiteSpace(p.Size)) return "Size is required.";
            if (string.IsNullOrWhiteSpace(p.Color)) return "Color is required.";
            if (string.IsNullOrWhiteSpace(p.Location)) return "Location is required.";

            if (p.Name.Length > 100) return "Name must be 100 characters or fewer.";
            if (p.Category.Length > 50) return "Category must be 50 characters or fewer.";
            if (p.Size.Length > 20) return "Size must be 20 characters or fewer.";
            if (p.Color.Length > 30) return "Color must be 30 characters or fewer.";
            if (p.Location.Length > 50) return "Location must be 50 characters or fewer.";

            if (p.Price <= 0) return "Price must be greater than zero.";
            if (p.Quantity < 0) return "Quantity cannot be negative.";
            if (p.Quantity > InventoryService.MaxStock)
                return $"Stock can't be more than {InventoryService.MaxStock:N0} units.";
            return null;
        }
    }
}
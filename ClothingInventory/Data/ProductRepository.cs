using ClothingInventory.Models;
using Microsoft.Data.Sqlite;

namespace ClothingInventory.Data
{
    public class ProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository()
        {
            string dbPath = Path.Combine(AppContext.BaseDirectory, "clothing.db");
            _connectionString = $"Data Source={dbPath}";
        }

        // Creates the table and adds sample data the first time the app runs
        public void Initialize()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var create = conn.CreateCommand();
            create.CommandText = @"
                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Category TEXT,
                    Size TEXT,
                    Color TEXT,
                    Location TEXT,
                    Price REAL NOT NULL,
                    Quantity INTEGER NOT NULL DEFAULT 0
                );";
            create.ExecuteNonQuery();

            var createSales = conn.CreateCommand();
            createSales.CommandText = @"
                CREATE TABLE IF NOT EXISTS Sales (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            ProductId INTEGER NOT NULL,
                            Quantity INTEGER NOT NULL,
                            UnitPrice REAL NOT NULL,
                            SoldAt TEXT NOT NULL,
                            FOREIGN KEY (ProductId) REFERENCES Products(Id)
                        );";
            createSales.ExecuteNonQuery();

            var count = conn.CreateCommand();
            count.CommandText = "SELECT COUNT(*) FROM Products;";
            long rows = (long)count.ExecuteScalar()!;

            if (rows == 0)
            {
                var seed = conn.CreateCommand();
                seed.CommandText = @"
                    INSERT INTO Products (Name, Category, Size, Color, Location, Price, Quantity) VALUES
                    ('Classic T-Shirt', 'Tops', 'M', 'White', 'Aisle 1', 19.99, 25),
                    ('Classic T-Shirt', 'Tops', 'L', 'Black', 'Aisle 1', 19.99, 15),
                    ('Slim Fit Jeans', 'Bottoms', '32', 'Blue', 'Aisle 2', 59.99, 10),
                    ('Hoodie', 'Outerwear', 'L', 'Grey', 'Aisle 3', 44.50, 8);";
                seed.ExecuteNonQuery();
            }
        }

        // Finds products whose name contains the search text
        public List<Product> SearchByName(string name)
        {
            var results = new List<Product>();

            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Products WHERE Name LIKE @name ESCAPE '\';";
            string safe = name.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
            cmd.Parameters.AddWithValue("@name", "%" + safe + "%");

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new Product
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Category = reader.GetString(reader.GetOrdinal("Category")),
                    Size = reader.GetString(reader.GetOrdinal("Size")),
                    Color = reader.GetString(reader.GetOrdinal("Color")),
                    Location = reader.GetString(reader.GetOrdinal("Location")),
                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity"))
                });
            }

            return results;
        }

        // Reduces stock and records the sale together; returns false if there isn't enough stock
        public bool RecordSale(int productId, int quantity)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var tx = conn.BeginTransaction();

            var update = conn.CreateCommand();
            update.Transaction = tx;
            update.CommandText = @"UPDATE Products
                           SET Quantity = Quantity - @q
                           WHERE Id = @id AND Quantity >= @q;";
            update.Parameters.AddWithValue("@q", quantity);
            update.Parameters.AddWithValue("@id", productId);

            if (update.ExecuteNonQuery() == 0)
            {
                tx.Rollback();
                return false;
            }

            var insert = conn.CreateCommand();
            insert.Transaction = tx;
            insert.CommandText = @"INSERT INTO Sales (ProductId, Quantity, UnitPrice, SoldAt)
                           SELECT Id, @q, Price, @when FROM Products WHERE Id = @id;";
            insert.Parameters.AddWithValue("@q", quantity);
            insert.Parameters.AddWithValue("@id", productId);
            insert.Parameters.AddWithValue("@when", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            insert.ExecuteNonQuery();

            tx.Commit();
            return true;
        }

        public List<Product> GetAll()
        {
            var results = new List<Product>();

            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Products ORDER BY Name, Size;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new Product
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Category = reader.GetString(reader.GetOrdinal("Category")),
                    Size = reader.GetString(reader.GetOrdinal("Size")),
                    Color = reader.GetString(reader.GetOrdinal("Color")),
                    Location = reader.GetString(reader.GetOrdinal("Location")),
                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity"))
                });
            }

            return results;
        }

        public void AddProduct(Product p)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Products (Name, Category, Size, Color, Location, Price, Quantity)
                        VALUES (@name, @cat, @size, @color, @loc, @price, @qty);";
            cmd.Parameters.AddWithValue("@name", p.Name);
            cmd.Parameters.AddWithValue("@cat", p.Category);
            cmd.Parameters.AddWithValue("@size", p.Size);
            cmd.Parameters.AddWithValue("@color", p.Color);
            cmd.Parameters.AddWithValue("@loc", p.Location);
            cmd.Parameters.AddWithValue("@price", p.Price);
            cmd.Parameters.AddWithValue("@qty", p.Quantity);
            cmd.ExecuteNonQuery();
        }

        public bool UpdateProduct(Product p)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Products
                        SET Name=@name, Category=@cat, Size=@size, Color=@color,
                            Location=@loc, Price=@price, Quantity=@qty
                        WHERE Id=@id;";
            cmd.Parameters.AddWithValue("@name", p.Name);
            cmd.Parameters.AddWithValue("@cat", p.Category);
            cmd.Parameters.AddWithValue("@size", p.Size);
            cmd.Parameters.AddWithValue("@color", p.Color);
            cmd.Parameters.AddWithValue("@loc", p.Location);
            cmd.Parameters.AddWithValue("@price", p.Price);
            cmd.Parameters.AddWithValue("@qty", p.Quantity);
            cmd.Parameters.AddWithValue("@id", p.Id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Restock(int productId, int amount)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Products SET Quantity = Quantity + @amt WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@amt", amount);
            cmd.Parameters.AddWithValue("@id", productId);
            return cmd.ExecuteNonQuery() > 0;
        }

        // Returns sales newest first; pass a date to see only that day's sales, or null for all
        public List<SaleRecord> GetSales(DateTime? day)
        {
            var results = new List<SaleRecord>();

            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        SELECT s.Id, s.SoldAt, p.Name, p.Size, p.Color,
               s.Quantity, s.UnitPrice, s.Quantity * s.UnitPrice AS Total
        FROM Sales s
        JOIN Products p ON p.Id = s.ProductId
        WHERE @day IS NULL OR substr(s.SoldAt, 1, 10) = @day
        ORDER BY s.SoldAt DESC;";
            cmd.Parameters.AddWithValue("@day",
                day.HasValue ? day.Value.ToString("yyyy-MM-dd") : DBNull.Value);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new SaleRecord
                {
                    SaleId = reader.GetInt32(reader.GetOrdinal("Id")),
                    SoldAt = reader.GetString(reader.GetOrdinal("SoldAt")),
                    Item = reader.GetString(reader.GetOrdinal("Name")),
                    Size = reader.GetString(reader.GetOrdinal("Size")),
                    Color = reader.GetString(reader.GetOrdinal("Color")),
                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                    UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                    Total = reader.GetDecimal(reader.GetOrdinal("Total"))
                });
            }

            return results;
        }

        // True if another product already has the same name, size and colour
        public bool Exists(string name, string size, string color, int? excludeId = null)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT COUNT(*) FROM Products
                        WHERE Name = @name COLLATE NOCASE
                          AND Size = @size COLLATE NOCASE
                          AND Color = @color COLLATE NOCASE
                          AND (@id IS NULL OR Id <> @id);";
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@size", size);
            cmd.Parameters.AddWithValue("@color", color);
            cmd.Parameters.AddWithValue("@id", (object?)excludeId ?? DBNull.Value);

            return (long)cmd.ExecuteScalar()! > 0;
        }

        // Products at or below the given stock level, lowest first
        public List<Product> GetLowStock(int level)
        {
            var results = new List<Product>();

            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Products WHERE Quantity <= @level ORDER BY Quantity, Name;";
            cmd.Parameters.AddWithValue("@level", level);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new Product
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Category = reader.GetString(reader.GetOrdinal("Category")),
                    Size = reader.GetString(reader.GetOrdinal("Size")),
                    Color = reader.GetString(reader.GetOrdinal("Color")),
                    Location = reader.GetString(reader.GetOrdinal("Location")),
                    Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity"))
                });
            }

            return results;
        }

    }
}
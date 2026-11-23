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

        // ---------- Setup ----------
        public void Initialize()
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            Run(conn, @"CREATE TABLE IF NOT EXISTS Products (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Category TEXT,
                Size TEXT,
                Color TEXT,
                Location TEXT,
                Price REAL NOT NULL,
                Quantity INTEGER NOT NULL DEFAULT 0,
                ImagePath TEXT,
                IsActive INTEGER NOT NULL DEFAULT 1);");

            // Databases created before this update need the two new columns
            AddColumnIfMissing(conn, "Products", "ImagePath", "TEXT");
            AddColumnIfMissing(conn, "Products", "IsActive", "INTEGER NOT NULL DEFAULT 1");

            Run(conn, @"CREATE TABLE IF NOT EXISTS Sales (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProductId INTEGER NOT NULL,
                Quantity INTEGER NOT NULL,
                UnitPrice REAL NOT NULL,
                SoldAt TEXT NOT NULL,
                FOREIGN KEY (ProductId) REFERENCES Products(Id));");

            using var count = conn.CreateCommand();
            count.CommandText = "SELECT COUNT(*) FROM Products;";
            if ((long)count.ExecuteScalar()! == 0)
            {
                Run(conn, @"INSERT INTO Products (Name, Category, Size, Color, Location, Price, Quantity) VALUES
                    ('Classic T-Shirt', 'Tops', 'M', 'White', 'Aisle 1', 19.99, 25),
                    ('Classic T-Shirt', 'Tops', 'L', 'Black', 'Aisle 1', 19.99, 15),
                    ('Slim Fit Jeans', 'Bottoms', '32', 'Blue', 'Aisle 2', 59.99, 10),
                    ('Hoodie', 'Outerwear', 'L', 'Grey', 'Aisle 3', 44.50, 8);");
            }
        }

        private static void Run(SqliteConnection conn, string sql)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        private static void AddColumnIfMissing(SqliteConnection conn, string table, string column, string definition)
        {
            bool exists = false;

            using (var check = conn.CreateCommand())
            {
                check.CommandText = $"PRAGMA table_info({table});";
                using var r = check.ExecuteReader();
                while (r.Read())
                    if (string.Equals(r.GetString(r.GetOrdinal("name")), column, StringComparison.OrdinalIgnoreCase))
                        exists = true;
            }

            if (!exists) Run(conn, $"ALTER TABLE {table} ADD COLUMN {column} {definition};");
        }

        // ---------- Reading (safe when a text field is empty) ----------
        private static string Str(SqliteDataReader r, string column)
        {
            int i = r.GetOrdinal(column);
            return r.IsDBNull(i) ? "" : r.GetString(i);
        }

        private static Product ReadProduct(SqliteDataReader r) => new Product
        {
            Id = r.GetInt32(r.GetOrdinal("Id")),
            Name = Str(r, "Name"),
            Category = Str(r, "Category"),
            Size = Str(r, "Size"),
            Color = Str(r, "Color"),
            Location = Str(r, "Location"),
            Price = r.GetDecimal(r.GetOrdinal("Price")),
            Quantity = r.GetInt32(r.GetOrdinal("Quantity")),
            ImagePath = Str(r, "ImagePath"),
            IsActive = r.GetInt32(r.GetOrdinal("IsActive")) == 1
        };

        public List<Product> SearchByName(string name, bool includeDelisted = false)
        {
            var results = new List<Product>();

            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM Products
                                WHERE Name LIKE @name ESCAPE '\' AND (@all = 1 OR IsActive = 1)
                                ORDER BY Name, Size;";
            string safe = name.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
            cmd.Parameters.AddWithValue("@name", "%" + safe + "%");
            cmd.Parameters.AddWithValue("@all", includeDelisted ? 1 : 0);

            using var reader = cmd.ExecuteReader();
            while (reader.Read()) results.Add(ReadProduct(reader));
            return results;
        }

        public List<Product> GetAll(bool includeDelisted = false)
        {
            var results = new List<Product>();

            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Products WHERE (@all = 1 OR IsActive = 1) ORDER BY Name, Size;";
            cmd.Parameters.AddWithValue("@all", includeDelisted ? 1 : 0);

            using var reader = cmd.ExecuteReader();
            while (reader.Read()) results.Add(ReadProduct(reader));
            return results;
        }

        // Products at or below the given stock level, lowest first (delisted items are ignored)
        public List<Product> GetLowStock(int level)
        {
            var results = new List<Product>();

            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM Products WHERE Quantity <= @level AND IsActive = 1 ORDER BY Quantity, Name;";
            cmd.Parameters.AddWithValue("@level", level);

            using var reader = cmd.ExecuteReader();
            while (reader.Read()) results.Add(ReadProduct(reader));
            return results;
        }

        // True if another product (listed or delisted) has the same name, size and colour
        public bool Exists(string name, string size, string color, int? excludeId = null)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT COUNT(*) FROM Products
                                WHERE Name = @name COLLATE NOCASE
                                  AND Size = @size COLLATE NOCASE
                                  AND Color = @color COLLATE NOCASE
                                  AND (@id IS NULL OR Id <> @id);";
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@size", size);
            cmd.Parameters.AddWithValue("@color", color);
            cmd.Parameters.AddWithValue("@id", excludeId.HasValue ? (object)excludeId.Value : DBNull.Value);

            return (long)cmd.ExecuteScalar()! > 0;
        }

        // ---------- Writing ----------
        public void AddProduct(Product p)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Products (Name, Category, Size, Color, Location, Price, Quantity, ImagePath)
                                VALUES (@name, @cat, @size, @color, @loc, @price, @qty, @img);";
            AddFields(cmd, p);
            cmd.ExecuteNonQuery();
        }

        public bool UpdateProduct(Product p)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Products
                                SET Name=@name, Category=@cat, Size=@size, Color=@color,
                                    Location=@loc, Price=@price, Quantity=@qty, ImagePath=@img
                                WHERE Id=@id;";
            AddFields(cmd, p);
            cmd.Parameters.AddWithValue("@id", p.Id);
            return cmd.ExecuteNonQuery() > 0;
        }

        private static void AddFields(SqliteCommand cmd, Product p)
        {
            cmd.Parameters.AddWithValue("@name", p.Name);
            cmd.Parameters.AddWithValue("@cat", p.Category);
            cmd.Parameters.AddWithValue("@size", p.Size);
            cmd.Parameters.AddWithValue("@color", p.Color);
            cmd.Parameters.AddWithValue("@loc", p.Location);
            cmd.Parameters.AddWithValue("@price", p.Price);
            cmd.Parameters.AddWithValue("@qty", p.Quantity);
            cmd.Parameters.AddWithValue("@img",
                string.IsNullOrEmpty(p.ImagePath) ? DBNull.Value : (object)p.ImagePath);
        }

        public bool Restock(int productId, int amount)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Products SET Quantity = Quantity + @amt WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@amt", amount);
            cmd.Parameters.AddWithValue("@id", productId);
            return cmd.ExecuteNonQuery() > 0;
        }

        // Delist (false) or relist (true) an item
        public bool SetActive(int productId, bool active)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Products SET IsActive = @a WHERE Id = @id;";
            cmd.Parameters.AddWithValue("@a", active ? 1 : 0);
            cmd.Parameters.AddWithValue("@id", productId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool HasSales(int productId)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Sales WHERE ProductId = @id;";
            cmd.Parameters.AddWithValue("@id", productId);
            return (long)cmd.ExecuteScalar()! > 0;
        }

        // Permanent delete; refuses (returns false) if the product has any sales
        public bool Delete(int productId)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"DELETE FROM Products
                                WHERE Id = @id
                                  AND NOT EXISTS (SELECT 1 FROM Sales WHERE ProductId = @id);";
            cmd.Parameters.AddWithValue("@id", productId);
            return cmd.ExecuteNonQuery() > 0;
        }

        // Reduces stock and records the sale together; false if there isn't enough stock or the item is delisted
        public bool RecordSale(int productId, int quantity)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var tx = conn.BeginTransaction();

            using (var update = conn.CreateCommand())
            {
                update.Transaction = tx;
                update.CommandText = @"UPDATE Products
                                       SET Quantity = Quantity - @q
                                       WHERE Id = @id AND IsActive = 1 AND Quantity >= @q;";
                update.Parameters.AddWithValue("@q", quantity);
                update.Parameters.AddWithValue("@id", productId);

                if (update.ExecuteNonQuery() == 0)
                {
                    tx.Rollback();
                    return false;
                }
            }

            using (var insert = conn.CreateCommand())
            {
                insert.Transaction = tx;
                insert.CommandText = @"INSERT INTO Sales (ProductId, Quantity, UnitPrice, SoldAt)
                                       SELECT Id, @q, Price, @when FROM Products WHERE Id = @id;";
                insert.Parameters.AddWithValue("@q", quantity);
                insert.Parameters.AddWithValue("@id", productId);
                insert.Parameters.AddWithValue("@when", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                insert.ExecuteNonQuery();
            }

            tx.Commit();
            return true;
        }

        // Sales newest first; pass a date to see only that day's sales, or null for all
        public List<SaleRecord> GetSales(DateTime? day)
        {
            var results = new List<SaleRecord>();

            using var conn = new SqliteConnection(_connectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT s.Id, s.SoldAt, p.Name, p.Size, p.Color,
                       s.Quantity, s.UnitPrice, s.Quantity * s.UnitPrice AS Total
                FROM Sales s
                JOIN Products p ON p.Id = s.ProductId
                WHERE @day IS NULL OR substr(s.SoldAt, 1, 10) = @day
                ORDER BY s.SoldAt DESC;";
            cmd.Parameters.AddWithValue("@day",
                day.HasValue ? (object)day.Value.ToString("yyyy-MM-dd") : DBNull.Value);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new SaleRecord
                {
                    SaleId = reader.GetInt32(reader.GetOrdinal("Id")),
                    SoldAt = reader.GetString(reader.GetOrdinal("SoldAt")),
                    Item = Str(reader, "Name"),
                    Size = Str(reader, "Size"),
                    Color = Str(reader, "Color"),
                    Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                    UnitPrice = reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
                    Total = reader.GetDecimal(reader.GetOrdinal("Total"))
                });
            }

            return results;
        }
    }
}
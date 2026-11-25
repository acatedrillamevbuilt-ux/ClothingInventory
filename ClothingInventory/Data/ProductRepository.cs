using System.Data;
using Microsoft.Data.SqlClient;
using ClothingInventory.Models;

namespace ClothingInventory.Data
{
    public enum RestockOutcome { Done, NotFound, OverLimit }

    public class ProductRepository
    {
        private static readonly object InitLock = new();
        private static bool _ready;

        private static SqlConnection Open()
        {
            var conn = new SqlConnection(DbSettings.ConnectionString);
            conn.Open();
            return conn;
        }

        // ---------- Setup (runs once per launch) ----------
        public void Initialize()
        {
            lock (InitLock)
            {
                if (_ready) return;

                CreateDatabaseIfMissing();

                using var conn = Open();

                Run(conn, @"
                    IF OBJECT_ID('dbo.Products', 'U') IS NULL
                    CREATE TABLE dbo.Products (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Name NVARCHAR(100) NOT NULL,
                        Category NVARCHAR(50) NOT NULL DEFAULT '',
                        Size NVARCHAR(20) NOT NULL DEFAULT '',
                        Color NVARCHAR(30) NOT NULL DEFAULT '',
                        Location NVARCHAR(50) NOT NULL DEFAULT '',
                        Price DECIMAL(10,2) NOT NULL CHECK (Price >= 0),
                        Quantity INT NOT NULL DEFAULT 0 CHECK (Quantity >= 0),
                        ImagePath NVARCHAR(100) NOT NULL DEFAULT '',
                        IsActive BIT NOT NULL DEFAULT 1
                    );");

                Run(conn, @"
                    IF OBJECT_ID('dbo.Sales', 'U') IS NULL
                    CREATE TABLE dbo.Sales (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        ProductId INT NOT NULL REFERENCES dbo.Products(Id),
                        Quantity INT NOT NULL,
                        UnitPrice DECIMAL(10,2) NOT NULL,
                        SoldAt DATETIME2(0) NOT NULL
                    );");

                // Sample data only for a brand-new install (not when an old clothing.db is waiting to be imported)
                bool oldFile = File.Exists(Path.Combine(AppContext.BaseDirectory, "clothing.db"));
                if (!oldFile)
                {
                    Run(conn, @"
                        IF NOT EXISTS (SELECT 1 FROM dbo.Products)
                        INSERT INTO dbo.Products (Name, Category, Size, Color, Location, Price, Quantity) VALUES
                        ('Classic T-Shirt', 'Tops', 'M', 'White', 'Aisle 1', 19.99, 25),
                        ('Classic T-Shirt', 'Tops', 'L', 'Black', 'Aisle 1', 19.99, 15),
                        ('Slim Fit Jeans', 'Bottoms', '32', 'Blue', 'Aisle 2', 59.99, 10),
                        ('Hoodie', 'Outerwear', 'L', 'Grey', 'Aisle 3', 44.50, 8);");
                }

                _ready = true;
            }
        }

        private static void CreateDatabaseIfMissing()
        {
            var builder = new SqlConnectionStringBuilder(DbSettings.ConnectionString);
            string name = builder.InitialCatalog;
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("The connection string needs a Database name.");

            builder.InitialCatalog = "master";

            try
            {
                using var conn = new SqlConnection(builder.ConnectionString);
                conn.Open();

                using var cmd = conn.CreateCommand();
                cmd.CommandText =
                    "IF DB_ID(@n) IS NULL EXEC('CREATE DATABASE [' + REPLACE(@n, ']', ']]') + ']');";
                cmd.Parameters.AddWithValue("@n", name);
                cmd.ExecuteNonQuery();
            }
            catch (SqlException)
            {
                // No permission to create it (or to reach master): carry on if the database already exists
                using var probe = new SqlConnection(DbSettings.ConnectionString);
                probe.Open();
            }
        }

        private static void Run(SqlConnection conn, string sql)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.ExecuteNonQuery();
        }

        // ---------- Reading ----------
        private static string Str(SqlDataReader r, string column)
        {
            int i = r.GetOrdinal(column);
            return r.IsDBNull(i) ? "" : r.GetString(i);
        }

        private static Product ReadProduct(SqlDataReader r) => new Product
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
            IsActive = r.GetBoolean(r.GetOrdinal("IsActive"))
        };

        public List<Product> SearchByName(string name, bool includeDelisted = false)
        {
            var results = new List<Product>();

            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT * FROM dbo.Products
                                WHERE Name LIKE @name ESCAPE '\' AND (@all = 1 OR IsActive = 1)
                                ORDER BY Name, Size;";

            string safe = name.Replace(@"\", @"\\").Replace("%", @"\%")
                              .Replace("_", @"\_").Replace("[", @"\[");
            cmd.Parameters.AddWithValue("@name", "%" + safe + "%");
            cmd.Parameters.AddWithValue("@all", includeDelisted ? 1 : 0);

            using var reader = cmd.ExecuteReader();
            while (reader.Read()) results.Add(ReadProduct(reader));
            return results;
        }

        public List<Product> GetAll(bool includeDelisted = false)
        {
            var results = new List<Product>();

            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM dbo.Products WHERE (@all = 1 OR IsActive = 1) ORDER BY Name, Size;";
            cmd.Parameters.AddWithValue("@all", includeDelisted ? 1 : 0);

            using var reader = cmd.ExecuteReader();
            while (reader.Read()) results.Add(ReadProduct(reader));
            return results;
        }

        // Products at or below the given stock level, lowest first (delisted items are ignored)
        public List<Product> GetLowStock(int level)
        {
            var results = new List<Product>();

            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT * FROM dbo.Products WHERE Quantity <= @level AND IsActive = 1 ORDER BY Quantity, Name;";
            cmd.Parameters.AddWithValue("@level", level);

            using var reader = cmd.ExecuteReader();
            while (reader.Read()) results.Add(ReadProduct(reader));
            return results;
        }

        // True if another product (listed or delisted) has the same name, size and colour
        public bool Exists(string name, string size, string color, int? excludeId = null)
        {
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT COUNT(*) FROM dbo.Products
                                WHERE Name  = @name  COLLATE SQL_Latin1_General_CP1_CI_AS
                                  AND Size  = @size  COLLATE SQL_Latin1_General_CP1_CI_AS
                                  AND Color = @color COLLATE SQL_Latin1_General_CP1_CI_AS
                                  AND (@id IS NULL OR Id <> @id);";
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@size", size);
            cmd.Parameters.AddWithValue("@color", color);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value =
                excludeId.HasValue ? (object)excludeId.Value : DBNull.Value;

            return (int)cmd.ExecuteScalar()! > 0;
        }

        // ---------- Writing ----------
        private static void AddFields(SqlCommand cmd, Product p)
        {
            cmd.Parameters.Add("@name", SqlDbType.NVarChar, 100).Value = p.Name;
            cmd.Parameters.Add("@cat", SqlDbType.NVarChar, 50).Value = p.Category;
            cmd.Parameters.Add("@size", SqlDbType.NVarChar, 20).Value = p.Size;
            cmd.Parameters.Add("@color", SqlDbType.NVarChar, 30).Value = p.Color;
            cmd.Parameters.Add("@loc", SqlDbType.NVarChar, 50).Value = p.Location;

            var price = cmd.Parameters.Add("@price", SqlDbType.Decimal);
            price.Precision = 10;
            price.Scale = 2;
            price.Value = p.Price;

            cmd.Parameters.Add("@qty", SqlDbType.Int).Value = p.Quantity;
            cmd.Parameters.Add("@img", SqlDbType.NVarChar, 100).Value = p.ImagePath ?? "";
        }

        public void AddProduct(Product p)
        {
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO dbo.Products (Name, Category, Size, Color, Location, Price, Quantity, ImagePath)
                                VALUES (@name, @cat, @size, @color, @loc, @price, @qty, @img);";
            AddFields(cmd, p);
            cmd.ExecuteNonQuery();
        }

        public bool UpdateProduct(Product p)
        {
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE dbo.Products
                                SET Name=@name, Category=@cat, Size=@size, Color=@color,
                                    Location=@loc, Price=@price, Quantity=@qty, ImagePath=@img
                                WHERE Id=@id;";
            AddFields(cmd, p);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = p.Id;
            return cmd.ExecuteNonQuery() > 0;
        }

        // Adds stock, unless the total would pass the limit (checked inside the same SQL statement)
        public RestockOutcome Restock(int productId, int amount, int maxStock)
        {
            using var conn = Open();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"UPDATE dbo.Products SET Quantity = Quantity + @amt
                                    WHERE Id = @id AND Quantity + @amt <= @max;";
                cmd.Parameters.Add("@amt", SqlDbType.Int).Value = amount;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = productId;
                cmd.Parameters.Add("@max", SqlDbType.Int).Value = maxStock;
                if (cmd.ExecuteNonQuery() > 0) return RestockOutcome.Done;
            }

            // Nothing updated: either the item doesn't exist or the restock would pass the limit
            using var check = conn.CreateCommand();
            check.CommandText = "SELECT COUNT(*) FROM dbo.Products WHERE Id = @id;";
            check.Parameters.Add("@id", SqlDbType.Int).Value = productId;
            return (int)check.ExecuteScalar()! > 0 ? RestockOutcome.OverLimit : RestockOutcome.NotFound;
        }

        // Delist (false) or relist (true) an item
        public bool SetActive(int productId, bool active)
        {
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE dbo.Products SET IsActive = @a WHERE Id = @id;";
            cmd.Parameters.Add("@a", SqlDbType.Bit).Value = active;
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = productId;
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool HasSales(int productId)
        {
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM dbo.Sales WHERE ProductId = @id;";
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = productId;
            return (int)cmd.ExecuteScalar()! > 0;
        }

        // Permanent delete; refuses (returns false) if the product has any sales
        public bool Delete(int productId)
        {
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"DELETE FROM dbo.Products
                                WHERE Id = @id
                                  AND NOT EXISTS (SELECT 1 FROM dbo.Sales WHERE ProductId = @id);";
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = productId;
            return cmd.ExecuteNonQuery() > 0;
        }

        // Reduces stock and records the sale together; false if there isn't enough stock or the item is delisted
        public bool RecordSale(int productId, int quantity)
        {
            using var conn = Open();
            using var tx = conn.BeginTransaction();

            using (var update = conn.CreateCommand())
            {
                update.Transaction = tx;
                update.CommandText = @"UPDATE dbo.Products
                                       SET Quantity = Quantity - @q
                                       WHERE Id = @id AND IsActive = 1 AND Quantity >= @q;";
                update.Parameters.Add("@q", SqlDbType.Int).Value = quantity;
                update.Parameters.Add("@id", SqlDbType.Int).Value = productId;

                if (update.ExecuteNonQuery() == 0)
                {
                    tx.Rollback();
                    return false;
                }
            }

            using (var insert = conn.CreateCommand())
            {
                insert.Transaction = tx;
                insert.CommandText = @"INSERT INTO dbo.Sales (ProductId, Quantity, UnitPrice, SoldAt)
                                       SELECT Id, @q, Price, SYSDATETIME() FROM dbo.Products WHERE Id = @id;";
                insert.Parameters.Add("@q", SqlDbType.Int).Value = quantity;
                insert.Parameters.Add("@id", SqlDbType.Int).Value = productId;
                insert.ExecuteNonQuery();
            }

            tx.Commit();
            return true;
        }

        // Sales newest first; pass a date to see only that day's sales, or null for all
        public List<SaleRecord> GetSales(DateTime? day)
        {
            var results = new List<SaleRecord>();

            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT s.Id, s.SoldAt, p.Name, p.Size, p.Color,
                       s.Quantity, s.UnitPrice, s.Quantity * s.UnitPrice AS Total
                FROM dbo.Sales s
                JOIN dbo.Products p ON p.Id = s.ProductId
                WHERE @day IS NULL OR CAST(s.SoldAt AS date) = @day
                ORDER BY s.SoldAt DESC, s.Id DESC;";
            cmd.Parameters.Add("@day", SqlDbType.Date).Value =
                day.HasValue ? (object)day.Value.Date : DBNull.Value;

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                results.Add(new SaleRecord
                {
                    SaleId = reader.GetInt32(reader.GetOrdinal("Id")),
                    SoldAt = reader.GetDateTime(reader.GetOrdinal("SoldAt")).ToString("yyyy-MM-dd HH:mm:ss"),
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
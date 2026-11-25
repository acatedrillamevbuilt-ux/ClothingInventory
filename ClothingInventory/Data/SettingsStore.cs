using Microsoft.Data.SqlClient;

namespace ClothingInventory.Data
{
    // Small key/value table for app settings (the staff PIN hash and lockout state)
    public static class SettingsStore
    {
        private static readonly object Gate = new();
        private static bool _ready;

        private static SqlConnection Open()
        {
            var conn = new SqlConnection(DbSettings.ConnectionString);
            conn.Open();
            EnsureTable(conn);
            return conn;
        }

        private static void EnsureTable(SqlConnection conn)
        {
            if (_ready) return;

            lock (Gate)
            {
                if (_ready) return;

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    IF OBJECT_ID('dbo.Settings', 'U') IS NULL
                    CREATE TABLE dbo.Settings (
                        Name NVARCHAR(50) NOT NULL PRIMARY KEY,
                        Value NVARCHAR(400) NOT NULL
                    );";
                cmd.ExecuteNonQuery();
                _ready = true;
            }
        }

        public static string? Get(string name)
        {
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Value FROM dbo.Settings WHERE Name = @n;";
            cmd.Parameters.AddWithValue("@n", name);
            return cmd.ExecuteScalar() as string;
        }

        public static void Set(string name, string value)
        {
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE dbo.Settings SET Value = @v WHERE Name = @n;
                IF @@ROWCOUNT = 0 INSERT INTO dbo.Settings (Name, Value) VALUES (@n, @v);";
            cmd.Parameters.AddWithValue("@n", name);
            cmd.Parameters.AddWithValue("@v", value);
            cmd.ExecuteNonQuery();
        }

        public static void Delete(params string[] names)
        {
            using var conn = Open();
            foreach (string name in names)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM dbo.Settings WHERE Name = @n;";
                cmd.Parameters.AddWithValue("@n", name);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
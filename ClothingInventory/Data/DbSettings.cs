namespace ClothingInventory.Data
{
    // The database connection lives in ONE place. To point the app at another server,
    // put a connection string in a file called database.txt next to the app (no rebuild needed).
    public static class DbSettings
    {
        private const string DefaultConnection =
            @"Server=(localdb)\MSSQLLocalDB;Database=ClothingInventory;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=30;";

        public static string ConnectionString { get; } = Load();

        private static string Load()
        {
            try
            {
                string path = Path.Combine(AppContext.BaseDirectory, "database.txt");
                if (File.Exists(path))
                {
                    foreach (string raw in File.ReadAllLines(path))
                    {
                        string line = raw.Trim();
                        if (line.Length > 0 && !line.StartsWith('#')) return line;
                    }
                }
            }
            catch
            {
                // Fall back to the default below
            }

            return DefaultConnection;
        }
    }
}
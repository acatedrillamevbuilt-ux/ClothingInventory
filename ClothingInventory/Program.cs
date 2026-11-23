namespace ClothingInventory
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => ReportError(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => ReportError(e.ExceptionObject as Exception);

            ApplicationConfiguration.Initialize();
            Application.Run(new ClothingInventory.Forms.StartForm());
        }

        private static void ReportError(Exception? ex)
        {
            try
            {
                string path = Path.Combine(AppContext.BaseDirectory, "error.log");
                File.AppendAllText(path,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}{Environment.NewLine}{Environment.NewLine}");
            }
            catch
            {
                // Logging must never crash the app
            }

            MessageBox.Show(
                "Something went wrong with that action. You can keep using the app and try again.\n\n" +
                "If it keeps happening, send the file error.log (in the app's folder) to whoever maintains the app.",
                "Unexpected problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
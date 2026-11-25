using Microsoft.Data.SqlClient;
using ClothingInventory.Data;

namespace ClothingInventory
{
    internal static class Program
    {
        private const string DatabaseHelp =
            "The app can't reach its database.\n\n" +
            "Please check that:\n" +
            "  - SQL Server (or LocalDB) is installed and running. To test LocalDB, open Command Prompt and run: sqllocaldb info\n" +
            "  - If a shared server is used, the connection in the file database.txt is correct and the server PC is on.\n\n" +
            "The technical details were saved in error.log.";

        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) => ReportError(e.Exception);
            AppDomain.CurrentDomain.UnhandledException += (s, e) => ReportError(e.ExceptionObject as Exception);

            ApplicationConfiguration.Initialize();

            Application.Run(new Forms.StartForm());
        }

       
        private static bool IsDatabaseProblem(Exception? ex) =>
            ex is SqlException || ex?.InnerException is SqlException;

        private static void ReportError(Exception? ex)
        {
            Log(ex);

            MessageBox.Show(
                IsDatabaseProblem(ex)
                    ? DatabaseHelp
                    : "Something went wrong with that action. You can keep using the app and try again.\n\n" +
                      "If it keeps happening, send the file error.log (in the app's folder) to whoever maintains the app.",
                "Unexpected problem", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static void Log(Exception? ex)
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
        }
    }
}
using Microsoft.Data.Sqlite;
using WebCrawler.Models;

namespace WebCrawler.Services
{
    public class DatabaseService
    {
        private readonly string _dbPath = "Data/ProxyCrawler.db";

        public DatabaseService()
        {
            Directory.CreateDirectory("Data");

            using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                CREATE TABLE IF NOT EXISTS CrawlerLogs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    StartExecution TEXT NOT NULL,
                    EndExecution TEXT NOT NULL,
                    ProcessedPages INTEGER NOT NULL,
                    ExtractedRows INTEGER NOT NULL,
                    JsonFilePath TEXT NOT NULL
                )";
                command.ExecuteNonQuery();
            }
        }

        public async Task SaveLogAsync(CrawlerLog log)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));
            if (string.IsNullOrWhiteSpace(log.JsonFilePath))
                throw new ArgumentException("Caminho do JSON inválido.", nameof(log.JsonFilePath));

            try
            {
                using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
                {
                    await connection.OpenAsync();

                    var command = connection.CreateCommand();
                    command.CommandText = @"
                    INSERT INTO CrawlerLogs 
                    (StartExecution, EndExecution, ProcessedPages, ExtractedRows, JsonFilePath)
                    VALUES (@start, @end, @pages, @rows, @jsonPath)";

                    command.Parameters.AddWithValue("@start", log.StartExecution.ToString("o"));
                    command.Parameters.AddWithValue("@end", log.EndExecution.ToString("o"));
                    command.Parameters.AddWithValue("@pages", log.ProcessedPages);
                    command.Parameters.AddWithValue("@rows", log.ExtractedRows);
                    command.Parameters.AddWithValue("@jsonPath", log.JsonFilePath);

                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar log: {ex.Message}");
                throw;
            }
        }
    }
}
using WebCrawler.Models;
using WebCrawler.Services;

namespace WebCrawler
{
    class Program
    {
        static async Task Main()
        {
            var startTime = DateTime.Now;

            try
            {
                Console.WriteLine("Iniciando extração de proxies...");

                var proxyService = new ProxyService();
                var proxies = await proxyService.FetchProxiesMultithreadedAsync();

                Console.WriteLine($"Extraídos {proxies.Count} proxies. Salvando em JSON...");
                await proxyService.SaveProxiesAsJsonAsync(proxies);

                var log = new CrawlerLog
                {
                    StartExecution = startTime,
                    EndExecution = DateTime.Now,
                    ProcessedPages = (int)Math.Ceiling(proxies.Count / 20.0),
                    ExtractedRows = proxies.Count,
                    JsonFilePath = "proxies.json"
                };

                Console.WriteLine("Registrando log no banco de dados...");
                await new DatabaseService().SaveLogAsync(log);

                Console.WriteLine("Processo concluído! Verifique:");
                Console.WriteLine($"- Arquivo JSON: proxies.json");
                Console.WriteLine($"- Banco de dados: Data/ProxyCrawler.db");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }
    }
}
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using HtmlAgilityPack;
using WebCrawler.Models;

namespace WebCrawler.Services
{
    public class ProxyService : IProxyService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private const string BaseUrl = "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc/page/";
        private const int MaxPagesToCheck = 30;

        // Construtor
        public ProxyService()
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(60)
            };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml");
        }

        public async Task<List<Proxy>> FetchProxiesMultithreadedAsync()
        {
            var proxies = new ConcurrentBag<Proxy>();
            var tasks = new List<Task>();
            var semaphore = new SemaphoreSlim(3);
            int page = 1;
            int extractedProxyCount = 0;
            bool hasMorePages = true;

            while (page <= MaxPagesToCheck && hasMorePages)
            {
                await semaphore.WaitAsync();
                int currentPage = page;
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        string url = BaseUrl + currentPage;
                        // Baixa Conteúdo HTML
                        string html = await FetchHtmlPage(url);
                        if (html == null) return;

                        // Salva o Conteúdo HTML no arquivo pagina_1.html
                        await HtmlHelper.SaveHtmlContentAsync(html, currentPage);

                        //Carrega o HTML na memória para o programa analisar e extrair os dados
                        var htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(html);

                        var newProxies = ParseProxiesFromHtml(htmlDoc, currentPage);
                        foreach (var proxy in newProxies)
                        {
                            proxies.Add(proxy);
                            Interlocked.Increment(ref extractedProxyCount);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Erro na página {currentPage}: {ex.Message}");
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }));
                page++;
            }

            await Task.WhenAll(tasks);
            Console.WriteLine($"Total de proxies extraídos: {extractedProxyCount}");
            return proxies.ToList();
        }

        // Traz o conteúdo HTML
        private async Task<string> FetchHtmlPage(string url)
        {
            try
            {
                return await _httpClient.GetStringAsync(url);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Falha ao acessar {url}: {ex.Message}");
                throw new InvalidOperationException($"Falha ao buscar HTML da URL: {url}", ex);
            }
        }

        private List<Proxy> ParseProxiesFromHtml(HtmlDocument htmlDoc, int pageNumber)
        {
            var proxies = new List<Proxy>();
            var table = htmlDoc.DocumentNode.SelectSingleNode("//table[contains(@class, 'table-hover')]");
            if (table == null)
            {
                Console.WriteLine($"Tabela não encontrada na página {pageNumber}");
                return proxies;
            }

            var rows = table.SelectNodes(".//tbody/tr");
            if (rows == null || rows.Count == 0)
            {
                Console.WriteLine($"Nenhum proxy encontrado na página {pageNumber}");
                return proxies;
            }

            foreach (var row in rows)
            {
                var columns = row.SelectNodes("td");
                if (columns == null || columns.Count < 7) continue;

                var proxy = new Proxy
                {
                    IPAddress = columns[1]?.InnerText?.Trim(),
                    Port = ParsePort(columns[2], pageNumber),
                    Country = columns[3]?.InnerText?.Trim(),
                    Protocol = columns[6]?.InnerText?.Trim()
                };

                if (!string.IsNullOrEmpty(proxy.IPAddress))
                {
                    proxies.Add(proxy);
                }
            }

            Console.WriteLine($"Página {pageNumber} processada. Proxies válidos: {proxies.Count}");
            return proxies;
        }

        private string ParsePort(HtmlNode portColumn, int pageNumber)
        {
            try
            {
                var portHex = portColumn.SelectSingleNode(".//span")?.GetAttributeValue("data-port", "");
                if (string.IsNullOrEmpty(portHex)) return "N/A";

                if (int.TryParse(portHex, NumberStyles.HexNumber, null, out int portValue))
                {
                    return portValue.ToString();
                }
                Console.WriteLine($"Porta inválida na página {pageNumber}: {portHex}");
                return "N/A";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao converter porta na página {pageNumber}: {ex.Message}");
                return "N/A";
            }
        }

        public async Task SaveProxiesAsJsonAsync(List<Proxy> proxies)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                await File.WriteAllTextAsync("proxies.json", JsonSerializer.Serialize(proxies, options));
                Console.WriteLine($"JSON salvo com {proxies.Count} proxies.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao salvar JSON: {ex.Message}");
                throw;
            }
        }
    }
}
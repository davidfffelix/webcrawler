using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

public class ProxyService : IProxyService
{
    private readonly HttpClient _client = new HttpClient();
    private const string BaseUrl = "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc/page/";
    private const int MaxPagesToCheck = 20;

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
                    string html;

                    try
                    {
                        html = await _client.GetStringAsync(url);
                    }
                    catch (HttpRequestException)
                    {
                        hasMorePages = false;
                        return;
                    }

                    await HtmlHelper.SaveHtmlContentAsync(html, currentPage);

                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(html);

                    var table = htmlDoc.DocumentNode.SelectSingleNode("//table[contains(@class, 'proxy__t')]");
                    if (table == null)
                    {
                        hasMorePages = false;
                        return;
                    }

                    var rows = table.SelectNodes(".//tr[position() > 1]");
                    if (rows == null || rows.Count == 0)
                    {
                        hasMorePages = false;
                        return;
                    }

                    foreach (var row in rows)
                    {
                        var columns = row.SelectNodes("td");
                        if (columns == null || columns.Count < 7) continue;

                        proxies.Add(new Proxy
                        {
                            IPAddress = columns[0].InnerText.Trim(),
                            Port = columns[1].InnerText.Trim(),
                            Country = columns[2].InnerText.Trim(),
                            Protocol = columns[6].InnerText.Trim()
                        });
                        Interlocked.Increment(ref extractedProxyCount);
                    }
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

    public async Task SaveProxiesAsJsonAsync(List<Proxy> proxies)
    {
        string json = JsonSerializer.Serialize(proxies, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync("proxies.json", json);
    }
}
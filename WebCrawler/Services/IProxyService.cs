using WebCrawler.Models;

namespace WebCrawler.Services
{
    public interface IProxyService
    {
        Task<List<Proxy>> FetchProxiesMultithreadedAsync();
        Task SaveProxiesAsJsonAsync(List<Proxy> proxies);
    }
}
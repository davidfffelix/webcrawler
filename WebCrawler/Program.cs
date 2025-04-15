public class Program
{
    public static async Task Main()
    {
        IProxyService service = new ProxyService();
        var proxies = await service.FetchProxiesMultithreadedAsync();
        await service.SaveProxiesAsJsonAsync(proxies);
    }
}
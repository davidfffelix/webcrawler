using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        string url = "https://proxyservers.pro/proxy/list/order/updated/order_dir/desc";

        using HttpClient client = new HttpClient();
        string html = await client.GetStringAsync(url);

        Console.WriteLine(html);
    }
}
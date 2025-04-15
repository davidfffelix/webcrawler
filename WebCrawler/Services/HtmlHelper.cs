using System.IO;
using System.Threading.Tasks;

public static class HtmlHelper
{
    public static async Task SaveHtmlContentAsync(string html, int page)
    {
        string path = $"pagina_{page}.html";
        await File.WriteAllTextAsync(path, html);
    }
}
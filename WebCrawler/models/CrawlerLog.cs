namespace WebCrawler.Models
{

    public class CrawlerLog
    {
        public DateTime StartExecution { get; set; }
        public DateTime EndExecution { get; set; }
        public int ProcessedPages { get; set; }
        public int ExtractedRows { get; set; }
        public string JsonFilePath { get; set; } = string.Empty;
    }
}
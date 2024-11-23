namespace CryptoPriceAIAssistance.Cache;

public class CacheEntry
{
    public string Url { get; set; }
    public string Response { get; set; }
    public DateTime CachedAt { get; set; }
}

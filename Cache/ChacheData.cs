using System.Collections.Concurrent;

namespace CryptoPriceAIAssistance.Cache;

public class CacheData
{
    public DateTime CacheDate { get; set; }
    public ConcurrentDictionary<string, CacheEntry> Entries { get; set; } = new ConcurrentDictionary<string, CacheEntry>();
}

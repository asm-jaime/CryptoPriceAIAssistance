using System.Text.Json;

namespace CryptoPriceAIAssistance.Cache;

public class CacheService
{
    private readonly string CacheFilePath;
    private CacheData _cacheData;

    public CacheService(string filePath = "cache.json")
    {
        CacheFilePath = filePath;
        LoadCache();
    }

    private void LoadCache()
    {
        if(File.Exists(CacheFilePath))
        {
            try
            {
                string json = File.ReadAllText(CacheFilePath);
                _cacheData = JsonSerializer.Deserialize<CacheData>(json);
            }
            catch
            {
                _cacheData = new CacheData { CacheDate = DateTime.Today };
            }
        }
        else
        {
            _cacheData = new CacheData { CacheDate = DateTime.Today };
        }

        // Reset cache if date has changed
        if(_cacheData.CacheDate.Date != DateTime.Today)
        {
            _cacheData.Entries.Clear();
            _cacheData.CacheDate = DateTime.Today;
        }
    }

    private void SaveCache()
    {
        try
        {
            string json = JsonSerializer.Serialize(_cacheData);
            File.WriteAllText(CacheFilePath, json);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Error saving cache: {ex.Message}");
        }
    }

    public string Get(string key)
    {
        ResetIfNeeded();

        if(_cacheData.Entries.TryGetValue(key, out var entry))
        {
            return entry.Response;
        }
        else
        {
            return null;
        }
    }

    public void Set(string key, string value)
    {
        ResetIfNeeded();

        var entry = new CacheEntry
        {
            Url = key,
            Response = value,
            CachedAt = DateTime.Now
        };

        _cacheData.Entries[key] = entry;
        SaveCache();
    }

    private void ResetIfNeeded()
    {
        if(_cacheData.CacheDate.Date == DateTime.Today)
        {
            return;
        }

        _cacheData.Entries.Clear();
        _cacheData.CacheDate = DateTime.Today;
        SaveCache();
    }
}


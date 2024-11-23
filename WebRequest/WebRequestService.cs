using CryptoPriceAIAssistance.Cache;
using System.Text;

namespace CryptoPriceAIAssistance.WebRequest;

public class WebRequestService(CacheService cacheService)
{
    private readonly CacheService _cacheService = cacheService;

    public async Task<string> GetAsync(string url, Dictionary<string, string> headers = null)
    {
        var cachedResponse = _cacheService.Get(url);
        if (!string.IsNullOrEmpty(cachedResponse))
        {
            return cachedResponse;
        }

        using var client = new HttpClient();

        if (headers != null)
        {
            foreach (var header in headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        try
        {

            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            _cacheService.Set(url, responseContent);

            return responseContent;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"can't make request for {url}, we know nothing about what happens, hehe");
            throw;
        }
    }

    public async Task<string> PostAsync(string url, string content, Dictionary<string, string> headers = null)
    {
        using var client = new HttpClient();

        foreach (var header in headers)
        {
            // Exclude 'Content-Type' from DefaultRequestHeaders
            if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)) continue;

            client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        try
        {
            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, httpContent);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            return "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
        }
    }
}


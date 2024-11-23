using CryptoPriceAIAssistance.WebRequest;
using Newtonsoft.Json.Linq;
using System.Text;

namespace CryptoPriceAIAssistance.News;

public class NewsService(WebRequestService webRequestService)
{
    private readonly WebRequestService _webRequestService = webRequestService;

    public async Task<string> GetNewsForGpt()
    {
        var newsLastSevenDays = await GetNewsForLastSevenDays();

        var newsString = new StringBuilder();

        for (int i = 0; i < newsLastSevenDays.Count; i++)
        {
            int daysAgo = i + 1;
            newsString.AppendLine($"News for {daysAgo} days ago: {newsLastSevenDays[i]}");
        }

        return newsString.ToString();
    }

    private async Task<List<string>> GetNewsForLastSevenDays()
    {
        var tasks = Enumerable.Range(1, 7).Select(async daysAgo =>
        {
            await Task.Delay(daysAgo * 100);
            DateTime date = DateTime.Now.Date.AddDays(-daysAgo);
            string news = await GetTop3NewsPerDay(date);
            return new { DaysAgo = daysAgo, News = news };
        });

        var results = await Task.WhenAll(tasks);

        var output = results.OrderBy(r => r.DaysAgo)
                            .Select(r => $"News for {r.DaysAgo} days ago: {r.News}")
                            .ToList();

        return output;
    }

    private async Task<string> GetTop3NewsPerDay(DateTime date)
    {
        var queryParams = new Dictionary<string, string>
        {
            { "q", "cryptocurrency" },
            { "from", date.ToString("yyyy-MM-dd") },
            { "to", date.ToString("yyyy-MM-dd") },
            { "sortBy", "popularity" },
            { "pageSize", "3" },
            { "apiKey", "{API}" }
        };

        var queryString = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        var url = $"https://newsapi.org/v2/everything?{queryString}";

        var headers = new Dictionary<string, string>
        {
            { "User-Agent", "CSharpApp" },
            { "Accept", "application/json" }
        };

        var response = await _webRequestService.GetAsync(url, headers);

        var json = JObject.Parse(response);

        var articles = json["articles"] as JArray;
        if (articles == null)
        {
            return string.Empty;
        }

        var titles = articles.Select(a => a["title"]?.ToString() ?? string.Empty).Take(3);

        var truncatedTitles = titles.Select(t => t.Length > 50 ? t.Substring(0, 50) : t);

        var result = string.Join(" | ", truncatedTitles);

        return result;
    }
}


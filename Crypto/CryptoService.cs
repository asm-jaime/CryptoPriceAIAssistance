using CryptoPriceAIAssistance.WebRequest;
using Newtonsoft.Json;
using System.Text;

namespace CryptoPriceAIAssistance.Crypto;

public class PriceData
{
    public Data data { get; set; }
}

public class Data
{
    public string @base { get; set; }
    public string currency { get; set; }
    public List<PriceEntry> prices { get; set; }
}

public class PriceEntry
{
    public decimal Price { get; set; }
    public long Time { get; set; }
}


public class CryptoService(WebRequestService webRequestService)
{
    private readonly WebRequestService _webRequestService = webRequestService;
    private const string Url = "https://api.coinbase.com/v2/prices/BTC-USD/historic?period=week";
    private readonly Dictionary<string, string> Headers = new()
    {
        { "Authorization", "Bearer {API}" },
        { "CB-VERSION", "2023-10-01" }
    };

    public async Task<string> GetPricesForGpt()
    {
        var prices = await GetPricePerDay();
        var volatility = GetVolatility(prices);

        var pricesString = new StringBuilder();

        for (int i = 0; i < prices.Count; i++)
        {
            int daysAgo = prices.Count - i;
            decimal price = prices[i];

            pricesString.AppendLine($"Price for {daysAgo} days ago: {price:F2}");
        }
        pricesString.AppendLine($"Volatility: {volatility:F2}");

        return pricesString.ToString();
    }

    private async Task<List<decimal>> GetPricePerDay()
    {

        string response = await _webRequestService.GetAsync(Url, Headers);
        var priceData = JsonConvert.DeserializeObject<PriceData>(response);

        var prices = priceData.data.prices;

        var averagePricesPerDay = prices
            .Select(p => new
            {
                p.Price,
                DateTimeOffset.FromUnixTimeSeconds(p.Time).Date
            })
            .GroupBy(p => p.Date)
            .Select(g => new
            {
                Date = g.Key,
                Price = g.Average(p => p.Price)
            })
            .OrderBy(p => p.Date)
            .Select((p, i) => p.Price)
            .ToList();

        return averagePricesPerDay;
    }

    private decimal GetVolatility(List<decimal> prices)
    {
        if (prices.Count < 2)
        {
            return 0m;
        }

        var returns = new List<decimal>();

        for (int i = 1; i < prices.Count; i++)
        {
            decimal previousPrice = prices[i - 1];
            decimal currentPrice = prices[i];
            decimal dailyReturn = (currentPrice - previousPrice) / previousPrice;
            returns.Add(dailyReturn);
        }

        decimal averageReturn = returns.Average();
        decimal sumSquaredDifferences = returns.Sum(r => (r - averageReturn) * (r - averageReturn));
        decimal variance = sumSquaredDifferences / (returns.Count - 1);
        decimal standardDeviation = (decimal)Math.Sqrt((double)variance);

        return standardDeviation;
    }
}


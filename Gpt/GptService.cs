using CryptoPriceAIAssistance.WebRequest;
using Newtonsoft.Json;

namespace CryptoPriceAIAssistance.Gpt;

public class GptService(WebRequestService webRequestService)
{
    private const string ApiKey ="{API}";
    private const string ApiUrl = "https://api.proxyapi.ru/openai/v1/chat/completions";
    private readonly Dictionary<string, string> Headers = new()
    {
         { "Authorization", $"Bearer {ApiKey}" },
         { "Content-Type", "application/json" }
    };

    private readonly WebRequestService _webRequestService = webRequestService;
    private const int MaxTokens = 2028;
    private const string DefaultModel = "gpt-4";
    private const double DefaultTemperature = 0.5;

    private const string DefaultRole = "user";

    private const string PricesPrefix = "based on this BTC prices:";
    private const string NewsPrefix = "based on this news:";
    private const string UserPromptPrefix = "answer on this question:";

    public async Task<string> GetAnswer(string pricesForGpt, string newsForGpt, string userPrompt)
    {
        string totalUserMessage = $"{PricesPrefix}\n\n\n{pricesForGpt}\n\n\n{NewsPrefix}\n\n\n{newsForGpt}\n\n\n{UserPromptPrefix}\n\n\n {userPrompt}";

        var messages = new List<Dictionary<string, string>>
            {
                new() {
                    { "role", DefaultRole },
                    { "content", totalUserMessage }
                }
            };

        var gptQuestionPayload = new
        {
            model = DefaultModel,
            messages,
            max_tokens = MaxTokens,
            temperature = DefaultTemperature,
        };
        var jsonPayload = JsonConvert.SerializeObject(gptQuestionPayload);


        var response = await _webRequestService.PostAsync(ApiUrl, jsonPayload, Headers);
        if(string.IsNullOrEmpty(response))
        {
            return "No response received from the API.";
        }

        try
        {
            var jsonResponse = JsonConvert.DeserializeObject<ChatCompletionResponse>(response);
            var isJsonResponseValid = jsonResponse != null && jsonResponse.Choices != null && jsonResponse.Choices.Count > 0;
            if(isJsonResponseValid)
            {
                return jsonResponse.Choices[0].Message.Content.Trim();
            }
        }
        catch(Exception ex)
        {
            return "An error occurred while processing the response.";
        }

        return "";
    }
}


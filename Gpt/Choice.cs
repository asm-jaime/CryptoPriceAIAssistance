using Newtonsoft.Json;

namespace CryptoPriceAIAssistance.Gpt;

public class Choice
{
    [JsonProperty("index")]
    public int Index { get; set; }

    [JsonProperty("message")]
    public Message Message { get; set; }

    [JsonProperty("finish_reason")]
    public string FinishReason { get; set; }
}

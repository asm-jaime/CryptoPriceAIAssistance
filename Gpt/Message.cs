using Newtonsoft.Json;

namespace CryptoPriceAIAssistance.Gpt;
public class Message
{
    [JsonProperty("role")]
    public string Role { get; set; }

    [JsonProperty("content")]
    public string Content { get; set; }
}

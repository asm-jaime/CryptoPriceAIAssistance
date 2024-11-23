using CryptoPriceAIAssistance.Gpt;
using CryptoPriceAIAssistance.WebRequest;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;

namespace CryptoPriceAIAssistance.Tests;

[TestFixture]
public class GptServiceTests
{
    private GptService _gptService;
    private Mock<WebRequestService> _mockWebRequestService;

    [SetUp]
    public void Setup()
    {
        _mockWebRequestService = new Mock<WebRequestService>();
        _gptService = new GptService(_mockWebRequestService.Object);
    }

    [Test]
    public async Task GetAnswer_ReturnsExpectedResponse_WhenApiResponseIsValid()
    {
    }
}

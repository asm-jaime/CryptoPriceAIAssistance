using CryptoPriceAIAssistance.Crypto;
using CryptoPriceAIAssistance.Gpt;
using CryptoPriceAIAssistance.News;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace CryptoPriceAIAssistance.TelegramBot;

public class TelegramBotService
{
    private const string Token = "{API}";

    private readonly TelegramBotClient _botClient;

    private CryptoService _cryptoService;
    private NewsService _newsService;
    private GptService _gptService;

    private CancellationTokenSource _cts;
    private Task _mainTask;

    public TelegramBotService(CryptoService cryptoService, NewsService newsService, GptService gptService)
    {
        _botClient = new TelegramBotClient(Token);
        _cryptoService = cryptoService;
        _newsService = newsService;
        _gptService = gptService;
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        _mainTask = Task.Run(() => RunBot(_cts.Token));
    }

    public void Stop()
    {
        _cts.Cancel();
        _mainTask.Wait();
    }

    private async Task RunBot(CancellationToken cancellationToken)
    {
        var me = await _botClient.GetMeAsync();
        Console.WriteLine($"Bot id: {me.Id}. Bot Name: {me.FirstName}");

        int offset = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            var updates = await _botClient.GetUpdatesAsync(offset, cancellationToken: cancellationToken);

            foreach (var update in updates)
            {
                offset = update.Id + 1;

                if (update.Type == UpdateType.Message && update.Message.Text != null)
                {
                    var message = update.Message;

                    Console.WriteLine($"Received a message from {message.Chat.FirstName}: {message.Text}");

                    var pricesForGpt = await _cryptoService.GetPricesForGpt();
                    var newsForGpt = await _newsService.GetNewsForGpt();
                    var userPrompt = message.Text;
                    var gptAnswer = await _gptService.GetAnswer(pricesForGpt, newsForGpt, userPrompt);

                    // Respond 'yes' to any message
                    await _botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: gptAnswer,
                        cancellationToken: cancellationToken
                    );
                }
            }

            // Delay to prevent hitting Telegram API limits
            await Task.Delay(1000, cancellationToken);
        }
    }
}

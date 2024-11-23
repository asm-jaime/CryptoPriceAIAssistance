using CryptoPriceAIAssistance.Cache;
using CryptoPriceAIAssistance.Crypto;
using CryptoPriceAIAssistance.Gpt;
using CryptoPriceAIAssistance.News;
using CryptoPriceAIAssistance.TelegramBot;
using CryptoPriceAIAssistance.WebRequest;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// Register services
services.AddSingleton<CacheService>();
services.AddSingleton<WebRequestService>();
services.AddSingleton<CryptoService>();
services.AddSingleton<NewsService>();
services.AddSingleton<GptService>();
services.AddSingleton<TelegramBotService>();

var serviceProvider = services.BuildServiceProvider();

// Resolve the TelegramBotService
var botService = serviceProvider.GetService<TelegramBotService>();

// Start the bot
botService.Start();

Console.WriteLine("Bot is running. Press any key to stop.");
Console.ReadKey();

// Stop the bot
botService.Stop();

Console.WriteLine("Bot stopped. Press any key to exit.");
Console.ReadKey();


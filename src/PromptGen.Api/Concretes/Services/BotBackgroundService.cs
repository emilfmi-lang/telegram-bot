
using PromptGen.Api.Concretes.Interfaces;
using PromptGen.Api.Entities.Identity;
using PromptGen.Api.Persistence.Data;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace PromptGen.Api.Concretes.Services;

public class BotBackgroundService(ITelegramBotClient botClient, IServiceProvider serviceProvider) : BackgroundService
{
    private readonly ITelegramBotClient _botClient = botClient;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<BotBackgroundService> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = []
        };

        _botClient.StartReceiving(
             updateHandler: HandleUpdateAsync,
             errorHandler: HandlePollingErrorAsync,
             receiverOptions: receiverOptions,
             cancellationToken: stoppingToken
         );
        Console.WriteLine("🚀Bot işə düşdü və mesajları gözləyir...");
        await Task.Delay(-1, stoppingToken);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message) return;

        var chatId = message.Chat.Id;
        var firstName = message.From?.FirstName ?? "Dostum";

        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var geminiService = scope.ServiceProvider.GetRequiredService<IGeminiService>();

            var user = dbContext.Users.FirstOrDefault(u => u.TelegramId == chatId);
            if (user == null)
            {
                user = new AppUser { TelegramId = chatId, FirstName = firstName, CreatedAt = DateTime.UtcNow };
                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            if (message.Text != null && message.Text.StartsWith("/"))
            {
                if (message.Text.ToLower() == "/start")
                {
                    await botClient.SendMessage(chatId, $"Salam {firstName}! 👋\n\nMənə bir mövzu yaz (məs: 'Meşədə qədim qala') və ya bir şəkil göndər, mən isə sənin üçün Midjourney promptu hazırlayım.", cancellationToken: cancellationToken);
                }
                return;
            }

            try
            {
                string aiResult = string.Empty;

                if (message.Type == Telegram.Bot.Types.Enums.MessageType.Photo)
                {
                    await botClient.SendMessage(chatId, "📸 Şəkli analiz edirəm, bir az gözlə...", cancellationToken: cancellationToken);

                    var photo = message.Photo!.Last(); 
                    var file = await botClient.GetFile(photo.FileId, cancellationToken);

                    using var ms = new MemoryStream();
                    await botClient.DownloadFile(file.FilePath!, ms, cancellationToken);
                    var base64Image = Convert.ToBase64String(ms.ToArray());

                    aiResult = await geminiService.GeneratePromptFromImageAsync(base64Image, message.Caption);
                }
                else if (message.Text is { } messageText)
                {
                    await botClient.SendMessage(chatId, "🤖 Mövzu əsasında prompt hazırlayıram...", cancellationToken: cancellationToken);
                    aiResult = await geminiService.GeneratePromptAsync(messageText);
                }

                if (!string.IsNullOrEmpty(aiResult))
                {
                    await botClient.SendMessage(
                        chatId: chatId,
                        text: $"✨ **Sənin üçün hazırladığım Prompt:**\n\n`{aiResult}`",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        cancellationToken: cancellationToken
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mesaj emalı zamanı xəta!"); 
                await botClient.SendMessage(chatId, "⚠️ Üzr istəyirəm, Gemini AI hazırda bu sorğuya cavab verə bilmir.", cancellationToken: cancellationToken);
            }
        }
    }
    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine("Xəta baş verdi: " + exception.Message);
        return Task.CompletedTask;
    }
}

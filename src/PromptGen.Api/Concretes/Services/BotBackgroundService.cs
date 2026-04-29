namespace PromptGen.Api.Concretes.Services;

public class BotBackgroundService(ITelegramBotClient botClient, IServiceProvider serviceProvider) : BackgroundService
{
    private readonly ITelegramBotClient _botClient = botClient;
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<BotBackgroundService> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiverOptions = new ReceiverOptions { AllowedUpdates = [] };

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
                    await botClient.SendMessage(chatId, $"Salam {firstName}! 👋\n\nMənə bir mövzu yaz və ya şəkil göndər, mən isə sənin üçün Midjourney promptu hazırlayım.", cancellationToken: cancellationToken);
                }
                return;
            }

            try
            {
                string aiResult = string.Empty;

                if (message.Type == MessageType.Photo)
                {
                    await botClient.SendMessage(chatId, "📸 Şəkli analiz edirəm, bir az gözlə...", cancellationToken: cancellationToken);
                    var photo = message.Photo!.Last();
                    var file = await botClient.GetFile(photo.FileId, cancellationToken);
                    using var ms = new MemoryStream();
                    await botClient.DownloadFile(file.FilePath!, ms, cancellationToken);
                    ms.Position = 0;
                    var base64Image = Convert.ToBase64String(ms.ToArray());
                    aiResult = await geminiService.GeneratePromptFromImageAsync(base64Image, message.Caption);
                }

                else if (message.Text is { } messageText)
                {
                    var lowerText = messageText.ToLower().Trim();
                    var greetings = new List<string> { "salam", "sa", "merhaba", "hi", "hello", "necesen" };

                    if (!greetings.Any(g => lowerText.Contains(g)))
                    {
                        await botClient.SendMessage(chatId, "🤖 Mövzu əsasında prompt hazırlayıram...", cancellationToken: cancellationToken);
                    }

                    var history = await dbContext.ChatMessages
                        .Where(m => m.TelegramId == chatId)
                        .OrderByDescending(m => m.CreatedAt)
                        .Take(6)
                        .OrderBy(m => m.CreatedAt)
                        .ToListAsync(cancellationToken);

                    aiResult = await geminiService.GeneratePromptWithHistoryAsync(chatId, messageText, history);

                    dbContext.ChatMessages.Add(new PromptGen.Api.Entities.ChatMessage { TelegramId = chatId, Role = "user", Content = messageText });
                    dbContext.ChatMessages.Add(new PromptGen.Api.Entities.ChatMessage { TelegramId = chatId, Role = "model", Content = aiResult });

                    await dbContext.SaveChangesAsync(cancellationToken);

                    if (!string.IsNullOrEmpty(aiResult))
                    {
                        await botClient.SendMessage(
                            chatId: chatId,
                            text: aiResult,
                            parseMode: ParseMode.Markdown,
                            cancellationToken: cancellationToken
                        );
                    }

                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mesaj emalı zamanı xəta!");
                await botClient.SendMessage(chatId, "⚠️ Gemini AI hazırda cavab verə bilmir.", cancellationToken: cancellationToken);
            }
        }
    }
    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Polling xətası!");
        return Task.CompletedTask;
    }
}

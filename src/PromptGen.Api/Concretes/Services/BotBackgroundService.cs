
using PromptGen.Api.Entities.Identity;
using PromptGen.Api.Persistence.Data;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace PromptGen.Api.Concretes.Services;

public class BotBackgroundService(ITelegramBotClient botClient, IServiceProvider serviceProvider) : BackgroundService
{
    private readonly ITelegramBotClient _botClient = botClient;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

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
    }
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { Text: { } messageText } message)
            return;

        var chatId = message.Chat.Id;
        var firstName = message.From?.FirstName ?? "Naməlum";

        Console.WriteLine($"{firstName} mesaj yazdı: {messageText}");

        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var userExists = dbContext.Users.Any(u => u.TelegramId == chatId);

            if (!userExists)
            {
                var newUser = new AppUser
                {
                    TelegramId = chatId,
                    FirstName = firstName,
                    CreatedAt = DateTime.UtcNow
                };
                dbContext.Users.Add(newUser);
                await dbContext.SaveChangesAsync(cancellationToken);

                await botClient.SendMessage(
                    chatId: chatId,
                    text: $"Salam {firstName}, sən artıq bazaya qeyd olundun!",
                    cancellationToken: cancellationToken
                    );
            }
            else
            {
                await botClient.SendMessage(
                                    chatId: chatId,
                                    text: "Səni yenidən görmək xoşdur!",
                                    cancellationToken: cancellationToken
                                );
            }
        }
    }
    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine("Xəta baş verdi: " + exception.Message);
        return Task.CompletedTask;
    }
}

using Microsoft.EntityFrameworkCore;
using PromptGen.Api.Concretes.Services;
using PromptGen.Api.Persistence.Data;
using Scalar.AspNetCore;
using Serilog;
using Telegram.Bot;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/bot_log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
    builder.Services.AddOpenApi();
    builder.Services.AddSingleton<ITelegramBotClient>(sp =>
    {
        var configuration = sp.GetRequiredService<IConfiguration>();
        var token = configuration["BotConfiguration:BotToken"];

        if (string.IsNullOrEmpty(token))
        {
            throw new Exception("Bot Token appsettings.json fayl?nda tap?lmad?!");
        }

        return new TelegramBotClient(token);
    });
    builder.Services.AddHostedService<BotBackgroundService>();
    builder.Services.AddScoped<GeminiService>();

    builder.Services.AddControllers();

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();

        app.MapScalarApiReference(options =>
        {
            options.WithTitle("AI Prompt Generator API")
                   .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });
    }

    app.UseHttpsRedirection();
    app.MapControllers();

    Log.Information("Bot ve API (Scalar ile) aktivdir...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Tetbiq dayandi!");
}
finally
{
    Log.CloseAndFlush();
}
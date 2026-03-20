using Serilog;
using Scalar.AspNetCore; 

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/bot_log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    builder.Services.AddOpenApi();

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

    Log.Information("Bot v? API (Scalar il?) aktivdir...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "T?tbiq dayand?!");
}
finally
{
    Log.CloseAndFlush();
}
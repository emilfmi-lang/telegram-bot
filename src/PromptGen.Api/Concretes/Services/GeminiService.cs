using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;
using PromptGen.Api.Concretes.Interfaces;
using System.Text.Json;

namespace PromptGen.Api.Concretes.Services;

public class GeminiService : IGeminiService
{
    private readonly string _apiKey;
    public GeminiService(IConfiguration configuration)
    {
        _apiKey = configuration["GeminiConfiguration:ApiKey"];
    }
    public async Task<string> GeneratePromptAsync(string userInput)
    {
        var model = new GoogleAI(_apiKey).GenerativeModel(Model.Gemini20Flash);
        var response = await model.GenerateContent($"Bu mövzunu Midjourney promptuna çevir: {userInput}");
        return response.Text;
    }

    public async Task<string> GeneratePromptFromImageAsync(string base64Image, string? userPrompt = null)
    {
        var model = new GoogleAI(_apiKey).GenerativeModel(Model.Gemini20Flash);
        var request = new GenerateContentRequest
        {
            Contents = new List<Content> {
                new() {
                    Parts = new List<IPart> {
                        new TextData { Text = userPrompt ?? "Bu şəkli analiz et." },
                        new InlineData { MimeType = "image/jpeg", Data = base64Image }
                    }
                }
            }
        };
        var response = await model.GenerateContent(request);
        return response.Text;
    }

}

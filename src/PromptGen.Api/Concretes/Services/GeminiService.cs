using PromptGen.Api.Concretes.Interfaces;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PromptGen.Api.Concretes.Services;

public class GeminiService : IGeminiService
{
    private readonly string _apiKey;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<GeminiService> logger)
    {
        _apiKey = configuration["GeminiConfiguration:ApiKey"]
                  ?? throw new ArgumentNullException("Gemini API Key appsettings-də tapılmadı!");
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }
    public async Task<string> GeneratePromptAsync(string userInput)
    {
        _logger.LogInformation("Mətn əsaslı prompt istəyi gəldi: {Input}", userInput);

        var requestBody = new
        {
            contents = new[] {
                new { parts = new[] { new { text = GetSystemInstruction(userInput) } } }
            }
        };

        return await SendRequestToGemini(requestBody);
    }

    private async Task<string> SendRequestToGemini(object requestBody)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient();
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";

            var response = await client.PostAsJsonAsync(url, requestBody);

            if (!response.IsSuccessStatusCode)
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API Xətası! Status: {Status}, Detal: {Error}", response.StatusCode, errorMsg);
                return "Bağışlayın, AI hazırda cavab verə bilmir.";
            }

            var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();

            var resultText = jsonResponse.GetProperty("candidates")[0]
                                         .GetProperty("content")
                                         .GetProperty("parts")[0]
                                         .GetProperty("text").GetString();

            return resultText?.Trim() ?? "AI boş cavab qaytardı.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini sorğusu zamanı gözlənilməz xəta!");
            return "Sistem xətası baş verdi.";
        }
    }

    private object GetSystemInstruction(string input) =>
        $"Sən peşəkar Image Prompt mütəxəssisisən. Mövzu: '{input}'. Bu mövzunu Midjourney/DALL-E üçün detallı, ingilis dilində, bədii bir prompta çevir. Yalnız prompt mətnini qaytar.";
    public async Task<string> GeneratePromptFromImageAsync(string base64Image, string? userPrompt = null)
    {
        _logger.LogInformation("Şəkil analizi istəyi gəldi.");

        var requestBody = new
        {
            contents = new[] {
                new {
                    parts = new object[] {
                        new { text = userPrompt ?? "Bu şəkli analiz et və Midjourney üçün çox detallı, bədii ingilis dilində prompt yarat." },
                        new { inline_data = new { mime_type = "image/jpeg", data = base64Image } }
                    }
                }
            }
        };

        return await SendRequestToGemini(requestBody);
    }
}

using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PromptGen.Api.Concretes.Services;

public class GeminiService(IConfiguration configuration)
{
    private readonly string _apiKey = configuration["GeminiConfiguration:ApiKey"]!;
    private readonly HttpClient _httpClient = new();

    public async Task<string> GeneratePromptAsync(string userInput)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";
        var requestBody = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = $"Sən bir peşəkar Image Prompt mütəxəssisisən. İstifadəçinin verdiyi mövzu üçün Midjourney və ya DALL-E 3-də istifadə edilə bilən, detallı, ingilis dilində bədii bir prompt hazırla. Mövzu: {userInput}" } } }
            },
        };
        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) return "Xəta: AI ilə bağlantı qurula bilmədi.";

            var data = JsonNode.Parse(responseContent);
            return data?["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString() ?? "Xəta: Cavab tapılmadı";
        }
}

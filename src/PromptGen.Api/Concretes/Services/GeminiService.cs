using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;
using PromptGen.Api.Concretes.Interfaces;

namespace PromptGen.Api.Concretes.Services;

public class GeminiService : IGeminiService
{
    private readonly string _apiKey;
    private readonly ILogger<GeminiService> _logger;
    public GeminiService(IConfiguration configuration , ILogger<GeminiService> logger)
    {
        _apiKey = configuration["GeminiConfiguration:ApiKey"] ?? throw new Exception("API Key tapılmadı!");
        _logger = logger;
    }
    public async Task<string> GeneratePromptAsync(string userInput)
    {
        try
        {
            var googleAi = new GoogleAI(_apiKey);
            var model = googleAi.GenerativeModel(Model.Gemini25Flash);

            string systemInstruction =
                "Sən həm səmimi bir asistan, həm də peşəkar Midjourney mütəxəssisisən. " +
                "1. İstifadəçi səninlə salamlaşırsa, Azərbaycan dilində mehriban cavab ver. " +
                "2. İstifadəçi bir təsvir verərsə (məs: 'meşə', 'maşın'), dərhal ingilis dilində detallı Midjourney promptu hazırla və yalnız promptu qaytar.";

            var response = await model.GenerateContent($"{systemInstruction}\n\nİstifadəçi: {userInput}");
            return response.Text?.Trim() ?? "AI cavab verə bilmədi.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini SDK xətası!");
            return "Bağlantı xətası yaşandı. Bir az sonra yenidən yoxlayın.";
        }
    }
    public async Task<string> GeneratePromptFromImageAsync(string base64Image, string? userPrompt = null)
    {
        try
        {
            GoogleAI googleAi = new(_apiKey);
            var model = googleAi.GenerativeModel(Model.Gemini25Flash);

            var promptText = userPrompt ?? "Bu şəkli analiz et və Midjourney üçün ingilis dilində bədii prompt hazırla.";

            var request = new GenerateContentRequest
            {
                Contents = new List<Content> {
                    new Content {
                        Parts = new List<IPart> {
                            new TextData { Text = promptText },
                            new InlineData { MimeType = "image/jpeg", Data = base64Image }
                        }
                    }
                }
            };

            var response = await model.GenerateContent(request);
            return response.Text?.Trim() ?? "Şəkil analiz edilə bilmədi.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şəkil analizi xətası!");
            return "Şəkil emal edilərkən xəta baş verdi.";
        }
    }

}

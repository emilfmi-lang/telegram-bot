
namespace PromptGen.Api.Concretes.Interfaces;

public interface IGeminiService
{
    Task<string> GeneratePromptAsync(string userInput);
    Task<string> GeneratePromptFromImageAsync(string base64Image, string? userPrompt = null);
    Task<string> GeneratePromptWithHistoryAsync(long telegramId, string userInput, List<Entities.ChatMessage> history);
}
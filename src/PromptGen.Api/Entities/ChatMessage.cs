using PromptGen.Api.Entities.Common;

namespace PromptGen.Api.Entities;

public class ChatMessage : BaseEntity
{
    public long TelegramId { get; set; }
    public string Role { get; set; }
    public string Content { get; set; }
}
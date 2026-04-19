using PromptGen.Api.Entities.Common;
using PromptGen.Api.Entities.Identity;

namespace PromptGen.Api.Entities;

public class PromptLog : BaseEntity
{
    public Guid UserId { get; set; }
    public virtual AppUser User { get; set; } = null!;
    public string ImageHash { get; set; } = string.Empty;
    public string TelegramFileId { get; set; } = string.Empty;
    public string GeneratedPrompt { get; set; } = string.Empty;
    public long ProcessingTimeMs { get; set; }
    public PromptLog() { }
}


using PromptGen.Api.Entities.Common;

namespace PromptGen.Api.Entities.Identity;

public class AppUser : BaseEntity
{
    public long TelegramId { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
     
}

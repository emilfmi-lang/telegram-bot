using PromptGen.Api.Entities.Common;

namespace PromptGen.Api.Entities.Identity;

public class AppUser : BaseEntity
{   
    public long TelegramId { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string ? LastName { get; set; }
    public bool IsPremium { get; set; } = false;
    public int DailyRequestCount { get; set; } = 0;
    public DateTime LastRequestDate { get; set; } = DateTime.UtcNow;
    public virtual ICollection<PromptLog> PromptLogs { get; set; } = [];

}

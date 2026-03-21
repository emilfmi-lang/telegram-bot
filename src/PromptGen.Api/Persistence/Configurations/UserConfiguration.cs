using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromptGen.Api.Entities.Identity;

namespace PromptGen.Api.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.HasKey(u => u.Id);
        builder.HasIndex(u => u.TelegramId).IsUnique();
        builder.Property(u => u.UserName).HasMaxLength(100);
        builder.Property(u => u.FirstName).HasMaxLength(150);
        builder.Property(u => u.LastName).HasMaxLength(150);
        builder.HasMany(u => u.PromptLogs)
               .WithOne(p => p.User)
               .HasForeignKey(p => p.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

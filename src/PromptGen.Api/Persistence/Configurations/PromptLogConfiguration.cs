using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PromptGen.Api.Entities;

namespace PromptGen.Api.Persistence.Configurations;

public class PromptLogConfiguration : IEntityTypeConfiguration<PromptLog>
{
    public void Configure(EntityTypeBuilder<PromptLog> builder)
    {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.ImageHash);
        builder.Property(p => p.GeneratedPrompt).IsRequired();
        builder.Property(p => p.TelegramFileId).IsRequired().HasMaxLength(500);
    }
}

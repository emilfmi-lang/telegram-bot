using Microsoft.EntityFrameworkCore;
using PromptGen.Api.Entities;
using PromptGen.Api.Entities.Identity;
using System.Reflection;

namespace PromptGen.Api.Persistence.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}
    public DbSet<AppUser> Users { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.HasPostgresExtension("uuid-ossp");
    }
}

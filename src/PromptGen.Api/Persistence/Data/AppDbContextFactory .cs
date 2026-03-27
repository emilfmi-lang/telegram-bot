namespace PromptGen.Api.Persistence.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=PromptGenDb;Username=postgres;Password=mysecretpassword",
            npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()
        );

        return new AppDbContext(optionsBuilder.Options);
    }
}

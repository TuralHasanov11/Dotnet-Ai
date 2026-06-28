using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace WebApi1.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.MapWolverineEnvelopeStorage();
    }
}
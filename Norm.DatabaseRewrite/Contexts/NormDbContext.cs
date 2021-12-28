using Microsoft.EntityFrameworkCore;
using Norm.DatabaseRewrite.Entities;

namespace Norm.DatabaseRewrite.Contexts;

public class NormDbContext : DbContext
{
    public NormDbContext(DbContextOptions options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(NormDbContext).Assembly);
        base.OnModelCreating(builder);
    }

    // User Tables
    public DbSet<UserTimeZone> UserTimeZones => Set<UserTimeZone>();
}
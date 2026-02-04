using EmployeeContactApi.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeContactApi.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<EmployeeEntity> Employees => Set<EmployeeEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmployeeEntity>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}

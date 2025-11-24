using Microsoft.EntityFrameworkCore;

namespace ApiTest.ApiService;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Sample entity for testing the connection
    public DbSet<TestEntity> TestEntities { get; set; }
}

public class TestEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
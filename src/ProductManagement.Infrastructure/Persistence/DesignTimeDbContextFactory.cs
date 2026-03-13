using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProductManagement.Infrastructure.Persistence;

/// <summary>
/// Used by EF Core tooling (dotnet ef migrations add) at design time.
/// </summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ProductManagementDbContext>
{
    public ProductManagementDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProductManagementDbContext>();
        optionsBuilder.UseNpgsql(
            // TODO: Use from appSetting
            "Host=localhost;Port=5432;Database=productmanagement;Username=productmgmt;Password=productmgmt_dev",
            npgsql => npgsql.MigrationsAssembly(typeof(ProductManagementDbContext).Assembly.FullName));
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new ProductManagementDbContext(optionsBuilder.Options);
    }
}

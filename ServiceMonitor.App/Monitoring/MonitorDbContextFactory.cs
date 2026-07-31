using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ServiceMonitor.App.Monitoring;

public class MonitorDbContextFactory : IDesignTimeDbContextFactory<MonitorDbContext>
{
    public MonitorDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MonitorDbContext>();
        optionsBuilder.UseSqlite("Data Source=design-time.db");
        return new MonitorDbContext(optionsBuilder.Options);
    }
}
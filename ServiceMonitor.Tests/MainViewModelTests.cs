using Microsoft.EntityFrameworkCore;
using ServiceMonitor.App.Monitoring;
using ServiceMonitor.App.Services;
using ServiceMonitor.App.ViewModels;

namespace ServiceMonitor.Tests;

public class MainViewModelTests
{
    private class FakeStartupService : IStartupService
    {
        public bool Enabled { get; set; }
        public bool SetRunAtStartupCalled { get; private set; }
        public bool LastSetValue { get; private set; }

        public bool IsRunAtStartupEnabled() => Enabled;

        public void SetRunAtStartup(bool enable)
        {
            SetRunAtStartupCalled = true;
            LastSetValue = enable;
            Enabled = enable;
        }
    }

    private class TestDbContextFactory : IDbContextFactory<MonitorDbContext>
    {
        private readonly DbContextOptions<MonitorDbContext> _options;

        public TestDbContextFactory()
        {
            _options = new DbContextOptionsBuilder<MonitorDbContext>()
                .UseSqlite("Data Source=:memory:")
                .Options;
        }

        public MonitorDbContext CreateDbContext()
        {
            var context = new MonitorDbContext(_options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();
            return context;
        }

        public Task<MonitorDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(CreateDbContext());
    }

    [Fact]
    public void Constructor_InitializesStartOnLogin_FromStartupService()
    {
        var store = new MonitorResultsStore();
        var historyService = new HistoryService(new TestDbContextFactory());
        var startupService = new FakeStartupService { Enabled = true };

        var vm = new MainViewModel(store, historyService, startupService);

        Assert.True(vm.StartOnLogin);
    }

    [Fact]
    public void StartOnLogin_Changed_CallsStartupService()
    {
        var store = new MonitorResultsStore();
        var historyService = new HistoryService(new TestDbContextFactory());
        var startupService = new FakeStartupService { Enabled = false };

        var vm = new MainViewModel(store, historyService, startupService);
        Assert.False(vm.StartOnLogin);

        vm.StartOnLogin = true;

        Assert.True(startupService.SetRunAtStartupCalled);
        Assert.True(startupService.LastSetValue);
    }
}

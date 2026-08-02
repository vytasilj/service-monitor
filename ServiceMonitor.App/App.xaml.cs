using System.IO;
using System.Windows;
using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceMonitor.App.Configuration;
using ServiceMonitor.App.Monitoring;
using ServiceMonitor.App.Updates;
using ServiceMonitor.App.ViewModels;
using Velopack;
using Application = System.Windows.Application;

namespace ServiceMonitor.App;

public partial class App : Application
{
    private IHost? _host;
    private NotifyIcon? _trayIcon;
    private Mutex? _singleInstanceMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _singleInstanceMutex = new Mutex(true, "ServiceMonitor-SingleInstance-Mutex", out var isNewInstance);
        if (!isNewInstance)
        {
            System.Windows.MessageBox.Show(
                "Service Monitor is already running — check your system tray.",
                "Service Monitor",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            // This thread never acquired ownership of the mutex (another instance did),
            // so we must not call ReleaseMutex() on it later in OnExit — just dispose it.
            _singleInstanceMutex.Dispose();
            _singleInstanceMutex = null;

            Shutdown();
            return;
        }

        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton<ConfigService>();
        builder.Services.AddSingleton(sp => sp.GetRequiredService<ConfigService>().Load());
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainWindow>();
        builder.Services.AddSingleton<KubernetesStatusChecker>();
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<HttpEndpointChecker>();
        builder.Services.AddSingleton<MonitorResultsStore>();
        builder.Services.AddHostedService<MonitorBackgroundService>();

        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ServiceMonitor", "history.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        builder.Services.AddDbContextFactory<MonitorDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));
        builder.Services.AddSingleton<HistoryService>();

        builder.Services.AddSingleton(new UpdateService("https://github.com/vytasilj/service-monitor"));
        builder.Services.AddHostedService<UpdateCheckBackgroundService>();

        _host = builder.Build();

        using (var scope = _host.Services.CreateScope())
        {
            var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MonitorDbContext>>();
            using var db = dbFactory.CreateDbContext();
            db.Database.Migrate();
        }

        _host.Start();

        var store = _host.Services.GetRequiredService<MonitorResultsStore>();
        store.OverallStateChanged += OnOverallStateChanged;

        SetupTrayIcon();
        ShowMainWindow();
    }

    private void OnOverallStateChanged(HealthState state)
    {
        Dispatcher.Invoke(() =>
        {
            var iconName = state switch
            {
                HealthState.Ok => "status-ok",
                HealthState.Warning => "status-warn",
                HealthState.Error => "status-error",
                _ => "status-unknown"
            };
            var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", $"{iconName}.ico");

            var oldIcon = _trayIcon!.Icon;
            _trayIcon.Icon = new Icon(iconPath);
            oldIcon?.Dispose();

            if (state is HealthState.Warning or HealthState.Error)
            {
                var toolTipIcon = state == HealthState.Error ? ToolTipIcon.Error : ToolTipIcon.Warning;
                _trayIcon.ShowBalloonTip(5000, "Service Monitor", $"Status changed to {state}", toolTipIcon);
            }
        });
    }

    private void SetupTrayIcon()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "status-unknown.ico");

        _trayIcon = new NotifyIcon
        {
            Icon = new Icon(iconPath),
            Visible = true,
            Text = "Service Monitor"
        };

        _trayIcon.DoubleClick += (_, _) => ShowMainWindow();

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Open", null, (_, _) => ShowMainWindow());
        contextMenu.Items.Add("Exit", null, (_, _) => ExitApplication());
        _trayIcon.ContextMenuStrip = contextMenu;
    }

    private void ShowMainWindow()
    {
        MainWindow = _host!.Services.GetRequiredService<MainWindow>();
        MainWindow.Show();
        MainWindow.WindowState = WindowState.Normal;
        MainWindow.Activate();
    }

    private void ExitApplication()
    {
        AppState.IsExiting = true;
        _trayIcon!.Visible = false;
        Shutdown();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.StopAsync().GetAwaiter().GetResult();
        _trayIcon?.Dispose();
        _host?.Dispose();
        _singleInstanceMutex?.ReleaseMutex();
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }

    [STAThread]
    private static void Main(string[] args)
    {
        try
        {
            // Must run before anything else — this is what lets Velopack manage
            // installation, shortcuts, and (later) updates for this app.
            VelopackApp.Build().Run();

            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show("Unhandled startup exception: " + ex);
        }
    }
}
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceMonitor.App.ViewModels;
using Application = System.Windows.Application;

namespace ServiceMonitor.App;

public partial class App : Application
{
    private IHost? _host;
    private NotifyIcon? _trayIcon;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainWindow>();
        _host = builder.Build();

        SetupTrayIcon();
        ShowMainWindow();
    }

    private void SetupTrayIcon()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "status-unknown.ico");

        _trayIcon = new NotifyIcon
        {
            Icon = new System.Drawing.Icon(iconPath),
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
        _trayIcon?.Dispose();
        _host?.Dispose();
        base.OnExit(e);
    }
}
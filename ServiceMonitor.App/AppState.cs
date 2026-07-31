namespace ServiceMonitor.App;

public static class AppState
{
    // Set to true only when the user explicitly chooses "Exit" from the tray menu.
    // Distinguishes a real shutdown from the window merely being hidden to the tray.
    public static bool IsExiting { get; set; }
}
using System.Drawing;
using System.IO;
using System.Windows;
using Forms = System.Windows.Forms;

namespace PrivacyGuardian.Services;

public sealed class TrayService(ISettingsService settingsService) : ITrayService
{
    private Forms.NotifyIcon? _notifyIcon;
    private Window? _mainWindow;
    private bool _isExitRequested;

    public void Initialize(Window mainWindow)
    {
        _mainWindow = mainWindow;
        _mainWindow.Closing += OnMainWindowClosing;
        _mainWindow.StateChanged += (_, _) =>
        {
            if (_mainWindow.WindowState == WindowState.Minimized && settingsService.Current.RunInBackgroundOnClose)
            {
                HideToTray();
            }
        };

        _notifyIcon = new Forms.NotifyIcon
        {
            Text = "Privacy Guardian",
            Icon = LoadIcon(),
            Visible = true,
            ContextMenuStrip = BuildMenu()
        };
        _notifyIcon.DoubleClick += (_, _) => ShowMainWindow();
    }

    public void ExitApplication()
    {
        _isExitRequested = true;
        _notifyIcon?.Dispose();
        Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
    }

    public void Dispose()
    {
        if (_mainWindow is not null)
        {
            _mainWindow.Closing -= OnMainWindowClosing;
        }

        _notifyIcon?.Dispose();
    }

    private void OnMainWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_isExitRequested || !settingsService.Current.RunInBackgroundOnClose)
        {
            return;
        }

        e.Cancel = true;
        HideToTray();
    }

    private void HideToTray()
    {
        if (_mainWindow is null)
        {
            return;
        }

        _mainWindow.Hide();
        _mainWindow.WindowState = WindowState.Minimized;
    }

    private void ShowMainWindow()
    {
        if (_mainWindow is null)
        {
            return;
        }

        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private Forms.ContextMenuStrip BuildMenu()
    {
        var menu = new Forms.ContextMenuStrip();
        menu.Items.Add("Open Privacy Guardian", null, (_, _) => ShowMainWindow());
        menu.Items.Add("Exit", null, (_, _) => ExitApplication());
        return menu;
    }

    private static Icon LoadIcon()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Privacy_Guard_Icon.ico");
        return File.Exists(iconPath) ? new Icon(iconPath) : SystemIcons.Shield;
    }
}

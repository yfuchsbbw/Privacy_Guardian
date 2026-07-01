using System.Windows;
using PrivacyGuardian.Services;

namespace PrivacyGuardian.Views;

public partial class MainWindow : Window
{
    public MainWindow(ITrayService trayService)
    {
        InitializeComponent();
        Loaded += (_, _) => trayService.Initialize(this);
    }
}

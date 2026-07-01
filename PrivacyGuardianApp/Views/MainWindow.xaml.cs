using System.Windows;
using PrivacyGuardian.Services;

namespace PrivacyGuardian.Views;

public partial class MainWindow : Window
{
    public MainWindow(ITrayService trayService)
    {
        InitializeComponent();
        trayService.Initialize(this);
    }
}

using System.Windows;
using System.Windows.Media;

namespace PrivacyGuardian.Services;

public sealed class ThemeService : IThemeService
{
    public void Apply(bool isDarkMode)
    {
        var resources = System.Windows.Application.Current.Resources;
        Set(resources, "WindowBrush", isDarkMode ? "#FF0F1117" : "#FFF5F7FA");
        Set(resources, "PanelBrush", isDarkMode ? "#FF171B24" : "#FFFFFFFF");
        Set(resources, "SurfaceBrush", isDarkMode ? "#FF202532" : "#FFE8EDF5");
        Set(resources, "BorderBrush", isDarkMode ? "#FF313746" : "#FFD4DAE5");
        Set(resources, "TextBrush", isDarkMode ? "#FFF6F7FB" : "#FF172033");
        Set(resources, "MutedTextBrush", isDarkMode ? "#FF9EA7B8" : "#FF5B6576");
    }

    private static void Set(ResourceDictionary resources, string key, string color)
    {
        resources[key] = new SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color));
    }
}

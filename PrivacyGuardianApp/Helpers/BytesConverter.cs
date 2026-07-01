using System.Globalization;
using System.Windows.Data;

namespace PrivacyGuardian.Helpers;

public sealed class BytesConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value switch
        {
            long bytes => FormatHelper.Bytes(bytes),
            double bytes => FormatHelper.BytesPerSecond(bytes),
            _ => string.Empty
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        Binding.DoNothing;
}

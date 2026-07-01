using System.Collections.ObjectModel;
using PrivacyGuardian.Models;
using PrivacyGuardian.Services;

namespace PrivacyGuardian.ViewModels;

public sealed class UsbViewModel : ObservableViewModel
{
    public UsbViewModel(IUsbMonitorService usbMonitor, IUiDispatcher dispatcher, ILocalizationService localization)
    {
        localization.LanguageChanged += (_, _) => Title = localization.Get("Usb");
        Title = localization.Get("Usb");
        usbMonitor.DeviceChanged += (_, e) => dispatcher.Invoke(() => Events.Insert(0, e));
    }

    public ObservableCollection<UsbDeviceEvent> Events { get; } = [];
}

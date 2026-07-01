using System.Collections.ObjectModel;
using PrivacyGuardian.Models;
using PrivacyGuardian.Services;

namespace PrivacyGuardian.ViewModels;

public sealed class FileActivityViewModel : ObservableViewModel
{
    public FileActivityViewModel(IFileActivityService fileActivityService, IUiDispatcher dispatcher, ILocalizationService localization)
    {
        localization.LanguageChanged += (_, _) => Title = localization.Get("FileActivity");
        Title = localization.Get("FileActivity");
        fileActivityService.ActivityDetected += (_, e) => dispatcher.Invoke(() =>
        {
            Events.Insert(0, e);
            while (Events.Count > 300)
            {
                Events.RemoveAt(Events.Count - 1);
            }
        });
    }

    public ObservableCollection<FileActivityEvent> Events { get; } = [];
}

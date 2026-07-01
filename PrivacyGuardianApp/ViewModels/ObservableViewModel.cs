using PrivacyGuardian.Helpers;

namespace PrivacyGuardian.ViewModels;

public abstract class ObservableViewModel : ObservableObject
{
    private bool _isBusy;
    private string _status = string.Empty;
    private string _title = string.Empty;

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
}

using PrivacyGuardian.ViewModels;

namespace PrivacyGuardian.Services;

public interface INavigationService
{
    ObservableViewModel CurrentViewModel { get; }
    event EventHandler? CurrentViewModelChanged;
    void NavigateTo<TViewModel>() where TViewModel : ObservableViewModel;
}

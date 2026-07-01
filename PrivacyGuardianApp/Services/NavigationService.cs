using Microsoft.Extensions.DependencyInjection;
using PrivacyGuardian.ViewModels;

namespace PrivacyGuardian.Services;

public sealed class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private ObservableViewModel _currentViewModel;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _currentViewModel = _serviceProvider.GetRequiredService<DashboardViewModel>();
    }

    public ObservableViewModel CurrentViewModel => _currentViewModel;
    public event EventHandler? CurrentViewModelChanged;

    public void NavigateTo<TViewModel>() where TViewModel : ObservableViewModel
    {
        _currentViewModel = _serviceProvider.GetRequiredService<TViewModel>();
        CurrentViewModelChanged?.Invoke(this, EventArgs.Empty);
    }
}

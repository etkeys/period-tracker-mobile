
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PeriodTracker.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    bool isBusy = false;

    public IAsyncRelayCommand<string> WebBrowserNavigateCommand =>
        new AsyncRelayCommand<string>(LaunchWebBrowser);

    private async Task LaunchWebBrowser(string? url){
        if (string.IsNullOrWhiteSpace(url)) return;
        await Browser.OpenAsync(url);
    }
}
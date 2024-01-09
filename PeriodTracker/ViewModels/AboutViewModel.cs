using CommunityToolkit.Mvvm.ComponentModel;

namespace PeriodTracker.ViewModels;

public partial class AboutViewModel: ViewModelBase
{

    public AboutViewModel(IAppInfo appInfo){
        DisplayVersionText = appInfo.VersionString;
    }

    [ObservableProperty]
    private string _displayVersionText = string.Empty;

}

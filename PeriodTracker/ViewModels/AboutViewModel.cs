using CommunityToolkit.Mvvm.ComponentModel;

namespace PeriodTracker.ViewModels;

public partial class AboutViewModel: ViewModelBase
{

    [ObservableProperty]
    private string _displayVersionText = "?.?.?-alpha (sha1)";

}

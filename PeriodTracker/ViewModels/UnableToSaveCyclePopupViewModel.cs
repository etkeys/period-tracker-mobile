using CommunityToolkit.Mvvm.ComponentModel;

namespace PeriodTracker.ViewModels;

public partial class UnableToSaveCyclePopupViewModel : ViewModelBase
{

    [ObservableProperty]
    private string reasonText = string.Empty;

}

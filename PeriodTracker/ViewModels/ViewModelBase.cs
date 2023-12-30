
using CommunityToolkit.Mvvm.ComponentModel;

namespace PeriodTracker.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    bool isBusy = false;
}
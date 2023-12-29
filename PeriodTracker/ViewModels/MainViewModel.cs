
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PeriodTracker.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private int daysUntilNextPeriod = 98;

    [ObservableProperty]
    private string daysUntilNextPeriodText;

    public MainViewModel(){
        daysUntilNextPeriodText = $"{daysUntilNextPeriod}*";
    }

}
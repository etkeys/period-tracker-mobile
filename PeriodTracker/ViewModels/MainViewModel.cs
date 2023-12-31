
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PeriodTracker.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private const int defaultCycleLengthDays = 28;
    private bool dataRefreshRequired = true;

    [ObservableProperty]
    private string daysUntilNextCycleText = "??";

    [ObservableProperty]
    private bool isCycleStartOverdue;

    [ObservableProperty]
    private string nextCycleStartDateText = string.Empty;

    public async Task LoadAsync(){
        if (!dataRefreshRequired) return;

        var delayTask = Task.Delay(TimeSpan.FromSeconds(2));
        try{
            IsBusy = true;

            using var db = Repository.GetContext();
            var mostRecentCycleStart = (await db.GetMostRecentCycle())?.StartDate;
            await delayTask;

            if (mostRecentCycleStart is null) return;

            var daysEllapsed = (DateTime.Today - mostRecentCycleStart.Value).Days;
            var daysUntilNext = defaultCycleLengthDays - daysEllapsed;

            DaysUntilNextCycleText = daysUntilNext switch {
                0 when mostRecentCycleStart == DateTime.Today => $"{defaultCycleLengthDays}",
                >= 0 => $"{daysUntilNext}",
                _ => "0"
            };

            IsCycleStartOverdue = daysUntilNext < 0;
            NextCycleStartDateText = mostRecentCycleStart.Value
                .AddDays(defaultCycleLengthDays)
                .ToString("D");
        }
        finally{
            IsBusy = false;
            dataRefreshRequired = false;
        }
    }

}
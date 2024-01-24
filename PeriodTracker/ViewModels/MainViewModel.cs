
using CommunityToolkit.Mvvm.ComponentModel;

namespace PeriodTracker.ViewModels;

public partial class MainViewModel : ViewModelBase, IEventBusListener
{
    private const int defaultCycleLengthDays = 28;
    private bool dataRefreshRequired = true;
    private readonly IDbContextProvider _dbProvider;

    public MainViewModel(IDbContextProvider dbProvider){
        EventBus.RegisterListener(this);

        _dbProvider = dbProvider;
    }

    [ObservableProperty]
    private string daysUntilNextCycleText = "??";

    [ObservableProperty]
    private bool isCycleStartOverdue;

    [ObservableProperty]
    private string nextCycleStartDateText = string.Empty;

    public void HandleEvent(EventBusBroadcastedEvent @event){
        if (@event != EventBusBroadcastedEvent.CyclesUpdated) return;

        dataRefreshRequired = true;
    }

    public async Task LoadAsync(){
        if (!dataRefreshRequired) return;

        var delayTask = Task.Delay(TimeSpan.FromSeconds(2));
        try{
            IsBusy = true;
            IsCycleStartOverdue = false;

            using var db = await _dbProvider.GetContext();
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

            await CheckForUpdates();
        }
        finally{
            IsBusy = false;
            dataRefreshRequired = false;
        }
    }

    private async Task CheckForUpdates(){
        using var updateSvc = ServiceHelper.GetService<IUpdateService>()!;

        if (!await updateSvc.GetShouldCheckForUpdates()) return;

        var latestVersion = await updateSvc.GetLatestVersion();

        await updateSvc.SetNextNotifyTime();

        // TODO need to add logging or something
        if (latestVersion is null) return;

        var currentVersion = ServiceHelper.GetService<IAppInfo>()!.Version;

        if (latestVersion > currentVersion)
            await ServiceHelper.GetService<IAlertService>()
                !.ShowAlertAsync(
                    "Update available",
                    $"New version {latestVersion:3} is available. For instructions about how to upate, see About > How to update."
                );
    }

}
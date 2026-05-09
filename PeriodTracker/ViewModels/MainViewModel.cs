
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore;
using PeriodTracker.Services;

namespace PeriodTracker.ViewModels;

public partial class MainViewModel : ViewModelBase, IEventBusListener
{
    private const int _defaultCycleLengthDays = 29;
    private const int _defaultPeriodLengthDays = 6;
    private bool dataRefreshRequired = true;

    private readonly IAlertService _alertService;
    private readonly IAppInfo _appInfo;
    private readonly IDbContextProvider _dbContextProvider;
    private readonly IDateTimeService _dateTimeService;
    private readonly IUpdateServiceProvider _updateServiceProvider;

    public MainViewModel(
        IDbContextProvider dbContextProvider,
        IDateTimeService dateTimeService,
        IUpdateServiceProvider updateServiceProvider,
        IAppInfo appInfo,
        IAlertService alertService) {

        EventBus.RegisterListener(this);

        _alertService = alertService;
        _appInfo = appInfo;
        _dbContextProvider = dbContextProvider;
        _dateTimeService = dateTimeService;
        _updateServiceProvider = updateServiceProvider;
    }

    [ObservableProperty]
    private string daysUntilNextCycleText = "??";

    [ObservableProperty]
    private bool isCycleStartOverdue;

    [ObservableProperty]
    private string fertileWindowDateText = string.Empty;
    [ObservableProperty]
    private string fertileWindowText = string.Empty;

    [ObservableProperty]
    private string nextCycleStartDateText = string.Empty;

    [ObservableProperty]
    private string periodEndDateText = string.Empty;
    [ObservableProperty]
    private string periodEndText = string.Empty;

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

            var today = _dateTimeService.Today;

            using var db = await _dbContextProvider.GetContext();
            var mostRecentCycleStart = await
                (from c in db.Cycles
                orderby c.StartDate descending
                select c.StartDate)
                .FirstOrDefaultAsync();
            await delayTask;

            if (mostRecentCycleStart.Equals(default)) return;

            var daysEllapsed = (today - mostRecentCycleStart).Days;
            var daysUntilNext = _defaultCycleLengthDays - daysEllapsed;

            DaysUntilNextCycleText = daysUntilNext switch {
                0 when mostRecentCycleStart == today => $"{_defaultCycleLengthDays}",
                >= 0 => $"{daysUntilNext}",
                _ => "0"
            };

            IsCycleStartOverdue = daysUntilNext < 0;
            NextCycleStartDateText = mostRecentCycleStart
                .AddDays(_defaultCycleLengthDays)
                .ToString("D");

            UpdateFertileWindowText(mostRecentCycleStart);
            UpdatePeriodEndText(mostRecentCycleStart);

            await CheckForUpdates();
        }
        finally{
            IsBusy = false;
            dataRefreshRequired = false;
        }
    }

    private async Task CheckForUpdates(){
        using var updateSvc = _updateServiceProvider.GetUpdateService();

        if (!await updateSvc.GetShouldCheckForUpdates()) return;

        var latestVersion = await updateSvc.GetLatestVersion();

        await updateSvc.SetNextNotifyTime();

        // TODO need to add logging or something
        if (latestVersion is null) return;

        var currentVersion = _appInfo.Version;

        if (latestVersion > currentVersion)
            await _alertService.ShowAlertAsync(
                "Update available",
                $"New version {latestVersion:3} is available. For instructions about how to update, see About > How to update."
            );
    }

    private void UpdateFertileWindowText(DateTime cycleStart)
    {
        FertileWindowText = string.Empty;
        FertileWindowDateText = string.Empty;

        if (cycleStart == default)
            return;

        // Fertile window is considered to be the 3 days leading up to ovulation,
        // the day of ovulation (which occurs around 14-15 days after the current
        // cycle starts), and the day after ovulation.
        // NOTE: Without tracking ovulation more precisely, this is just an
        // estimate based on the average cycle length and ovulation timing.
        // NOTE: Ovulation day can also be calculated by halving the cycle length.
        // This is probably the way to go to allow for different cycle lengths.

        var ovulationDay = cycleStart.AddDays(Math.Floor(_defaultCycleLengthDays / 2.0));

        var fertileWindowStart = ovulationDay.AddDays(-3);
        var fertileWindowEnd = ovulationDay.AddDays(1);

        var today = _dateTimeService.Today;
        if (today > fertileWindowEnd)
            return;
        else if (today == fertileWindowEnd)
        {
            FertileWindowText = "Today should be the last day with the highest chance for pregnancy.";
        }
        else if (today == fertileWindowEnd.AddDays(-1))
        {
            FertileWindowText = "Tomorrow should be the last day with the highest chances for pregnancy.";
            FertileWindowDateText = $"Ends:{Environment.NewLine}{fertileWindowEnd:D}";
        }
        else if (today >= fertileWindowStart)
        {
            var daysRemaining = (fertileWindowEnd - today).Days;
            FertileWindowText = $"Highest chances for pregnancy should be ending soon. Last day should be in {daysRemaining} days.";
            FertileWindowDateText = $"Ends:{Environment.NewLine}{fertileWindowEnd:D}";
        }
        else
        {
            var daysUntil = (fertileWindowStart - today).Days;
            var dayText = daysUntil > 1 ? "days" : "day";
            FertileWindowText = $"Highest chances for pregnancy should start in {daysUntil} {dayText}.";
            FertileWindowDateText = $"{fertileWindowStart:D}{Environment.NewLine}      thru{Environment.NewLine}{fertileWindowEnd:D}";
        }

    }

    private void UpdatePeriodEndText(DateTime cycleStart)
    {
        var periodEndDate = cycleStart.AddDays(_defaultPeriodLengthDays);
        var daysRemaining = (cycleStart == default)
            ? -1
            : _defaultPeriodLengthDays - (_dateTimeService.Today - cycleStart).Days;

        PeriodEndDateText = daysRemaining > 0 ? periodEndDate.ToString("D") : string.Empty;
        PeriodEndText = daysRemaining switch
        {
            // > 0 => $"The last day of your period should be {periodEndDate:D} (in {daysRemaining} days).",
            > 1 => $"Last day of your period should be in {daysRemaining} days:",
            1 => $"Last day of your period should be in {daysRemaining} day:",
            0 => "Today should be the last day of your period.",
            _ => string.Empty
        };
    }

}
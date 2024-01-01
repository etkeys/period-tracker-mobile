
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PeriodTracker.ViewModels;

public partial class CycleEditViewModel : ViewModelBase
{
    private const string saveButtonTextSave = "Save";
    private const string saveButtonTextSaving = "Saving...";

    public CycleEditViewModel(){
        PageTitleText = "New cycle";
        SaveButtonText = saveButtonTextSave;
    }

    [ObservableProperty]
    private DateTime maxStartDate = DateTime.Today;
    [ObservableProperty]
    private DateTime minStartDate = DateTime.Today.AddMonths(-2);
    [ObservableProperty]
    private string pageTitleText;
    [ObservableProperty]
    private string saveButtonText;
    [ObservableProperty]
    private DateTime selectedStartDate = DateTime.Today;

    private async Task<bool> IsNewEntryValid(Repository db, Cycle newEntry){
        var cycles =
            (from c in await db.GetCycles()
            select c.StartDate)
            .ToHashSet();

        return !cycles.Contains(newEntry.StartDate);

        // TODO Should check for nearby dates, like one within a week?
    }

    public async Task<bool> Save() {
        var delayTask = Task.Delay(TimeSpan.FromSeconds(2));

        try{
            // Do saving here
            IsBusy = true;
            SaveButtonText = saveButtonTextSaving;

            var newEntry = new Cycle{
                RecordedDate = DateTime.Today,
                StartDate = SelectedStartDate,
            };

            using var db = Repository.GetContext();
            if (!await IsNewEntryValid(db, newEntry)){
                await ServiceHelper.GetService<IAlertService>()
                    !.ShowAlertAsync(
                        "Save failed",
                        $"An entry with the same start date already exists.");

                return false;
            }

            await db.AddCycle(newEntry);
            await EventBus.BroadcastEvent(EventBusBroadcastedEvent.CyclesUpdated);
            return true;
        }
        finally{
            // Let the user think work is being done
            await delayTask;
            SaveButtonText = saveButtonTextSave;
            IsBusy = false;
        }
    }

}
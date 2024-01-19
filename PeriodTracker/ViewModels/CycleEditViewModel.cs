
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PeriodTracker.ViewModels;

public partial class CycleEditViewModel : ViewModelBase
{
    private const string saveButtonTextSave = "Save";
    private const string saveButtonTextSaving = "Saving...";

    private readonly IDbContextProvider _dbProvider;

    public CycleEditViewModel(IDbContextProvider dbProvider){
        _dbProvider = dbProvider;

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

    public async Task<bool> Save() {
        var delayTask = Task.Delay(TimeSpan.FromSeconds(2));

        try{
            IsBusy = true;
            SaveButtonText = saveButtonTextSaving;

            var newEntry = new Cycle{
                RecordedDate = DateTime.Today,
                StartDate = SelectedStartDate,
            };

            using var db = await _dbProvider.GetContext();
            if (!await db.AddCycle(newEntry)){
                await ServiceHelper.GetService<IAlertService>()
                    !.ShowAlertAsync(
                        "Save failed",
                        $"An entry with the same start date already exists.");

                return false;
            }

            await EventBus.BroadcastEvent(EventBusBroadcastedEvent.CyclesUpdated);
            return true;
        }
        finally{
            await delayTask;
            SaveButtonText = saveButtonTextSave;
            IsBusy = false;
        }
    }

}
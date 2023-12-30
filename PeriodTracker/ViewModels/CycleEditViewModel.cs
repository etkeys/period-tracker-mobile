
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

    public async Task Save() {
        var delayTask = Task.Delay(TimeSpan.FromSeconds(2));

        try{
            // Do saving here
            IsBusy = true;
            SaveButtonText = saveButtonTextSaving;

        }
        finally{
            // Let the user think work is being done
            await delayTask;
            SaveButtonText = saveButtonTextSave;
            IsBusy = false;
        }
    }

}
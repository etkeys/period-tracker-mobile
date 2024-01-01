using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

namespace PeriodTracker.ViewModels;

public class HistoryViewModel : ViewModelBase, IEventBusListener
{

    private bool dataRefreshRequired = true;

    public HistoryViewModel(){
        EventBus.RegisterListener(this);

        DeleteCycleCommand = new AsyncRelayCommand<Cycle>(DeleteCycle);
    }

    public ObservableCollection<Cycle> Cycles {get; private set;} = new();

    public IAsyncRelayCommand<Cycle> DeleteCycleCommand {get;}

    private async Task DeleteCycle(Cycle? cycle){
        if (cycle is null) return;

        var confirmDelete = await ServiceHelper.GetService<IAlertService>()
            !.ShowConfirmationAsync(
                "Confirm delete",
                $"Are you sure you want to delete cycle with start date \"{cycle.StartDateText}\"?");

        if (!confirmDelete) return;

        var delayTask = Task.Delay(TimeSpan.FromSeconds(2));
        try{
            IsBusy = true;

            using var db = Repository.GetContext();
            var deleted = await db.DeleteCycle(cycle);

            await delayTask;

            if (deleted)
                Cycles.RemoveAt(Cycles.IndexOf(cycle));

            await EventBus.BroadcastEvent(EventBusBroadcastedEvent.CyclesUpdated);
        }
        finally{
            IsBusy = false;
            dataRefreshRequired = false;
        }
    }

    public void HandleEvent(EventBusBroadcastedEvent @event){
        if (@event != EventBusBroadcastedEvent.CyclesUpdated) return;

        dataRefreshRequired = true;
    }

    public async Task LoadAsync(){
        if (!dataRefreshRequired) return;

        var delayTask = Task.Delay(TimeSpan.FromSeconds(2));
        try{
            IsBusy = true;

            using var db = Repository.GetContext();
            var cycles =
                from c in await db.GetCycles()
                orderby c.StartDate descending
                select c;
            Cycles = new ObservableCollection<Cycle>(cycles);

            dataRefreshRequired = false;
        }
        finally{
            await delayTask;
            IsBusy = false;
            OnPropertyChanged(nameof(Cycles));
        }
    }
}

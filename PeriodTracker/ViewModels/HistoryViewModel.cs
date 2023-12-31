using System.Collections.ObjectModel;

namespace PeriodTracker.ViewModels;

public class HistoryViewModel : ViewModelBase, IEventBusListener
{

    private bool dataRefreshRequired = true;

    public HistoryViewModel(){
        EventBus.RegisterListener(this);
    }

    public ObservableCollection<Cycle> Cycles {get; private set;} = new();

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

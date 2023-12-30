using System.Collections.ObjectModel;

namespace PeriodTracker.ViewModels;

public class HistoryViewModel : ViewModelBase
{

    public ObservableCollection<Cycle> Cycles {get; private set;} = new(
        new Cycle[]{
            new(){
                RecordedDate = DateTime.Parse("9/29/2023"),
                StartDate = DateTime.Parse("9/29/2023"),
                },
            new(){
                RecordedDate = DateTime.Parse("10/31/2023"),
                StartDate = DateTime.Parse("10/30/2023")
                },
            new(){
                RecordedDate = DateTime.Parse("11/27/2023"),
                StartDate = DateTime.Parse("11/27/2023")
                },
        }
    );

    // public async Task LoadAsync(){
    //     var delayTask = Task.Delay(TimeSpan.FromSeconds(2));
    //     try{
    //         IsBusy = true;
    //         await Task.Run(() =>{

    //             using var db = Repository.GetContext();
    //             var cycles =
    //                 from c in db.GetCycles()
    //                 orderby c.StartDate descending
    //                 select c;
    //             Cycles = new ObservableCollection<Cycle>(cycles);
    //             OnPropertyChanged(nameof(Cycles));

    //         });
    //     }
    //     finally{
    //         await delayTask;
    //         IsBusy = false;
    //     }
    // }
}

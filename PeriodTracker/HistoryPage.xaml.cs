using PeriodTracker.ViewModels;

namespace PeriodTracker;

public partial class HistoryPage : ContentPage
{
	public HistoryPage(HistoryViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
	}

    protected override async void OnNavigatedTo(NavigatedToEventArgs e){
        await ((HistoryViewModel)BindingContext).LoadAsync();
    }

    private async void OnRecordNewClicked(object sender, EventArgs e){
        ;
    }
}
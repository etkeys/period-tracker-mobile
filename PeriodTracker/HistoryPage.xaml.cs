using PeriodTracker.ViewModels;

namespace PeriodTracker;

public partial class HistoryPage : ContentPage
{
	public HistoryPage(HistoryViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
	}

    private async void OnRecordNewClicked(object sender, EventArgs e){
        ;
    }
}
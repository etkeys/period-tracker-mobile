using PeriodTracker.ViewModels;

namespace PeriodTracker;

public partial class MainPage : ContentPage
{
	public MainPage(MainViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
	}

    private async void OnRecordNewClicked(object sender, EventArgs e){
        await Navigation.PushModalAsync(new CycleEditPage(new CycleEditViewModel()));
    }

}


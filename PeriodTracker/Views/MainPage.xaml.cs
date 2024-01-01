using PeriodTracker.ViewModels;

namespace PeriodTracker;

public partial class MainPage : ContentPage
{
	public MainPage(MainViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
	}

    protected override async void OnNavigatedTo(NavigatedToEventArgs e){
        await ((MainViewModel)BindingContext).LoadAsync();
    }

    private async void OnRecordNewClicked(object sender, EventArgs e){
        await Navigation.PushModalAsync(new CycleEditPage(new CycleEditViewModel()));
    }

}


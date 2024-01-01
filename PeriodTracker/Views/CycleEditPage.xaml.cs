using PeriodTracker.ViewModels;

namespace PeriodTracker;

public partial class CycleEditPage : ContentPage
{
	public CycleEditPage(CycleEditViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
	}

    private async void OnCancelClicked(object sender, EventArgs e){
        await Navigation.PopModalAsync();
    }

    private async void OnSaveClicked(object sender, EventArgs e){
        if (await ((CycleEditViewModel)BindingContext).Save())
            await Navigation.PopModalAsync();
    }
}
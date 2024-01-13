using PeriodTracker.ViewModels;

namespace PeriodTracker;

public partial class AboutPage : ContentPage
{
	public AboutPage(AboutViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
	}

    private async void OnViewAttributionsTapped(object sender, EventArgs e) =>
        await Navigation.PushAsync(new AttributionsPage());
}
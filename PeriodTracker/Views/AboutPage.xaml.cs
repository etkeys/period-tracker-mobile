using PeriodTracker.ViewModels;

namespace PeriodTracker;

public partial class AboutPage : ContentPage
{
	public AboutPage(AboutViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
	}
}
using PeriodTracker.ViewModels;

namespace PeriodTracker;

public partial class MainPage : ContentPage
{
	public MainPage(MainViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
	}

}


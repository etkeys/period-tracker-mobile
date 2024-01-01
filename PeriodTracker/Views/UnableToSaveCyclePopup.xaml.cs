using CommunityToolkit.Maui.Views;
using PeriodTracker.ViewModels;

namespace PeriodTracker;

public partial class UnableToSaveCyclePopup : Popup
{
	public UnableToSaveCyclePopup(UnableToSaveCyclePopupViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
	}

    private void OnOkClicked(object? sender, EventArgs e) => Close();
}
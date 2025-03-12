using PeriodTracker.ViewModels;

namespace PeriodTracker;

public partial class ImportExportPage : ContentPage
{
	public ImportExportPage(ImportExportViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
	}

    private async void OnExportClicked(object sender, EventArgs e)
    {
        await ((ImportExportViewModel)BindingContext).ExportData();
    }

    private async void OnImportClicked(object sender, EventArgs e)
    {
        await ((ImportExportViewModel)BindingContext).ImportData();
    }
}
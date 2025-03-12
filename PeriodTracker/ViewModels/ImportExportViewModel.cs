using System.Diagnostics;
using CommunityToolkit.Maui.Storage;
using PeriodTracker.Services;

namespace PeriodTracker.ViewModels;

public partial class ImportExportViewModel: ViewModelBase
{
    private readonly IAppInfo _appInfo;
    private readonly IAlertService _alertService;
    private readonly IDbContextProvider _dbProvider;
    private readonly IFilePicker _filePicker;
    private readonly IFolderPicker _folderPicker;
    private readonly IImportExportService _importExportService;
    public ImportExportViewModel(IServiceProvider services)
    {
        _appInfo = services.GetRequiredService<IAppInfo>();
        _alertService = services.GetRequiredService<IAlertService>();
        _dbProvider = services.GetRequiredService<IDbContextProvider>();
        _importExportService = services.GetRequiredService<IImportExportService>();
        _filePicker = services.GetRequiredService<IFilePicker>();
        _folderPicker = services.GetRequiredService<IFolderPicker>();
    }

    public async Task ExportData()
    {
        try
        {
            IsBusy = true;

            using var db = await _dbProvider.GetContext();
            var payload = await _importExportService.GetDataForExport(db, _appInfo.Version);

#pragma warning disable CA1416 //This call site is reachable on all platforms.
            var folder = await _folderPicker.PickAsync();
            folder.EnsureSuccess();
#pragma warning restore CA1416

            var outFile = Path.Combine(
                folder.Folder.Path,
                $"{_appInfo.Name} export_{DateTime.Now:yyyy-MM-dd HH-mm-ss}.json");

            File.WriteAllText(outFile, payload);

            await _alertService.ShowToastAsync("File saved successfully.");
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Error during export: {ex}");
            await _alertService.ShowAlertAsync("Error", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public async Task ImportData()
    {
        try
        {
            if (! await UserConfirmsImport())
                return;

            IsBusy = true;

            var file = await _filePicker.PickAsync();
            if (file == null || string.IsNullOrWhiteSpace(file.FullPath))
                return;

            using var stream = await file.OpenReadAsync();
            using var reader = new StreamReader(stream);
            var payload = reader.ReadToEnd();

            using var db = await _dbProvider.GetContext();

            await _importExportService.ImportData(db, _appInfo.Version, payload);

            await _alertService.ShowToastAsync("File imported successfully.");

            await EventBus.BroadcastEvent(EventBusBroadcastedEvent.CyclesUpdated);
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Error during export: {ex}");
            await _alertService.ShowAlertAsync("Error", ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<bool> UserConfirmsImport()
    {
        var userConfirmation = await _alertService.ShowConfirmationAsync(
            "WARNING: Data Loss",
            "You are about to delete all current data and replace it with data from a different install. " +
            "Do you want to continue?");
        if (!userConfirmation)
            return false;

        // second confirmation, just to be sure
        userConfirmation = await _alertService.ShowConfirmationAsync(
            "WARNING: Data Loss (Re-Confirm)",
            "Seriously, all data that is currently in the app will be gone, replaced with the data you will "+
                "be asked to select to load. " +
            "Are you really sure you want to do this?");
        if (!userConfirmation)
            return false;

        return true;
    }
}
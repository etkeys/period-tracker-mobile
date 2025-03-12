using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace PeriodTracker.Services;

public interface IImportExportService
{
    Task<string> GetDataForExport(AppDbContext db, Version appVersion);

    Task ImportData(AppDbContext db, Version appVersion, string dataToImport);
}

public class ImportExportService: IImportExportService
{
    public async Task<string> GetDataForExport(AppDbContext db, Version appVersion)
    {
        try
        {
            var data = new ImportExportData(
                AppVersion: $"{appVersion:3}",
                AppStates: await db.AppState.ToListAsync(),
                Cycles: await db.Cycles.ToListAsync()
            );

            var dataString = JsonSerializer.Serialize(data);
            Debug.WriteLine(dataString);

            return dataString;
        }
        catch(Exception ex)
        {
            throw new Exception("Failed to create data extract payload.", ex);
        }
    }

    public async Task ImportData(AppDbContext db, Version appVersion, string dataToImport)
    {
        try
        {
            var payload = JsonSerializer.Deserialize<ImportExportData>(dataToImport);
            if (payload?.AppVersion is null)
                throw new FileFormatException($"\"{nameof(ImportExportData.AppVersion)}\" property not found.");

            var payloadVersion = new Version(payload.AppVersion);
            if (payloadVersion != appVersion)
            {
                var tip = payloadVersion < appVersion
                    ? "You must upgrade the app you exported from, then repeat the export step."
                    : "You must upgrade this app to the same version, then retry the import.";

                throw new InvalidOperationException($"Importing data from version \"{payloadVersion}\" is not allowed. {tip}");
            }

            // We want validate to happen after version check because an older version
            // might not have a table the newer version has. We don't want to error
            // when all the user needs to do is update the exporting app first.
            ImportValidatePayload(payload);

            using var trans = await db.Database.BeginTransactionAsync();
            await db.ClearAllTables();

            await db.AppState.AddRangeAsync(payload.AppStates);
            await db.Cycles.AddRangeAsync(payload.Cycles);

            await db.SaveChangesAsync();
            await trans.CommitAsync();
        }
        catch(Exception ex)
        {
            throw new Exception("Failed to import data.", ex);
        }
    }

    private void ImportValidatePayload(ImportExportData payload)
    {
        const string tableMissingMessage = "Missing expected table: {0}.";

        if (payload.AppStates is null)
            throw new FileFormatException(string.Format(
                tableMissingMessage,
                nameof(ImportExportData.AppStates)));
        if (payload.Cycles is null)
            throw new FileFormatException(string.Format(
                tableMissingMessage,
                nameof(ImportExportData.Cycles)));

        var payloadAppStates = payload.AppStates.Select(i => i.AppStatePropertyId).ToHashSet();
        foreach(var item in Enum.GetValues<AppStateProperty>().Where(a => a != AppStateProperty.Unknown))
        {
            if (!payloadAppStates.Contains(item))
                throw new InvalidDataException($"Missing AppState property \"{item}\".");
        }
    }

    public record ImportExportData(
        string AppVersion,
        IEnumerable<AppState> AppStates,
        IEnumerable<Cycle> Cycles);
}
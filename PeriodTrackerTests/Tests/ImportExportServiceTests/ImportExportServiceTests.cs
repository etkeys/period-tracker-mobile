
using Microsoft.EntityFrameworkCore;
using PeriodTracker;
using PeriodTracker.Services;

namespace PeriodTrackerTests;

public partial class ImportExportServiceTests: BaseTest, IClassFixture<TemporaryDirectoryFixture>
{
    private readonly TemporaryDirectoryFixture _tempDir;

    public ImportExportServiceTests(TemporaryDirectoryFixture tempDirFixture){
        _tempDir = tempDirFixture;
    }

    [Theory, ClassData(typeof(GetDataForExportTestData))]
    public async Task GetDataForExportTests(TestCase<GetDataForExportTestData.TestParameters> t)
    {
        var testTempDir = _tempDir.CreateTestCaseDirectory(t.Name);

        await SetupDatabase(testTempDir, t.Parameters.Inputs.GetSeedData());

        var actor = new ImportExportService();
        using var db = new AppDbContext(CreateDbContextOptions(testTempDir), true);

        var actPayload = await actor.GetDataForExport(db, t.Parameters.Inputs.AppVersion);

        Assert.Equal(t.Parameters.Expected.Payload, actPayload);
    }

    [Theory, ClassData(typeof(ImportDataTestData))]
    public async Task ImportDataTests(TestCase<ImportDataTestData.TestParameters> t)
    {
        var testTempDir = _tempDir.CreateTestCaseDirectory(t.Name);

        await SetupDatabase(testTempDir, t.Parameters.Inputs.GetSeedData());

        using var db = new AppDbContext(CreateDbContextOptions(testTempDir), true);

        try
        {
            var actor = new ImportExportService();
            await actor.ImportData(db, t.Parameters.Inputs.AppVersion, t.Parameters.Inputs.Payload);
        }
        catch (Exception ex)
        {
            // Got an exception but it wasn't expected
            if (t.Parameters.Expected.Exception is null)
                throw;

            VerifyException(t.Parameters.Expected.Exception, ex);
        }

        await VerifyTableAppStates(db, t.Parameters.Expected.AppStates);
        await VerifyTableCycles(db, t.Parameters.Expected.Cycles);
    }

    private static async Task VerifyTableAppStates(AppDbContext db, List<AppState> expected)
    {
        var actual = await db.AppState.OrderBy(i => i.AppStatePropertyId).ToListAsync();
        Assert.Equal(expected.Count, actual.Count);

        var zipAppStatesExpAct = expected.OrderBy(i => i.AppStatePropertyId).Zip(actual).ToList();
        Assert.All(zipAppStatesExpAct, pair => {
            var (exp, act) = pair;
            Assert.Equal(exp.AppStatePropertyId, act.AppStatePropertyId);
            Assert.Equal(exp.Value, act.Value);
        });
    }

    private static async Task VerifyTableCycles(AppDbContext db, List<Cycle> expected)
    {
        var actual = await db.Cycles.OrderBy(i => i.StartDate).ToListAsync();
        Assert.Equal(expected.Count, actual.Count);

        var zipCyclesExpAct = expected.OrderBy(i => i.StartDate).Zip(actual).ToList();
        Assert.All(zipCyclesExpAct, pair => {
            var (exp, act) = pair;
            Assert.Equal(exp.StartDate, act.StartDate);
            Assert.Equal(exp.RecordedDate, act.RecordedDate);
        });
    }

}
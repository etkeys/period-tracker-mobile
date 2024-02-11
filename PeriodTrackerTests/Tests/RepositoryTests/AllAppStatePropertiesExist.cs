using PeriodTracker;

namespace PeriodTrackerTests;

public partial class RepositoryTests
{

    [Fact]
    public async Task AllAppStatePropertiesExist(){
        var testTempDir = _tempDir.CreateTestCaseDirectory();

        using var db = new AppDbContext(CreateDbContextOptions(testTempDir), true);

        var properties = Enum.GetValues<AppStateProperty>().Where(p => p > AppStateProperty.Unknown);

        // We just need to make sure that all of our properties are in the database
        // and that they can be parsed.
        foreach(var prop in properties){
            object value = prop switch {
                AppStateProperty.NotifyUpdateAvailableInterval => await db.GetAppStateValue(prop, Convert.ToInt32),
                AppStateProperty.NotifyUpdateAvailableNextDate => await db.GetAppStateValue(prop, Convert.ToDateTime),
                _ => throw new NotImplementedException($"App state property '{prop}' not handled.")
            };
        }

        Assert.True(true);
    }
}
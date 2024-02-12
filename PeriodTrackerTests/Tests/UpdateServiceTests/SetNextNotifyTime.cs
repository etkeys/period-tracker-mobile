using Moq;
using PeriodTracker;

namespace PeriodTrackerTests;

public partial class UpdateServiceTests
{

    [Theory, MemberData(nameof(SetNextNotifyTimeTestsData))]
    public async Task SetNextNotifyTimeTests(TestCase test){
        var testTempDir = _tempDir.CreateTestCaseDirectory(test.Name);

        await SetupDatabase(testTempDir, test.Setups);

        using (var db = new AppDbContext(CreateDbContextOptions(testTempDir), true)){
            _dbContextProviderMock.Setup(m => m.GetContext()).Returns(Task.Run(() => db));
            _httpClientFactoryMock.Setup(m => m.CreateClient(It.IsAny<string>())).Returns(new HttpClient());

            using var actor = new UpdateService(_httpClientFactoryMock.Object, _dbContextProviderMock.Object);
            await actor.SetNextNotifyTime();
            // actor should dispose of db
            await Assert.ThrowsAsync<ObjectDisposedException>(() =>
                db.GetAppStateValue(AppStateProperty.NotifyUpdateAvailableNextDate, Convert.ToDateTime));
        }

        using var db2 = new AppDbContext(CreateDbContextOptions(testTempDir), true);
        var actDate = await db2.GetAppStateValue(AppStateProperty.NotifyUpdateAvailableNextDate, Convert.ToDateTime);
        var expDate = (DateTime)test.Expected["date"]!;

        Assert.Equal(expDate, actDate);
    }

    public static IEnumerable<object[]> SetNextNotifyTimeTestsData => BundleTestCases(
        new TestCase("Interval is 0")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"AppState", new object[]{
                        new object[]{AppStateProperty.NotifyUpdateAvailableInterval, 0}
                    }}
                })
            .WithExpected("date", DateTime.UtcNow.Date)

        ,new TestCase("Interval is 1")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"AppState", new object[]{
                        new object[]{AppStateProperty.NotifyUpdateAvailableInterval, 1}
                    }}
                })
            .WithExpected("date", DateTime.UtcNow.AddDays(1).Date)

        ,new TestCase("Interval is 2")
            // No setup because database default is enough
            .WithExpected("date", DateTime.UtcNow.AddDays(2).Date)

        ,new TestCase("Interval is -1") //just for fun
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"AppState", new object[]{
                        new object[]{AppStateProperty.NotifyUpdateAvailableInterval, -1}
                    }}
                })
            .WithExpected("date", DateTime.UtcNow.AddDays(-1).Date)
        );
}
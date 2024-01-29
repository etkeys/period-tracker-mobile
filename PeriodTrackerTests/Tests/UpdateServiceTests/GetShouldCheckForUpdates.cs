
using Moq;
using PeriodTracker;

namespace PeriodTrackerTests;

public partial class UpdateServiceTests
{

    [Theory, MemberData(nameof(GetShouldCheckForUpdatesTestsData))]
    public async Task GetShouldCheckForUpdatesTests(TestCase test){
        var testTempDir = _tempDir.CreateTestCaseDirectory(test.Name);

        using var db = await SetupDb(testTempDir, test.Setups["database"]!);
        _dbContextProviderMock.Setup(m => m.GetContext()).Returns(Task.Run(() => db));
        _httpClientFactoryMock.Setup(m => m.CreateClient(It.IsAny<string>())).Returns(new HttpClient());

        using var actor = new UpdateService(_httpClientFactoryMock.Object, _dbContextProviderMock.Object);

        var act = await actor.GetShouldCheckForUpdates();

        var exp = (bool)test.Expected["result"]!;

        Assert.Equal(exp, act);
    }

    public static IEnumerable<object[]> GetShouldCheckForUpdatesTestsData => BundleTestCases(
        new TestCase("Time has elapsed")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"AppState", new object[]{
                        new object[] {AppStateProperty.NotifyUpdateAvailableNextDate, DateTime.Parse("2024-01-01")},}
                }})
            .WithExpected("result", true),

        new TestCase("Time has elapsed - is today")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"AppState", new object[]{
                        new object[] {AppStateProperty.NotifyUpdateAvailableNextDate, DateTime.UtcNow.Date},}
                }})
            .WithExpected("result", true),

        new TestCase("Time has not elapsed")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"AppState", new object[]{
                        new object[] {AppStateProperty.NotifyUpdateAvailableNextDate, DateTime.UtcNow.AddDays(1)},}
                }})
            .WithExpected("result", false)
        );
}
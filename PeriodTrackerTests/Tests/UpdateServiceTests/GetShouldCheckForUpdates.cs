using Moq;
using PeriodTracker;

namespace PeriodTrackerTests;

public partial class UpdateServiceTests
{

    [Theory, ClassData(typeof(GetShouldCheckForUpdatesTestsData))]
    public async Task GetShouldCheckForUpdatesTests(TestCase<GetShouldCheckForUpdatesTestsData.TestParameters> test){
        var testTempDir = _tempDir.CreateTestCaseDirectory(test.Name);

        await SetupDatabase(testTempDir, test.Parameters.Inputs);

        using var db = new AppDbContext(CreateDbContextOptions(testTempDir), true);

        _dbContextProviderMock.Setup(m => m.GetContext()).Returns(Task.Run(() => db));
        _httpClientFactoryMock.Setup(m => m.CreateClient(It.IsAny<string>())).Returns(new HttpClient());

        using var actor = new UpdateService(_httpClientFactoryMock.Object, _dbContextProviderMock.Object);

        var act = await actor.GetShouldCheckForUpdates();
        var exp = test.Parameters.Expected.Result;

        Assert.Equal(exp, act);
    }

    public class GetShouldCheckForUpdatesTestsData: IEnumerable<object[]>
    {
        public record TestParameters(Inputs Inputs, ExpectedResults Expected);
        private static IEnumerable<object[]> TestCases()
        {
            yield return new []{
                new TestCase<TestParameters>("Time has elapsed",
                    new TestParameters(
                        // No setup because the database default is enough.
                        new Inputs(),
                        new ExpectedResults{ Result = true }
                    ))};

            yield return new []{
                new TestCase<TestParameters>("Time has elapsed - is today",
                    new TestParameters(
                        new Inputs{
                            AppStates = new List<AppState>{
                                new (){
                                    AppStatePropertyId = AppStateProperty.NotifyUpdateAvailableNextDate,
                                    Value = DateTime.UtcNow.Date.ToString()
                                }}},
                        new ExpectedResults{ Result = true }
                    ))};

            yield return new []{
                new TestCase<TestParameters>("Time has not elapsed",
                    new TestParameters(
                        new Inputs{
                            AppStates = new List<AppState>{
                                new (){
                                    AppStatePropertyId = AppStateProperty.NotifyUpdateAvailableNextDate,
                                    Value = DateTime.UtcNow.AddDays(1).ToString()
                                }}},
                        new ExpectedResults{ Result = false }
                    ))};
        }

        public IEnumerator<object[]> GetEnumerator() => TestCases().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public class ExpectedResults
        {
            public bool Result { get; init; }
        }

        public class Inputs: ISeedDataProvider
        {
            public List<AppState> AppStates { get; init; } = new ();

            public SeedData GetSeedData() => new SeedData{
                AppStates = AppStates
            };
        }
    }
}
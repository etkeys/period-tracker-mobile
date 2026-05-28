using Moq;
using PeriodTracker;

namespace PeriodTrackerTests;

public partial class UpdateServiceTests
{

    [Theory, ClassData(typeof(SetNextNotifyTimeTestsData))]
    public async Task SetNextNotifyTimeTests(TestCase<SetNextNotifyTimeTestsData.TestParameters> test){
        var testTempDir = _tempDir.CreateTestCaseDirectory(test.Name);

        await SetupDatabase(testTempDir, test.Parameters.Inputs);

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
        var expDate = test.Parameters.Expected.Date;

        Assert.Equal(expDate, actDate);
    }

    public class SetNextNotifyTimeTestsData: IEnumerable<object[]>
    {
        public record TestParameters(Inputs Inputs, ExpectedResults Expected);

        private static IEnumerable<object[]> TestCases()
        {
            yield return new []{
                new TestCase<TestParameters>("Interval is 0",
                new TestParameters(
                    new Inputs{
                        AppStates = new List<AppState>{
                            new (){
                                AppStatePropertyId = AppStateProperty.NotifyUpdateAvailableInterval,
                                Value = "0"
                            }}},
                    new ExpectedResults{
                        Date = DateTime.UtcNow.Date
                    }
                ))};

            yield return new []{
                new TestCase<TestParameters>("Interval is 1",
                new TestParameters(
                    new Inputs{
                        AppStates = new List<AppState>{
                            new (){
                                AppStatePropertyId = AppStateProperty.NotifyUpdateAvailableInterval,
                                Value = "1"
                            }}},
                    new ExpectedResults{
                        Date = DateTime.UtcNow.AddDays(1).Date
                    }
                ))};

            yield return new []{
                new TestCase<TestParameters>("Interval is 2",
                new TestParameters(
                    // No setup because database default is enough
                    new Inputs(),
                    new ExpectedResults{
                        Date = DateTime.UtcNow.AddDays(2).Date
                    }
                ))};

            yield return new []{
                // just for fun
                new TestCase<TestParameters>("Interval is -1",
                new TestParameters(
                    new Inputs{
                        AppStates = new List<AppState>{
                            new (){
                                AppStatePropertyId = AppStateProperty.NotifyUpdateAvailableInterval,
                                Value = "-1"
                            }}},
                    new ExpectedResults{
                        Date = DateTime.UtcNow.AddDays(-1).Date
                    }
                ))};
        }

        public IEnumerator<object[]> GetEnumerator() => TestCases().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public class ExpectedResults
        {
            public DateTime Date { get; init; }
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
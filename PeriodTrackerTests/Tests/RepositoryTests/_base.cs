using Moq;
using PeriodTracker;

namespace PeriodTrackerTests;

public partial class RepositoryTests : IClassFixture<TemporaryDirectoryFixture>
{
    private readonly Mock<IDbInitializationInfo> _dbInitInfoMock = new(MockBehavior.Strict);
    private readonly TemporaryDirectoryFixture _tempDir;

    public RepositoryTests(TemporaryDirectoryFixture tempDirFixture){
        _tempDir = tempDirFixture;
    }

    private void AssertCycles(Cycle[]? expected, Cycle[]? actual){
        if (expected is null && actual is null) return;
        if (expected is null || actual is null)
            Assert.Fail("Either expected or actual are null");

        Assert.Equal(expected.Length, actual.Length);
        Assert.All(
            expected,
            exp => {
                var act = actual.First(a => a.Equals(exp));
                Assert.Equal(exp.RecordedDate, act.RecordedDate);
            }
        );
    }


    private async Task SetupDatabase(Repository db, Dictionary<string, object?> testSetups){
        if (!testSetups.TryGetValue("database", out var maybeTables)) return;

        var tables = maybeTables as Dictionary<string, object[]>;

        if (tables!.TryGetValue("cycles", out var cycles))
            foreach(var cycle in (Cycle[])cycles)
                await db.AddCycle(cycle);
    }

}
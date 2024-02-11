using Microsoft.EntityFrameworkCore;
using Moq;
using PeriodTracker;

namespace PeriodTrackerTests;

public partial class RepositoryTests : BaseTest, IClassFixture<TemporaryDirectoryFixture>
{
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

}
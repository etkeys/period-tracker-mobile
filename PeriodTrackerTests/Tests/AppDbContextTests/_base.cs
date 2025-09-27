using Microsoft.EntityFrameworkCore;
using Moq;
using PeriodTracker;

namespace PeriodTrackerTests;

public partial class AppDbContextTests : BaseTest, IClassFixture<TemporaryDirectoryFixture>
{
    private readonly TemporaryDirectoryFixture _tempDir;

    public AppDbContextTests(TemporaryDirectoryFixture tempDirFixture)
    {
        _tempDir = tempDirFixture;
    }

    private void AssertCycles(Cycle[]? expected, Cycle[]? actual)
    {
        if (expected is null && actual is null) return;
        if (expected is null || actual is null)
            Assert.Fail("Either expected or actual are null");

        Assert.Equal(expected.Length, actual.Length);
        Assert.All(
            expected,
            exp =>
            {
                var act = actual.First(a => a.Equals(exp));
                Assert.Equal(exp.RecordedDate, act.RecordedDate);
            }
        );
    }

    private void AssertCyclesHistory(CycleHistory[]? expected, CycleHistory[]? actual)
    {
        if (expected is null && actual is null) return;
        if (expected is null || actual is null)
            Assert.Fail("Either expected or actual are null");

        Assert.Equal(expected.Length, actual.Length);
        Assert.All(
            expected,
            exp =>
            {
                var act = actual.First(a => a.StartDate == exp.StartDate);
                Assert.Equal(exp.RecordedDate, act.RecordedDate);
                Assert.Equal(exp.CycleLengthDays, act.CycleLengthDays);
            }
        );
    }

}
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

    private void AssertCycles(List<Cycle> expected, List<Cycle> actual)
    {
        Assert.Equal(expected.Count, actual.Count);

        expected = expected.OrderBy(c => c.StartDate).ToList();
        actual = actual.OrderBy(c => c.StartDate).ToList();

        var zipExpAct = expected.Zip(actual);
        Assert.All(
            zipExpAct,
            expAct =>
            {
                var (exp, act) = expAct;
                Assert.Equal(exp.RecordedDate, act.RecordedDate);
            }
        );
    }

}
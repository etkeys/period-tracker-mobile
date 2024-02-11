
using Microsoft.EntityFrameworkCore;
using PeriodTracker;

namespace PeriodTrackerTests;

public partial class AppDbContextTests
{

    [Fact]
    public void EfInitShouldNotBreakNewInstalls(){
        var testTempDir = _tempDir.CreateTestCaseDirectory();

        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={Path.Combine(testTempDir.FullName, "_.db")}")
            .Options;
        using var db = new AppDbContext(contextOptions);
        db.Database.Migrate();

        Assert.True(db.AppState.Count() >= 2);
        Assert.Equal(0, db.Cycles.Count());
    }
}
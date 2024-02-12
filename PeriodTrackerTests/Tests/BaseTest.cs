using Microsoft.EntityFrameworkCore;
using PeriodTracker;

namespace PeriodTrackerTests;

public class BaseTest
{

    protected static IEnumerable<object[]> BundleTestCases(params TestCase[] testCases) =>
        testCases.Select(tc => new object[]{tc});

    protected DbContextOptions<AppDbContext> CreateDbContextOptions(DirectoryInfo testTempDir) =>
        new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={Path.Combine(testTempDir.FullName, "_.db")}")
            .Options;

    protected async Task SetupDatabase(DirectoryInfo testTempDir, Dictionary<string, object?> testSetups){
        if (!testSetups.TryGetValue("database", out var maybeTables)) return;

        var tables = maybeTables as Dictionary<string, object[]>;

        using var db = new AppDbContext(CreateDbContextOptions(testTempDir), true);

        if (tables!.TryGetValue("Cycle", out var cyclesData))
            foreach(var cycle in (Cycle[])cyclesData)
                db.Cycles.Add(cycle);

        if (tables!.TryGetValue("AppState", out var appStateData))
            foreach(object[] row in appStateData){
                var dbItem = db.AppState.Where(r => r.AppStatePropertyId == (AppStateProperty)row[0]).First();
                dbItem.Value = row[1].ToString()!;
            }

        await db.SaveChangesAsync();
    }
}
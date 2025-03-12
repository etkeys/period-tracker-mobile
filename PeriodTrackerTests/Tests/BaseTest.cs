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

    protected async Task SetupDatabase(DirectoryInfo testTempDir, SeedData seedData)
    {
        using var db = new AppDbContext(CreateDbContextOptions(testTempDir), true);

        // AppState table is seeded by database initialization

        db.Cycles.AddRange(seedData.Cycles);

        await db.SaveChangesAsync();
    }

    protected void VerifyException(Exception expected, Exception actual)
    {
        var exp = expected;
        var act = actual;

        if (exp is null || act is null)
            Assert.Fail("Both expected and actual initial exceptions must be non-null.");

        while(exp is not null && act is not null)
        {
            Assert.IsType(exp.GetType(), act);
            Assert.Equal(exp.Message, act.Message, StringRegexEqualityComparer.Default);

            exp = exp.InnerException;
            act = act.InnerException;
        }
    }
}
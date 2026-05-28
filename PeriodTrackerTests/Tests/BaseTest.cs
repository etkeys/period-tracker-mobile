using Microsoft.EntityFrameworkCore;
using PeriodTracker;

namespace PeriodTrackerTests;

public class BaseTest
{
    protected DbContextOptions<AppDbContext> CreateDbContextOptions(DirectoryInfo testTempDir) =>
        new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={Path.Combine(testTempDir.FullName, "_.db")}")
            .Options;

    protected async Task SetupDatabase(DirectoryInfo testTempDir, ISeedDataProvider seedDataProvider)
    {
        var seedData = seedDataProvider.GetSeedData();

        using var db = new AppDbContext(CreateDbContextOptions(testTempDir), true);

        if (seedData.AppStates.Any())
        {
            // For AppStates seeding, we need to remove any existing entries
            // that we are going to seed.
            foreach(var item in seedData.AppStates)
            {
                await (
                    from a in db.AppState
                    where a.AppStatePropertyId == item.AppStatePropertyId
                    select a
                ).ExecuteDeleteAsync();

                db.AppState.Add(item);
            }
        }

        if (seedData.Cycles.Any())
        {
            db.Cycles.AddRange(seedData.Cycles);
        }

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
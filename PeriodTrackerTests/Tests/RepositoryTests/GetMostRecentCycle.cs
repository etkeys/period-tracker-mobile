
using PeriodTracker;

namespace PeriodTrackerTests;

public partial class RepositoryTests
{

    [Theory, MemberData(nameof(GetMostRecentCycleTestsData))]
    public async Task GetMostRecentCycleTests(TestCase test){
        var testTempDir = _tempDir.CreateTestCaseDirectory(test.Name);

        _dbInitInfoMock.Setup(p => p.Database)
            .Returns(new FileInfo(Path.Combine(testTempDir.FullName, "_.db")));

        using var db = await Repository.GetContext(_dbInitInfoMock.Object);

        await SetupDatabase(db, test.Setups);

        var actCycle = await db.GetMostRecentCycle();

        var expCycle = (Cycle?)test.Expected["cycle"];

        Assert.Equal(expCycle, actCycle);
    }

    public static IEnumerable<object[]> GetMostRecentCycleTestsData =>
        new TestCase[] {
            new TestCase("Single cycle")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"cycles", new []{
                        new Cycle{
                            RecordedDate = DateTime.Today,
                            StartDate = DateTime.Parse("2023-11-01"),
                        },
                    }}
                })
            .WithExpected("cycle", new Cycle{
                            RecordedDate = DateTime.Today,
                            StartDate = DateTime.Parse("2023-11-01"),
            }),

            new TestCase("Many cycles")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"cycles", new []{
                        new Cycle{
                            RecordedDate = DateTime.Today,
                            StartDate = DateTime.Parse("2023-11-01"),
                        },
                        new Cycle{
                            RecordedDate = DateTime.Today,
                            StartDate = DateTime.Parse("2023-12-01")
                        },
                    }}
                })
            .WithExpected("cycle", new Cycle{
                            RecordedDate = DateTime.Today,
                            StartDate = DateTime.Parse("2023-12-01"),
            }),

            new TestCase("Many cycles - inverted")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"cycles", new []{
                        new Cycle{
                            RecordedDate = DateTime.Today,
                            StartDate = DateTime.Parse("2023-12-01")
                        },
                        new Cycle{
                            RecordedDate = DateTime.Today,
                            StartDate = DateTime.Parse("2023-11-01"),
                        },
                    }}
                })
            .WithExpected("cycle", new Cycle{
                            RecordedDate = DateTime.Today,
                            StartDate = DateTime.Parse("2023-12-01"),
            }),

            new TestCase("No cycles")
            .WithExpected("cycle", null)

        }.Select(tc => new object[] {tc});
}
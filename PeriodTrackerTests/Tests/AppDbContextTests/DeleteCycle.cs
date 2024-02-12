using PeriodTracker;

namespace PeriodTrackerTests;

public partial class AppDbContextTests
{

    [Theory, MemberData(nameof(DeleteCycleTestsData))]
    public async Task DeleteCycleTests(TestCase test){
        var testTempDir = _tempDir.CreateTestCaseDirectory(test.Name);

        await SetupDatabase(testTempDir, test.Setups);

        using var db = new AppDbContext(CreateDbContextOptions(testTempDir), true);

        var toDelete = ((Cycle?)test.Inputs["cycle"])!;

        var actDeleteResult = await db.DeleteCycle(toDelete);
        var actCycles = (from c in db.Cycles select c).ToArray();

        var expCycles = (Cycle[]?)test.Expected["cycles"];
        var expDeleteResult = (bool?)test.Expected["delete result"];

        Assert.Equal(expDeleteResult, actDeleteResult);
        AssertCycles(expCycles, actCycles);
    }

    public static IEnumerable<object[]> DeleteCycleTestsData =>
        new []{
            new TestCase("Target exists")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"Cycle", new[]{
                        new Cycle{
                            RecordedDate = DateTime.Today,
                            StartDate = DateTime.Parse("2023-11-01"),
                            },
                        new Cycle{
                            RecordedDate = DateTime.Today,
                            StartDate = DateTime.Parse("2023-12-01")
                        }
                    }},
                })
            .WithInput("cycle", new Cycle {
                RecordedDate = DateTime.Today,
                StartDate = DateTime.Parse("2023-12-01")
            })
            .WithExpected("cycles", new[]{
                        new Cycle{
                            RecordedDate = DateTime.Today,
                            StartDate = DateTime.Parse("2023-11-01"),
                            },
            })
            .WithExpected("delete result", true),

            new TestCase("Target does not exist")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"Cycle", new[]{
                        new Cycle{
                            RecordedDate = DateTime.Today,
                            StartDate = DateTime.Parse("2023-11-01"),
                            },
                        new Cycle{
                            RecordedDate = DateTime.Today,
                            StartDate = DateTime.Parse("2023-12-01")
                        }
                    }},
                })
            .WithInput("cycle", new Cycle {
                RecordedDate = DateTime.Today,
                StartDate = DateTime.Parse("2023-10-02")
            })
            .WithExpected("cycles", new []{
                        new Cycle{
                            RecordedDate = DateTime.Today,
                            StartDate = DateTime.Parse("2023-11-01"),
                            },
                        new Cycle{
                            RecordedDate = DateTime.Today,
                            StartDate = DateTime.Parse("2023-12-01")
                        }
            })
            .WithExpected("delete result", false),

            new TestCase("Cycles are empty")
            .WithInput("cycle", new Cycle {
                RecordedDate = DateTime.Today,
                StartDate = DateTime.Parse("2023-10-02")
            })
            .WithExpected("cycles", new Cycle[]{})
            .WithExpected("delete result", false)

        }.Select(tc => new object[]{tc});

}

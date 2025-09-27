using PeriodTracker;

namespace PeriodTrackerTests;

public partial class AppDbContextTests
{

    [Theory, MemberData(nameof(GetCycleHistoryTestsData))]
    public async Task GetCycleHistoryTests(TestCase test)
    {
        var testTempDir = _tempDir.CreateTestCaseDirectory(test.Name);

        using var db = new AppDbContext(CreateDbContextOptions(testTempDir), true);

        var inp = ((Cycle[]?)test.Inputs["cycles"])!;

        foreach (var c in inp)
            await db.AddCycle(c);

        var act = await db.GetCycleHistory();

        var exp = (CycleHistory[]?)test.Expected["cycles"];

        AssertCyclesHistory(exp, act.ToArray());
    }

    public static IEnumerable<object[]> GetCycleHistoryTestsData =>
        new[] {
            new TestCase("No cycles")
            .WithInput("cycles", Array.Empty<Cycle>())
            .WithExpected("cycles", Array.Empty<CycleHistory>()),

            new TestCase("One cycle")
            .WithInput("cycles", new[] {
                new Cycle {
                    RecordedDate = DateTime.Today,
                    StartDate = DateTime.Parse("2023-11-01"),
                } })
            .WithExpected("cycles", new[] {
                new CycleHistory {
                    RecordedDate = DateTime.Today,
                    StartDate = DateTime.Parse("2023-11-01"),
                    CycleLengthDays = 0
                } }),

            new TestCase("Many cycles")
            .WithInput("cycles", new[] {
                new Cycle {
                    RecordedDate = DateTime.Today,
                    StartDate = DateTime.Parse("2023-10-01"),
                },
                new Cycle {
                    RecordedDate = DateTime.Today,
                    StartDate = DateTime.Parse("2023-11-01")
                },
                new Cycle {
                    RecordedDate = DateTime.Today,
                    StartDate = DateTime.Parse("2023-12-01")
                }
            })
            .WithExpected("cycles", new[] {
                new CycleHistory {
                    RecordedDate = DateTime.Today,
                    StartDate = DateTime.Parse("2023-12-01"),
                    CycleLengthDays = 30
                },
                new CycleHistory {
                    RecordedDate = DateTime.Today,
                    StartDate = DateTime.Parse("2023-11-01"),
                    CycleLengthDays = 31
                },
                new CycleHistory {
                    RecordedDate = DateTime.Today,
                    StartDate = DateTime.Parse("2023-10-01"),
                    CycleLengthDays = 0
                }
            })
        }
        .Select(tc => new object[] { tc });
}
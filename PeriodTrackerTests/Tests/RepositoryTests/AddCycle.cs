using PeriodTracker;

namespace PeriodTrackerTests;

public partial class RepositoryTests
{

    [Theory, MemberData(nameof(AddCycleTestsData))]
    public async Task AddCycleTests(TestCase test){
        var testTempDir = _tempDir.CreateTestCaseDirectory(test.Name);

        using var db = new AppDbContext(CreateDbContextOptions(testTempDir), true);

        var inp = ((Cycle[]?)test.Inputs["cycles"])!;

        var actInsertedResults = new bool[inp.Length];
        for(var i =0; i < inp.Length; i++)
            actInsertedResults[i] = await db.AddCycle(inp[i]);

        var actInserted = (from c in db.Cycles select c).ToArray();

        var expInserted = (Cycle[]?)test.Expected["cycles"];
        var expInsertedResults = (bool[]?)test.Expected["insert results"];

        Assert.Equal(expInsertedResults, actInsertedResults);
        AssertCycles(expInserted, actInserted);
    }

    public static IEnumerable<object[]> AddCycleTestsData =>
        new []{
            new TestCase("Add single")
            .WithInput("cycles", new []{
                    new Cycle{
                        RecordedDate = DateTime.Today,
                        StartDate = DateTime.Parse("2023-11-01"),
                        }})
            .WithExpected("cycles", new []{
                    new Cycle{
                        RecordedDate = DateTime.Today,
                        StartDate = DateTime.Parse("2023-11-01"),
                        }})
            .WithExpected("insert results", new[]{true}),

            new TestCase("Add many")
            .WithInput("cycles", new []{
                    new Cycle{
                        RecordedDate = DateTime.Today,
                        StartDate = DateTime.Parse("2023-11-01"),
                        },
                    new Cycle{
                        RecordedDate = DateTime.Today,
                        StartDate = DateTime.Parse("2023-12-01")
                    }
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
            .WithExpected("insert results", new[]{true, true}),

            new TestCase("Add many - inverted")
            .WithInput("cycles", new []{
                    new Cycle{
                        RecordedDate = DateTime.Today,
                        StartDate = DateTime.Parse("2023-12-01")
                        },
                    new Cycle{
                        RecordedDate = DateTime.Today,
                        StartDate = DateTime.Parse("2023-11-01"),
                        }
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
            .WithExpected("insert results", new[]{true, true}),

            new TestCase("Add many with same date")
            .WithInput("cycles", new []{
                    new Cycle{
                        RecordedDate = DateTime.Today,
                        StartDate = DateTime.Parse("2023-11-01"),
                        },
                    new Cycle{
                        RecordedDate = DateTime.Today,
                        StartDate = DateTime.Parse("2023-11-01")
                    }
                        })
            .WithExpected("cycles", new []{
                    new Cycle{
                        RecordedDate = DateTime.Today,
                        StartDate = DateTime.Parse("2023-11-01"),
                        },
                        })
            .WithExpected("insert results", new[]{true, false}),

        }
        .Select(tc => new object[] {tc});

}
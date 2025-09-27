using System.Collections;
using PeriodTracker;

namespace PeriodTrackerTests;

public partial class AppDbContextTests
{
    [Theory, ClassData(typeof(GetCycleHistoryTestData))]
    public async Task GetCycleHistoryTests(TestCase<GetCycleHistoryTestData.TestParameters> t)
    {
        var testTempDir = _tempDir.CreateTestCaseDirectory(t.Name);

        await SetupDatabase(testTempDir, t.Parameters.Inputs.GetSeedData());

        using var db = new AppDbContext(CreateDbContextOptions(testTempDir), true);

        var act = await db.GetCycleHistory();
        var exp = t.Parameters.Expected.Cycles;

        AssertCyclesHistory(exp, act);
    }

    public class GetCycleHistoryTestData : IEnumerable<object[]>
    {
        public record TestParameters(Inputs Inputs, ExpectedResults Expected);

        private readonly List<(string Name, TestParameters Parameters)> _testCases = [
            ("No cycles", NoCycles()),
            ("One cycle", OneCycle()),
            ("Many cycles", ManyCycles()),
        ];

        private static TestParameters NoCycles()
        {
            var inpCycles = new List<Cycle>();

            var expCycles = new List<CycleHistory>();

            return new TestParameters(
                new Inputs{
                    Cycles = inpCycles
                },
                new ExpectedResults{ Cycles = expCycles}
            );
        }

        private static TestParameters OneCycle()
        {
            var inpCycles = new List<Cycle>{
                new Cycle{
                    StartDate = DateTime.Parse("2023-11-01"),
                    RecordedDate = DateTime.Today
                }
            };

            var expCycles = new List<CycleHistory>{
                new CycleHistory{
                    StartDate = DateTime.Parse("2023-11-01"),
                    RecordedDate = DateTime.Today,
                    CycleLengthDays = 0
                }
            };

            return new TestParameters(
                new Inputs{
                    Cycles = inpCycles
                },
                new ExpectedResults{ Cycles = expCycles}
            );
        }

        private static TestParameters ManyCycles()
        {
            var inpCycles = new List<Cycle>{
                new Cycle{
                    StartDate = DateTime.Parse("2023-10-01"),
                    RecordedDate = DateTime.Today
                },
                new Cycle{
                    StartDate = DateTime.Parse("2023-11-01"),
                    RecordedDate = DateTime.Today
                },
                new Cycle{
                    StartDate = DateTime.Parse("2023-12-01"),
                    RecordedDate = DateTime.Today
                }
            };

            var expCycles = new List<CycleHistory>{
                new CycleHistory{
                    StartDate = DateTime.Parse("2023-12-01"),
                    RecordedDate = DateTime.Today,
                    CycleLengthDays = 30
                },
                new CycleHistory{
                    StartDate = DateTime.Parse("2023-11-01"),
                    RecordedDate = DateTime.Today,
                    CycleLengthDays = 31
                },
                new CycleHistory{
                    StartDate = DateTime.Parse("2023-10-01"),
                    RecordedDate = DateTime.Today,
                    CycleLengthDays = 0
                }
            };

            return new TestParameters(
                new Inputs{
                    Cycles = inpCycles
                },
                new ExpectedResults{ Cycles = expCycles}
            );
        }

        public IEnumerator<object[]> GetEnumerator() =>
            _testCases.Select(c => new object[] { new TestCase<TestParameters>(c.Name, c.Parameters) })
                .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public class ExpectedResults
        {
            public List<CycleHistory> Cycles { get; set; } = [];
        }

        public class Inputs
        {
            public List<Cycle> Cycles { get; set; } = [];

            public SeedData GetSeedData() => new() {
                Cycles = Cycles
            };
        }
    }
}
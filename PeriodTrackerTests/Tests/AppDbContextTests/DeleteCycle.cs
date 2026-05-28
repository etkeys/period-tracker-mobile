using Microsoft.EntityFrameworkCore;
using PeriodTracker;

namespace PeriodTrackerTests;

public partial class AppDbContextTests
{

    [Theory, ClassData(typeof(DeleteCycleTestsData))]
    public async Task DeleteCycleTests(TestCase<DeleteCycleTestsData.TestParameters> test){
        var testTempDir = _tempDir.CreateTestCaseDirectory(test.Name);

        await SetupDatabase(testTempDir, test.Parameters.Inputs);

        using var db = new AppDbContext(CreateDbContextOptions(testTempDir), true);

        var toDelete = test.Parameters.Inputs.Cycle;

        var actDeleteResult = await db.DeleteCycle(toDelete);
        var actCycles = await (from c in db.Cycles select c).ToListAsync();

        var expCycles = test.Parameters.Expected.Cycles;
        var expDeleteResult = test.Parameters.Expected.DeleteResult;

        Assert.Equal(expDeleteResult, actDeleteResult);
        AssertCycles(expCycles, actCycles);
    }

    public class DeleteCycleTestsData: IEnumerable<object[]>
    {
        public record TestParameters(Inputs Inputs, ExpectedResults Expected);

        public static IEnumerable<object[]> TestCases()
        {
            yield return new object[]{
                new TestCase<TestParameters>("Target exists",
                new TestParameters(
                    new Inputs{
                        Cycle = new Cycle {
                            RecordedDate = DateTime.Today,
                            StartDate = DateTime.Parse("2023-12-01")
                        },
                        Cycles = new List<Cycle>{
                            new (){
                                RecordedDate = DateTime.Today,
                                StartDate = DateTime.Parse("2023-11-01"),
                            },
                            new (){
                                RecordedDate = DateTime.Today,
                                StartDate = DateTime.Parse("2023-12-01"),
                            }
                        }},
                    new ExpectedResults{
                        Cycles = new List<Cycle>{
                            new (){
                                RecordedDate = DateTime.Today,
                                StartDate = DateTime.Parse("2023-11-01"),
                            }},
                        DeleteResult = true
                    }))};

            yield return new object[]{
                new TestCase<TestParameters>("Target does not exist",
                new TestParameters(
                    new Inputs{
                        Cycle = new Cycle {
                            RecordedDate = DateTime.Today,
                            StartDate = DateTime.Parse("2023-10-02")
                        },
                        Cycles = new List<Cycle>{
                            new (){
                                RecordedDate = DateTime.Today,
                                StartDate = DateTime.Parse("2023-11-01"),
                            },
                            new (){
                                RecordedDate = DateTime.Today,
                                StartDate = DateTime.Parse("2023-12-01"),
                            }
                        }},
                    new ExpectedResults{
                        Cycles = new List<Cycle>{
                            new (){
                                RecordedDate = DateTime.Today,
                                StartDate = DateTime.Parse("2023-11-01"),
                            },
                            new (){
                                RecordedDate = DateTime.Today,
                                StartDate = DateTime.Parse("2023-12-01"),
                            }
                        },
                        DeleteResult = false
                    }))};

            yield return new object[]{
                new TestCase<TestParameters>("Cycles are empty",
                new TestParameters(
                    new Inputs{
                        Cycle = new Cycle {
                            RecordedDate = DateTime.Today,
                            StartDate = DateTime.Parse("2023-10-02")
                        },
                        Cycles = new List<Cycle>()
                    },
                    new ExpectedResults{
                        Cycles = new List<Cycle>(),
                        DeleteResult = false
                    }))};

        }

        public IEnumerator<object[]> GetEnumerator() => TestCases().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public class ExpectedResults
        {
            public List<Cycle> Cycles { get; set; } = new();
            public bool DeleteResult { get; set; }
        }

        public class Inputs: ISeedDataProvider
        {
            public Cycle Cycle { get; set; } = new();
            public List<Cycle> Cycles { get; set; } = new();

            public SeedData GetSeedData() => new SeedData{
                Cycles = Cycles
            };
        }
    }
}

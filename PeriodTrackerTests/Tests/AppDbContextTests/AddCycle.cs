using Microsoft.EntityFrameworkCore;
using PeriodTracker;

namespace PeriodTrackerTests;

public partial class AppDbContextTests
{

    [Theory, ClassData(typeof(AddCycleTestsData))]
    public async Task AddCycleTests(TestCase<AddCycleTestsData.TestParameters> test){
        var testTempDir = _tempDir.CreateTestCaseDirectory(test.Name);

        using var db = new AppDbContext(CreateDbContextOptions(testTempDir), true);

        var inp = test.Parameters.Inputs.Cycles;

        var actInsertedResults = new bool[inp.Count];
        for(var i =0; i < inp.Count; i++)
            actInsertedResults[i] = await db.AddCycle(inp[i]);

        var actInserted = await (from c in db.Cycles select c).ToListAsync();

        var expInserted = test.Parameters.Expected.Cycles;
        var expInsertedResults = test.Parameters.Expected.InsertResults;

        Assert.Equal(expInsertedResults, actInsertedResults);
        AssertCycles(expInserted, actInserted);
    }

    public class AddCycleTestsData: IEnumerable<object[]>
    {
        public record TestParameters(Inputs Inputs, ExpectedResults Expected);
        public static IEnumerable<object[]> TestCases()
        {
            yield return new[] {
                new TestCase<TestParameters>("Add single",
                new TestParameters(
                    new Inputs{
                        Cycles = new List<Cycle>{
                            new (){
                                RecordedDate = DateTime.Today,
                                StartDate = DateTime.Parse("2023-11-01"),
                            }}},
                    new ExpectedResults{
                        Cycles = new List<Cycle>{
                            new (){
                                RecordedDate = DateTime.Today,
                                StartDate = DateTime.Parse("2023-11-01"),
                            }},
                        InsertResults = new List<bool>{true}
                    }))};

            yield return new[] {
                new TestCase<TestParameters>("Add many",
                new TestParameters(
                    new Inputs{
                        Cycles = new List<Cycle>{
                            new (){
                                RecordedDate = DateTime.Today,
                                StartDate = DateTime.Parse("2023-11-01"),
                            },
                            new (){
                                RecordedDate = DateTime.Today,
                                StartDate = DateTime.Parse("2023-12-01"),
                            }}},
                    new ExpectedResults{
                        Cycles = new List<Cycle>{
                            new (){
                                RecordedDate = DateTime.Today,
                                StartDate = DateTime.Parse("2023-11-01"),
                            },
                            new (){
                                RecordedDate = DateTime.Today,
                                StartDate = DateTime.Parse("2023-12-01"),
                            }},
                        InsertResults = new List<bool>{true, true}
                    }))};

            yield return new[] {
                new TestCase<TestParameters>("Add many - inverted",
                new TestParameters(
                    new Inputs{
                        Cycles = new List<Cycle>{
                            new (){
                                RecordedDate = DateTime.Today,
                                StartDate = DateTime.Parse("2023-12-01"),
                            },
                            new (){
                                RecordedDate = DateTime.Today,
                                StartDate = DateTime.Parse("2023-11-01"),
                            }}},
                    new ExpectedResults{
                        Cycles = new List<Cycle>{
                            new (){
                                RecordedDate = DateTime.Today,
                                StartDate = DateTime.Parse("2023-11-01"),
                            },
                            new (){
                                RecordedDate = DateTime.Today,
                                StartDate = DateTime.Parse("2023-12-01"),
                            }},
                        InsertResults = new List<bool>{true, true}
                    }))};

            yield return new[] {
                new TestCase<TestParameters>("Add many with same date",
                new TestParameters(
                    new Inputs{
                        Cycles = new List<Cycle>{
                            new (){
                                RecordedDate = DateTime.Today,
                                StartDate = DateTime.Parse("2023-11-01"),
                            },
                            new (){
                                RecordedDate = DateTime.Today,
                                StartDate = DateTime.Parse("2023-11-01"),
                            }}},
                    new ExpectedResults{
                        Cycles = new List<Cycle>{
                            new (){
                                RecordedDate = DateTime.Today,
                                StartDate = DateTime.Parse("2023-11-01"),
                            }},
                        InsertResults = new List<bool>{true, false}
                    }))};
        }

        public IEnumerator<object[]> GetEnumerator() => TestCases().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public class ExpectedResults
        {
            public List<bool> InsertResults {get; init;} = new();
            public List<Cycle> Cycles {get; init;} = new();
        }

        public class Inputs
        {
            public List<Cycle> Cycles {get; init;} = new();
        }
    }
}
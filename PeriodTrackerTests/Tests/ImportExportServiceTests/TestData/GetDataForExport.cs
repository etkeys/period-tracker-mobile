using System.Collections;
using System.Text.Json;
using PeriodTracker;
using PeriodTracker.Services;

namespace PeriodTrackerTests;

public partial class ImportExportServiceTests
{
    public class GetDataForExportTestData: IEnumerable<object[]>
    {
        private readonly List<(string Name, TestParameters Parameters)> _testCases = [
            ("Cycles table empty", CyclesTableEmpty()),
            ("Cycles table has many records", CyclesTableWithManyEntires()),
            ("Cycles table has one entry", CyclesTableWithOneEntry()),
        ];

        private static TestParameters CyclesTableEmpty()
        {
            var inpVersion = new Version(DefaultVersionString);
            var inpCycles = new List<Cycle>();

            var expPayload = GeneratePayload(new ImportExportService.ImportExportData(
                DefaultVersionString,
                DefaultAppStates,
                inpCycles));

            return new TestParameters(
                new Inputs{
                    AppVersion = inpVersion,
                    Cycles = inpCycles
                },
                new ExpectedResults{ Payload = expPayload}
            );
        }

        private static TestParameters CyclesTableWithManyEntires()
        {
            var inpVersion = new Version(DefaultVersionString);
            var inpCycles = new List<Cycle>{
                new Cycle{
                    // Have to this parse trick to remove the timezone offset
                    StartDate = DateTime.Parse(DateTime.Now.AddMonths(-3).Date.ToString("s")),
                    RecordedDate = DateTime.Parse(DateTime.Now.Date.ToString("s"))
                },
                new Cycle{
                    // Have to this parse trick to remove the timezone offset
                    StartDate = DateTime.Parse(DateTime.Now.AddMonths(-2).Date.ToString("s")),
                    RecordedDate = DateTime.Parse(DateTime.Now.Date.ToString("s"))
                },
                new Cycle{
                    // Have to this parse trick to remove the timezone offset
                    StartDate = DateTime.Parse(DateTime.Now.AddMonths(-1).Date.ToString("s")),
                    RecordedDate = DateTime.Parse(DateTime.Now.Date.ToString("s"))
                },
            };

            var expPayload = GeneratePayload(new ImportExportService.ImportExportData(
                DefaultVersionString,
                DefaultAppStates,
                inpCycles
            ));

            return new TestParameters(
                new Inputs{
                    AppVersion = inpVersion,
                    Cycles = inpCycles
                },
                new ExpectedResults { Payload = expPayload}
            );
        }

        private static TestParameters CyclesTableWithOneEntry()
        {
            var inpVersion = new Version(DefaultVersionString);
            var inpCycles = new List<Cycle>{
                new Cycle{
                    // Have to this parse trick to remove the timezone offset
                    StartDate = DateTime.Parse(DateTime.Now.AddMonths(-3).Date.ToString("s")),
                    RecordedDate = DateTime.Parse(DateTime.Now.Date.ToString("s"))
                }
            };

            var expPayload = GeneratePayload(new ImportExportService.ImportExportData(
                DefaultVersionString,
                DefaultAppStates,
                inpCycles
            ));

            return new TestParameters(
                new Inputs{
                    AppVersion = inpVersion,
                    Cycles = inpCycles
                },
                new ExpectedResults { Payload = expPayload}
            );
        }

        private static string GeneratePayload(ImportExportService.ImportExportData toSerialize) =>
            JsonSerializer.Serialize(toSerialize);

        private const string DefaultVersionString = "1.2.3";
        private static readonly List<AppState> DefaultAppStates = [..AppStateConfiguration.SeedData];

        public IEnumerator<object[]> GetEnumerator() =>
            _testCases.Select(c => new object[]{new TestCase<TestParameters>(c.Name, c.Parameters)})
                .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        public class ExpectedResults
        {
            public required string Payload {get; init;}
        }

        public class Inputs
        {
            public required Version AppVersion {get; init;}
            public required List<Cycle> Cycles {get; init;}

            public SeedData GetSeedData() => new() {
                Cycles = Cycles
            };
        }

        public record TestParameters(Inputs Inputs, ExpectedResults Expected);
    }
}
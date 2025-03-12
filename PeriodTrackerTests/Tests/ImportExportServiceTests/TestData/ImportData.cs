using System.Collections;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using PeriodTracker;
using PeriodTracker.Services;

namespace PeriodTrackerTests;

public partial class ImportExportServiceTests
{
    public class ImportDataTestData: IEnumerable<object[]>
    {
        private readonly List<(string Name, TestParameters Parameters)> _testCases =
            new (string, TestParameters)[] {
            ("Cycles has entries with same start date", CyclesHasEntriesWithSameStartDate()),
            ("Cycles has many entries", CyclesHasManyEntries()),
            ("Cycles has no entries", CyclesHasNoEntries()),
            ("Cycles has one entry", CyclesHasOneEntry()),
            ("Payload from newer version", PayloadFromNewerVersion()),
            ("Payload from older version", PayloadFromOlderVersion()),
            ("Payload is empty", PayloadIsEmpty()),
            ("Payload is missing AppVersion", PayloadIsMissingAppVersion()),
            ("Payload is not json", PayloadIsNotJson()),
        }
        .Concat(PayloadAppStatesMissingEntryCases())
        .Concat(PayloadMissingTableCases())
        .ToList();

        private static TestParameters CyclesHasEntriesWithSameStartDate()
        {
            var expAppStates = DefaultAppStates;

            var inpCycles = new List<Cycle> {
                new () {
                    StartDate = DateTime.Parse("2025-03-01"),
                    RecordedDate = DateTime.Now.Date
                },
                new () {
                    StartDate = DateTime.Parse("2025-03-01"),
                    RecordedDate = DateTime.Now.Date
                }
            };

            var inpPayload = GeneratePayload(new ImportExportService.ImportExportData(
                VersionStringDefault,
                expAppStates,
                inpCycles
            ));

            return new TestParameters(
                new Inputs {
                    AppVersion = new Version(VersionStringDefault),
                    Payload = inpPayload
                },
                new ExpectedResults {
                    AppStates = expAppStates,
                    Cycles = InitialCycles,
                    Exception = GenerateExpectedException(new InvalidOperationException(
                        @"The instance of entity.+cannot be tracked because another instance with the same key.+is already being tracked\."))
                }
            );

        }

        private static TestParameters CyclesHasManyEntries()
        {
            var expAppStates = DefaultAppStates;
            var expCycles = new List<Cycle> {
                new () {
                    StartDate = DateTime.Parse("2025-03-01"),
                    RecordedDate = DateTime.Now.Date
                },
                new () {
                    StartDate = DateTime.Parse("2025-02-02"),
                    RecordedDate = DateTime.Now.Date
                }
            };

            var inpPayload = GeneratePayload(new ImportExportService.ImportExportData(
                VersionStringDefault,
                expAppStates,
                expCycles
            ));

            return new TestParameters(
                new Inputs {
                    AppVersion = new Version(VersionStringDefault),
                    Payload = inpPayload
                },
                new ExpectedResults {
                    AppStates = expAppStates,
                    Cycles = expCycles
                }
            );
        }

        private static TestParameters CyclesHasNoEntries()
        {
            var expAppStates = DefaultAppStates;
            var expCycles = new List<Cycle>();

            var inpPayload = GeneratePayload(new ImportExportService.ImportExportData(
                VersionStringDefault,
                expAppStates,
                expCycles
            ));

            return new TestParameters(
                new Inputs{
                    AppVersion = new Version(VersionStringDefault),
                    Payload = inpPayload
                },
                new ExpectedResults {
                    AppStates = expAppStates,
                    Cycles = expCycles
                }
            );
        }

        private static TestParameters CyclesHasOneEntry()
        {
            var expAppStates = DefaultAppStates;
            var expCycles = new List<Cycle>{
                new () {
                    StartDate = DateTime.Parse("2025-03-01"),
                    RecordedDate = DateTime.Now.Date
                }
            };

            var inpPayload = GeneratePayload(new ImportExportService.ImportExportData(
                VersionStringDefault,
                expAppStates,
                expCycles
            ));

            return new TestParameters(
                new Inputs {
                    AppVersion = new Version(VersionStringDefault),
                    Payload = inpPayload
                },
                new ExpectedResults {
                    AppStates = expAppStates,
                    Cycles = expCycles
                }
            );
        }

        private static TestParameters PayloadFromNewerVersion()
        {
            var expAppStates = DefaultAppStates;
            var expCycles = InitialCycles;

            var inpPayload = GeneratePayload(new ImportExportService.ImportExportData(
                VersionStringDefault,
                expAppStates,
                expCycles
            ));

            return new TestParameters(
                new Inputs {
                    AppVersion = new Version(VersionStringOlder),
                    Payload = inpPayload
                },
                new ExpectedResults {
                    AppStates = expAppStates,
                    Cycles = expCycles,
                    Exception = GenerateExpectedException(new InvalidOperationException(
                        @"Importing data from version.+is not allowed\. You must upgrade this app to the same version"))
                }
            );
        }

        private static TestParameters PayloadFromOlderVersion()
        {
            var expAppStates = DefaultAppStates;
            var expCycles = InitialCycles;

            var inpPayload = GeneratePayload(new ImportExportService.ImportExportData(
                VersionStringOlder,
                expAppStates,
                expCycles
            ));

            return new TestParameters(
                new Inputs {
                    AppVersion = new Version(VersionStringDefault),
                    Payload = inpPayload
                },
                new ExpectedResults {
                    AppStates = expAppStates,
                    Cycles = expCycles,
                    Exception = GenerateExpectedException(new InvalidOperationException(
                        @"Importing data from version.+is not allowed\. You must upgrade the app you exported from"))
                }
            );
        }

        private static TestParameters PayloadIsEmpty()
        {
            return new TestParameters(
                new Inputs {
                    AppVersion = new Version(VersionStringDefault),
                    Payload = string.Empty
                },
                new ExpectedResults {
                    AppStates = DefaultAppStates,
                    Cycles = InitialCycles,
                    Exception = GenerateExpectedException(new JsonException(
                        "^The input does not contain any JSON"))
                }
            );
        }

        private static TestParameters PayloadIsMissingAppVersion()
        {
            return new TestParameters(
                new Inputs {
                    AppVersion = new Version(VersionStringDefault),
                    Payload = "{\"label\": \"a label\", \"text\": \"Pellentesque sit amet tortor ornare\"}"
                },
                new ExpectedResults {
                    AppStates = DefaultAppStates,
                    Cycles = InitialCycles,
                    Exception = GenerateExpectedException(new FileFormatException(
                        $"\"{nameof(ImportExportService.ImportExportData.AppVersion)}\" property not found."))
                }
            );
        }

        private static IEnumerable<(string, TestParameters)> PayloadAppStatesMissingEntryCases()
        {
            const string baseJson = $@"
            {{
                ""AppVersion"": ""{VersionStringDefault}"",
                ""AppStates"": [
                    @0
                ],
                ""Cycles"": [
                    {{""RecordedDate"": ""2025-03-01T00:00:00"", ""StartDate"": ""2025-01-15T00:00:00""}},
                    {{""RecordedDate"": ""2025-03-01T00:00:00"", ""StartDate"": ""2025-01-31T00:00:00""}},
                    {{""RecordedDate"": ""2025-03-01T00:00:00"", ""StartDate"": ""2025-02-15T00:00:00""}}
                ]
            }}";

            var allAppStates = Enum.GetValues<AppStateProperty>()
                .Where(a => a != AppStateProperty.Unknown)
                .ToArray();
            for (var a = 0; a < allAppStates.Length; a++)
            {
                var jsonBuilder = new StringBuilder();

                for (var i = 0; i < allAppStates.Length; i++)
                {
                    if (i == a)
                        continue;

                    jsonBuilder.AppendLine();
                    jsonBuilder.Append($"{{\"AppStatePropertyId\": {(int)allAppStates[i]},");

                    switch(allAppStates[i])
                    {
                        case AppStateProperty.NotifyUpdateAvailableInterval:
                            jsonBuilder.Append("\"Value\": \"3\"},");
                            break;
                        case AppStateProperty.NotifyUpdateAvailableNextDate:
                            jsonBuilder.Append("\"Value\": \"2021-01-01 00:00:00\"},");
                            break;
                        default:
                            throw new NotImplementedException($"AppStateProperty item {allAppStates[i]} generation not implemented.");
                    }
                }

                // Trim trailing comma from last element
                jsonBuilder.Length--;

                // var json = string.Format(baseJson, jsonBuilder.ToString());
                var json = baseJson.Replace("@0", jsonBuilder.ToString());

                yield return (
                    $"Payload AppStates missing item {allAppStates[a]}",
                    new TestParameters(
                    new Inputs {
                        AppVersion = new Version(VersionStringDefault),
                        Payload = json
                    },
                    new ExpectedResults {
                        AppStates = DefaultAppStates,
                        Cycles = InitialCycles,
                        Exception = GenerateExpectedException(new InvalidDataException(
                            $"Missing AppState property \"{allAppStates[a]}\"."
                        ))
                    }
                ));
            }
        }

        private static IEnumerable<(string, TestParameters)> PayloadMissingTableCases()
        {
            // Gets the T from DbSet<T> or the V from DbSet<V>
            var dbSetPropertyTypes =
                typeof(AppDbContext).GetProperties(
                    System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance)
                    .Where(p => {
                        var type = p.PropertyType;
                        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(DbSet<>);
                    })
                    .Select(p => p.PropertyType.GetGenericArguments().First());

            foreach(var propType in dbSetPropertyTypes)
            {
                var payloadBuilder = new StringBuilder();
                payloadBuilder.AppendLine($"{{\"AppVersion\": \"{VersionStringDefault}\",");

                var missingTableName = string.Empty;
                if (propType != typeof(AppState))
                {
                    payloadBuilder.AppendLine();
                    payloadBuilder.AppendLine("\"AppStates\": [");
                    payloadBuilder.AppendLine("{\"AppStatePropertyId\": 1, \"Value\": \"2\"},");
                    payloadBuilder.AppendLine("{\"AppStatePropertyId\": 2, \"Value\": \"2024-01-01 00:00:00\"}");
                    payloadBuilder.Append("],");
                }
                else
                    missingTableName = nameof(ImportExportService.ImportExportData.AppStates);

                if (propType != typeof(Cycle))
                {
                    payloadBuilder.AppendLine();
                    payloadBuilder.AppendLine("\"Cycles\": [");
                    payloadBuilder.AppendLine("{\"RecordedDate\": \"2025-03-01T00:00:00\", \"StartDate\": \"2025-01-15T00:00:00\"},");
                    payloadBuilder.AppendLine("{\"RecordedDate\": \"2025-03-01T00:00:00\", \"StartDate\": \"2025-01-31T00:00:00\"},");
                    payloadBuilder.AppendLine("{\"RecordedDate\": \"2025-03-01T00:00:00\", \"StartDate\": \"2025-02-15T00:00:00\"}");
                    payloadBuilder.Append("],");
                }
                else
                    missingTableName = nameof(ImportExportService.ImportExportData.Cycles);

                payloadBuilder.Length--;
                payloadBuilder.AppendLine();
                payloadBuilder.Append('}');

                yield return
                    ($"Payload missing table {missingTableName}",
                    new TestParameters(
                        new Inputs {
                            AppVersion = new Version(VersionStringDefault),
                            Payload = payloadBuilder.ToString()
                        },
                        new ExpectedResults {
                            AppStates = DefaultAppStates,
                            Cycles = InitialCycles,
                            Exception = GenerateExpectedException(new FileFormatException(
                                $"Missing expected table: {missingTableName}."))
                        }
                    ));
            }
        }

        private static TestParameters PayloadIsNotJson()
        {
            return new TestParameters(
                new Inputs {
                    AppVersion = new Version(VersionStringDefault),
                    Payload = "Ut odio lacus, lobortis in nibh vitae, porta porta odio."
                },
                new ExpectedResults {
                    AppStates = DefaultAppStates,
                    Cycles = InitialCycles,
                    Exception = GenerateExpectedException(new JsonException(
                        @"is an invalid start of a value\."))
                }
            );
        }

        private static Exception GenerateExpectedException(Exception innerException) =>
            new Exception("Failed to import data.", innerException);

        private static string GeneratePayload(ImportExportService.ImportExportData toSerialize) =>
            JsonSerializer.Serialize(toSerialize);

        private const string VersionStringDefault = "1.2.3";
        private const string VersionStringOlder = "1.0.0";
        private static readonly List<AppState> DefaultAppStates = [..AppStateConfiguration.SeedData];

        private static readonly List<Cycle> InitialCycles = new () {
                new Cycle{
                    StartDate = DateTime.Parse("2025-01-15"),
                    RecordedDate = DateTime.Now.Date
                },
                new Cycle{
                    StartDate = DateTime.Parse("2025-01-31"),
                    RecordedDate = DateTime.Now.Date
                },
                new Cycle{
                    StartDate = DateTime.Parse("2025-02-15"),
                    RecordedDate = DateTime.Now.Date
                }
        };

        public IEnumerator<object[]> GetEnumerator() =>
            _testCases.Select(c => new object[]{new TestCase<TestParameters>(c.Name, c.Parameters)})
                .GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public class ExpectedResults
        {
            public List<AppState> AppStates {get; init;} = new();
            public List<Cycle> Cycles {get; init;} = new();
            public Exception? Exception {get; init;} = null;
        }

        public class Inputs
        {
            public required Version AppVersion {get; init;}
            public required string Payload {get; init;}
            public List<Cycle> SeedCycles {get; init;} = InitialCycles;

            public SeedData GetSeedData() => new() {
                Cycles = SeedCycles
            };
        }

        public record TestParameters(Inputs Inputs, ExpectedResults Expected);

    }
}
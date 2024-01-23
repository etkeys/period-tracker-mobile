
using PeriodTracker;

namespace PeriodTrackerTests;

public partial class RepositoryTests
{

    [Theory, MemberData(nameof(SetAppStateTestsData))]
    public async Task SetAppStateTests(TestCase test){
        var testTempDir = _tempDir.CreateTestCaseDirectory(test.Name);

        _dbInitInfoMock.Setup(p => p.Database)
            .Returns(new FileInfo(Path.Combine(testTempDir.FullName, "_.db")));

        using var db = await Repository.GetContext(_dbInitInfoMock.Object);

        var inpProp = (AppStateProperty)test.Inputs["app state property"]!;
        var inpValue = test.Inputs["app state value"];

        var action = new Func<Task>(() => db.SetAppState(inpProp, inpValue!));

        if (test.Expected.TryGetValue("throws", out var expExType))
            await Assert.ThrowsAsync((Type)expExType!, action);
        else
        {
            await action.Invoke();

            var actValue = await db.GetAppState(inpProp);
            var propType = inpProp.GetDataTypeAttribute().Value;

            switch(propType){
                case Type _ when propType == typeof(DateTime): Assert.Equal(inpValue, Convert.ToDateTime(actValue)); break;
                case Type _ when propType == typeof(int): Assert.Equal(inpValue, Convert.ToInt32(actValue)); break;
                default: throw new InvalidCastException($"No conversion specificed for type {propType.Name}.");
            }
        }
    }

    public static IEnumerable<object[]> SetAppStateTestsData =>
        new []{
            new TestCase("Prop is DateTime, value is DateTime")
                .WithInput("app state property", AppStateProperty.NotifyUpdateAvailableNextDate)
                .WithInput("app state value", DateTime.Parse("2024-01-01")),

            new TestCase("Prop is int, value is int")
                .WithInput("app state property", AppStateProperty.NotifyUpdateAvailableInterval)
                .WithInput("app state value", 3),

            new TestCase("Prop is type A, value is type B")
                .WithInput("app state property", AppStateProperty.NotifyUpdateAvailableInterval)
                .WithInput("app state value", "3")
                .WithExpected("throws", typeof(ArgumentException)),

            new TestCase("Value is null")
                .WithInput("app state property", AppStateProperty.NotifyUpdateAvailableNextDate)
                .WithInput("app state value", null)
                .WithExpected("throws", typeof(ArgumentNullException)),

        }.Select(tc => new object[] {tc});

}
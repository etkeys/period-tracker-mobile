using PeriodTracker;

namespace PeriodTrackerTests;

public partial class RepositoryTests
{

    [Fact]
    public async Task AllAppStatePropertiesExist(){
        var testTempDir = _tempDir.CreateTestCaseDirectory();

        _dbInitInfoMock.Setup(p => p.Database)
            .Returns(new FileInfo(Path.Combine(testTempDir.FullName, "_.db")));

        using var db = await Repository.GetContext(_dbInitInfoMock.Object);
        foreach(var prop in Enum.GetValues<AppStateProperty>()){
            var value = await db.GetAppState(prop);
            var type = prop.GetDataTypeAttribute().Value;

            Assert.NotNull(value);
            try{
                switch(type){
                    case Type _ when type == typeof(bool): Convert.ToBoolean(value); break;
                    case Type _ when type == typeof(DateTime): Convert.ToDateTime(value); break;
                    case Type _ when type == typeof(int): Convert.ToInt32(value); break;
                    case Type _ when type == typeof(long): Convert.ToInt64(value); break;
                    case Type _ when type == typeof(string): break;
                    default: throw new InvalidCastException($"No conversion specificed for type {type.Name}.");
                }
            }
            catch (Exception ex){
                Assert.Fail($"Error when testing type of stored value for {prop}: {ex.Message}");
            }
        }
    }
}
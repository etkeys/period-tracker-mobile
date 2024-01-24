
using Moq;
using PeriodTracker;

namespace PeriodTrackerTests;

public partial class UpdateServiceTests : BaseTest, IClassFixture<TemporaryDirectoryFixture>
{
    private readonly Mock<IDbContextProvider> _dbContextProviderMock = new(MockBehavior.Strict);
    private readonly Mock<IDbInitializationInfo> _dbInitInfoMock = new(MockBehavior.Strict);
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new(MockBehavior.Strict);
    private readonly TemporaryDirectoryFixture _tempDir;

    public UpdateServiceTests(TemporaryDirectoryFixture tempDirFixture){
        _tempDir = tempDirFixture;
    }

    private async Task<Repository> SetupDb(DirectoryInfo testTempDir, object databaseSetup){
        _dbInitInfoMock.Setup(p => p.Database)
            .Returns(new FileInfo(Path.Combine(testTempDir.FullName, "_.db")));

        var result = await Repository.GetContext(_dbInitInfoMock.Object);

        var tables = databaseSetup as Dictionary<string, object[]>;

        if (tables!.TryGetValue("AppState", out var appStateData))
            foreach(object[] row in appStateData)
                await result.SetAppState((AppStateProperty)row[0], row[1]);

        return result;
    }


}
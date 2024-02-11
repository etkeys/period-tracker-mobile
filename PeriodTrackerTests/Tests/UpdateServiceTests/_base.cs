
using Moq;
using PeriodTracker;

namespace PeriodTrackerTests;

public partial class UpdateServiceTests : BaseTest, IClassFixture<TemporaryDirectoryFixture>
{
    private readonly Mock<IDbContextProvider> _dbContextProviderMock = new(MockBehavior.Strict);
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock = new(MockBehavior.Strict);
    private readonly TemporaryDirectoryFixture _tempDir;

    public UpdateServiceTests(TemporaryDirectoryFixture tempDirFixture){
        _tempDir = tempDirFixture;
    }

}
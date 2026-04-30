
using Microsoft.EntityFrameworkCore;
using Moq;
using PeriodTracker;
using PeriodTracker.Services;
using PeriodTracker.ViewModels;

namespace PeriodTrackerTests;

public class MainViewModelTests: BaseTest, IClassFixture<TemporaryDirectoryFixture>
{
    private readonly TemporaryDirectoryFixture _tempDir;

    public MainViewModelTests(TemporaryDirectoryFixture tempDirFixture){
        _tempDir = tempDirFixture;
    }

    [Theory]
    [InlineData("2026-04-16", "2026-04-16", "29", false)]
    [InlineData("2026-04-16", "2026-04-29", "16", false)]
    [InlineData("2026-04-16", "2026-05-13", "2", false)]
    [InlineData("2026-04-16", "2026-05-14", "1", false)]
    [InlineData("2026-04-16", "2026-05-15", "0", false)]
    [InlineData("2026-04-16", "2026-05-20", "0", true)]
    public async Task LoadAsync_CalculatesDaysUntilNextCycleCorrectly(
        string inpMostRecentCycleStartStr,
        string inpTodayStr,
        string expDaysUntilNextCycleText,
        bool expIsCycleStartOverdue)
    {
        var today = DateTime.Parse(inpTodayStr);
        var mockDateTimeService = new Mock<IDateTimeService>();
        mockDateTimeService.Setup(s => s.Today).Returns(today);

        var testTempDir = _tempDir.CreateTestCaseDirectory(inpTodayStr);
        var seedData = new SeedData {
            Cycles = new List<Cycle> {
                new Cycle { StartDate = DateTime.Parse(inpMostRecentCycleStartStr) }
            }
        };
        await SetupDatabase(testTempDir, seedData);
        var mockDbContextProvider = new Mock<IDbContextProvider>();
        mockDbContextProvider.Setup(p => p.GetContext())
            .ReturnsAsync(new AppDbContext(CreateDbContextOptions(testTempDir), true));

        var mockUpdateService = new Mock<IUpdateService>();
        mockUpdateService.Setup(s => s.GetShouldCheckForUpdates())
            .ReturnsAsync(false);
        var mockUpdateServiceProvider = new Mock<IUpdateServiceProvider>();
        mockUpdateServiceProvider.Setup(p => p.GetUpdateService())
            .Returns(mockUpdateService.Object);

        var mockAppInfo = new Mock<IAppInfo>();
        var mockAlertService = new Mock<IAlertService>();

        var actor = new MainViewModel(
            mockDbContextProvider.Object,
            mockDateTimeService.Object,
            mockUpdateServiceProvider.Object,
            mockAppInfo.Object,
            mockAlertService.Object);

        await actor.LoadAsync();

        Assert.Equal(expDaysUntilNextCycleText, actor.DaysUntilNextCycleText);
        Assert.Equal(expIsCycleStartOverdue, actor.IsCycleStartOverdue);
    }
}
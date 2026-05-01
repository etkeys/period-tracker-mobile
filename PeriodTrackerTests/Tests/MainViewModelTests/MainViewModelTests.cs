
using Microsoft.EntityFrameworkCore;
using Moq;
using PeriodTracker;
using PeriodTracker.Services;
using PeriodTracker.ViewModels;

namespace PeriodTrackerTests;

public class MainViewModelTests: BaseTest, IClassFixture<TemporaryDirectoryFixture>
{
    private readonly TemporaryDirectoryFixture _tempDir;

    private readonly Mock<IAlertService> _mockAlertService = new Mock<IAlertService>();
    private readonly Mock<IAppInfo> _mockAppInfo = new Mock<IAppInfo>();
    private readonly Mock<IDateTimeService> _mockDateTimeService = new Mock<IDateTimeService>();
    private readonly Mock<IDbContextProvider> _mockDbContextProvider = new Mock<IDbContextProvider>();
    private readonly Mock<IUpdateService> _mockUpdateService = new Mock<IUpdateService>();
    private readonly Mock<IUpdateServiceProvider> _mockUpdateServiceProvider = new Mock<IUpdateServiceProvider>();

    public MainViewModelTests(TemporaryDirectoryFixture tempDirFixture){
        _tempDir = tempDirFixture;
    }

    private void SetupMocks(DirectoryInfo testTempDir, DateTime today)
    {
        _mockDateTimeService.Setup(s => s.Today).Returns(today);

        _mockDbContextProvider.Setup(p => p.GetContext())
            .ReturnsAsync(new AppDbContext(CreateDbContextOptions(testTempDir), true));

        _mockUpdateServiceProvider.Setup(p => p.GetUpdateService())
            .Returns(_mockUpdateService.Object);
    }

    [Theory]
    [InlineData("2026-04-16", "2026-04-16", "2026-04-22", "Last day of your period should be in 6 days:")]
    [InlineData("2026-04-16", "2026-04-17", "2026-04-22", "Last day of your period should be in 5 days:")]
    [InlineData("2026-04-16", "2026-04-21", "2026-04-22", "Last day of your period should be in 1 day:")]
    [InlineData("2026-04-16", "2026-04-22", "", "Today should be the last day of your period.")]
    [InlineData("2026-04-16", "2026-04-23", "", "")]
    public async Task LoadAsync_CalculatePeriodEndCorrectly(
        string inpMostRecentCycleStartStr,
        string inpTodayStr,
        string expPeriodEndDateText,
        string expPeriodEndText)
    {
        var today = DateTime.Parse(inpTodayStr);

        var testTempDir = _tempDir.CreateTestCaseDirectory(inpTodayStr);
        var seedData = new SeedData {
            Cycles = new List<Cycle> {
                new Cycle { StartDate = DateTime.Parse(inpMostRecentCycleStartStr) }
            }
        };
        await SetupDatabase(testTempDir, seedData);

        SetupMocks(testTempDir, today);

        _mockUpdateService.Setup(s => s.GetShouldCheckForUpdates())
            .ReturnsAsync(false);

        var actor = new MainViewModel(
            _mockDbContextProvider.Object,
            _mockDateTimeService.Object,
            _mockUpdateServiceProvider.Object,
            _mockAppInfo.Object,
            _mockAlertService.Object);

        await actor.LoadAsync();

        expPeriodEndDateText = string.IsNullOrEmpty(expPeriodEndDateText)
            ? string.Empty
            : DateTime.Parse(expPeriodEndDateText).ToString("D");

        Assert.Equal(expPeriodEndDateText, actor.PeriodEndDateText);
        Assert.Equal(expPeriodEndText, actor.PeriodEndText);
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

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PeriodTracker;

namespace PeriodTrackerTests;

public partial class AppDbContextTests
{

    [Fact]
    public void EfInitShouldNotBreakExistingInstalls(){
        var testTempDir = _tempDir.CreateTestCaseDirectory();

        var dbFile = CreateLegacyDatabase(testTempDir);

        var contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={dbFile.FullName}")
            .Options;
        using var db = new AppDbContext(contextOptions);
        db.Database.Migrate();

        Assert.Equal(2, db.AppState.Count());

        Assert.Equal(2, db.Cycles.Count());
        Assert.Equal(
            1,
            (from c in db.Cycles
            where
                c.StartDate == DateTime.Parse("2023-12-03")
                && c.RecordedDate == DateTime.Parse("2024-01-02")
            select c)
            .Count()
        );
        Assert.Equal(
            1,
            (from c in db.Cycles
            where
                c.StartDate == DateTime.Parse("2024-01-04")
                && c.RecordedDate == DateTime.Parse("2024-01-04")
            select c)
            .Count()
        );
    }

    private FileInfo CreateLegacyDatabase(DirectoryInfo testTempDir){
        var queries = new []{
            @"CREATE TABLE AppState (
                PropertyName VARCHAR(255) PRIMARY KEY,
                PropertyVale VARCHAR(2000) NOT NULL
            )",

            "INSERT INTO AppState VALUES('NotifyUpdateAvailableInterval', 2)",
            "INSERT INTO AppState VALUES('NotifyUpdateAvailableNextDate', DATETIME('2024-02-11'))",

            @"CREATE TABLE Cycles (
                StartDate DATETIME PRIMARY KEY,
                RecordedDate DATETIME
            )",

            "INSERT INTO Cycles VALUES(DATETIME('2024-01-04'), DATETIME('2024-01-04'))",
            "INSERT INTO Cycles VALUES(DATETIME('2023-12-03'), DATETIME('2024-01-02'))",
        };

        var dbFile = new FileInfo(Path.Combine(testTempDir.FullName, "_.db"));

        using var conn = new SqliteConnection($"Data Source={dbFile.FullName}");
        conn.Open();
        using var cmd = conn.CreateCommand();

        foreach(var query in queries){
            cmd.CommandText = query;
            cmd.ExecuteNonQuery();
        }

        return dbFile;
    }

}
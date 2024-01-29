using System.Data.Common;

namespace PeriodTracker;

public class Migration_2401211956 : IDbMigration
{
    public long Id => Convert.ToInt64(nameof(Migration_2401211956).Split('_').Last());

    private readonly static string[] queries =
        [
            @"CREATE TABLE AppState (
                PropertyName VARCHAR(255) PRIMARY KEY,
                PropertyValue VARCHAR(2000) NOT NULL
            )",

            @$"INSERT INTO AppState VALUES('{nameof(AppStateProperty.NotifyUpdateAvailableInterval)}', 2)",

            @$"INSERT INTO AppState VALUES('{nameof(AppStateProperty.NotifyUpdateAvailableNextDate)}', DATETIME('2024-01-01'))"
        ];

    public Task Apply(DbConnection connection, DbTransaction transaction) =>
        Task.Run(() => {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;

            foreach(var query in queries){
                command.CommandText = query;
                command.ExecuteNonQuery();
            }
        });
}
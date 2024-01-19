
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace PeriodTracker;

public class Migration_2401011537 : IDbMigration
{

    public long Id => Convert.ToInt64(nameof(Migration_2401011537).Split('_').Last());

    private readonly static string[] queries = 
        [
            @"CREATE TABLE Migrations (
                Id CHAR(10) PRIMARY KEY
            )",

            @"CREATE TABLE Cycles (
                StartDate DATETIME PRIMARY KEY,
                RecordedDate DATETIME
            )",
        ];

    public Task Apply(DbConnection connection, DbTransaction transation) =>
        Task.Run(() => {
            using var command = connection.CreateCommand();
            command.Transaction = transation;

            foreach (var query in queries){
                command.CommandText = query;
                command.ExecuteNonQuery();
            }

        });
}
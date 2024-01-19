using System.Data.Common;

namespace PeriodTracker;

public class Migration_2401191624: IDbMigration
{
    public long Id => Convert.ToInt64(nameof(Migration_2401191624).Split('_').Last());

    private readonly static string[] queries = 
        [
            @"CREATE TABLE temp_Migrations AS
            SELECT *
            FROM Migrations",

            @"DROP TABLE Migrations",

            @"CREATE TABLE Migrations (
                Id BIGINT PRIMARY KEY
            )",

            @"INSERT INTO Migrations
            SELECT CAST(Id AS BIGINT)
            FROM temp_Migrations",

            @"DROP TABLE temp_Migrations"
        ];

    public Task Apply(DbConnection connection, DbTransaction transaction) =>
        Task.Run(() => {
            using var command = connection.CreateCommand();
            command.Transaction = transaction;

            foreach (var query in queries){
                command.CommandText = query;
                command.ExecuteNonQuery();
            }

        });

}

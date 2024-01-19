
using System.Data.Common;
using System.Reflection;
using Microsoft.Data.Sqlite;

namespace PeriodTracker;

public partial class Repository
{

    private Task AddAppliedMigration(SqliteTransaction trans, long migrationId) =>
        Task.Run(() => {
            using var command = _connection.CreateCommand();
            command.CommandText = "INSERT INTO Migrations VALUES ($id)";
            command.Parameters.AddWithValue("$id", migrationId);

            command.Transaction = trans;
            command.ExecuteNonQuery();
        });

    private Task<long> GetLastAppliedMigrations() =>
        Task.Run(() =>
        {
            using var command = _connection.CreateCommand();

            // First check if the migrations table actually exists
            command.CommandText =
                "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = 'Migrations'";
            var tableExists = command.ExecuteScalar() is not null;
            if (!tableExists) return 0;

            // Get a list of the migrations we've already applied
            command.CommandText = "SELECT MAX(Id) FROM Migrations";
            var lastMigration = Convert.ToInt64(command.ExecuteScalar());

            return lastMigration;
        });


    private async Task PerformMigrations(){
        var lastAppliedMigration = await GetLastAppliedMigrations();

        var migrationsToApply =
            from i in
                from t in Assembly.GetExecutingAssembly().GetTypes()
                where t.GetInterfaces().Contains(typeof(IDbMigration))
                select Activator.CreateInstance(t) as IDbMigration
            where i.Id > lastAppliedMigration
            orderby i.Id
            select i;

        using var trans = _connection.BeginTransaction();
        try{
            foreach(var migration in migrationsToApply){
                await migration.Apply(_connection, trans);
                await AddAppliedMigration(trans, migration.Id);
            }

            trans.Commit();
        }
        catch(Exception){
            trans.Rollback();
            throw;
        }
        
    }
}
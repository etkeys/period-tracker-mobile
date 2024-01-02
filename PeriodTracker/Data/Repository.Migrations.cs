
using System.Data.Common;
using System.Reflection;
using Microsoft.Data.Sqlite;

namespace PeriodTracker;

public partial class Repository : IDisposable
{

    private Task AddAppliedMigration(SqliteTransaction trans, string migrationId) =>
        Task.Run(() => {
            using var command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Migrations VALUES ($id)";
            command.Parameters.AddWithValue("$id", migrationId);

            command.Transaction = trans;
            command.ExecuteNonQuery();
        });

    private Task<HashSet<string>> GetAppliedMigrations() =>
        Task.Run(() =>
        {
            var result = new HashSet<string>();

            using var command = connection.CreateCommand();

            // First check if the migrations table actually exists
            command.CommandText =
                "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = 'Migrations'";
            var tableExists = command.ExecuteScalar() is not null;
            if (!tableExists) return result;

            // Get a list of the migrations we've already applied
            command.CommandText = "SELECT Id FROM Migrations";
            using var reader = command.ExecuteReader();
            while (reader.Read())
                result.Add((string)reader["Id"]);

            return result;
        });


    private async Task PerformMigrations(){
        var appliedMigrations = await GetAppliedMigrations();

        var possibleMigrations =
            from i in
                from t in Assembly.GetExecutingAssembly().GetTypes()
                where t.GetInterfaces().Contains(typeof(IDbMigration))
                select Activator.CreateInstance(t) as IDbMigration
            orderby i.Id
            select i;

        using var trans = connection.BeginTransaction();
        try{
            foreach(var migration in possibleMigrations){
                if (appliedMigrations.Contains(migration.Id))
                    continue;

                await migration.Apply(connection, trans);
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
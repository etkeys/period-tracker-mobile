using System.Collections;
using System.Collections.Immutable;
using Microsoft.Data.Sqlite;

namespace PeriodTracker;

public partial class Repository : IDisposable
{

    private readonly SqliteConnection connection;
    private bool disposed;
    private static bool hasBeenInitialized = false;


    private Repository() {
        connection = new SqliteConnection($"Data Source={DatabasePath}");
        connection.Open();
    }

    private static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, "app.db");

    public Task AddCycle(Cycle newCycle){
        CheckDisposed();

        return Task.Run(async () => {
            var alreadyExists =
                (from c in await GetCycles()
                where c.Equals(newCycle)
                select true)
                .Any();

            if (alreadyExists) return;

            using var trans = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = trans;

            try{
                command.CommandText = "INSERT INTO Cycles VALUES($startDate, $recordDate)";

                command.Parameters.AddWithValue("$startDate", newCycle.StartDate);
                command.Parameters.AddWithValue("$recordDate", newCycle.RecordedDate);

                command.ExecuteNonQuery();

                trans.Commit();
            }
            catch(Exception){
                trans.Rollback();
                throw;
            }
        });

    }

    public void CheckDisposed() =>
        ObjectDisposedException.ThrowIf(disposed, this);

    public void Dispose(){
        if (disposed) return;

        disposed = true;
    }

    public Task<bool> DeleteCycle(Cycle cycle) {
        CheckDisposed();

        return Task.Run(() => {
            using var trans = connection.BeginTransaction();
            using var command = connection.CreateCommand();
            command.Transaction = trans;

            try{
                command.CommandText = "DELETE FROM Cycles WHERE StartDate = $startDate";
                command.Parameters.AddWithValue("$startDate", cycle.StartDate);

                var rowsAffected = command.ExecuteNonQuery();

                trans.Commit();
                return rowsAffected > 0;
            }
            catch(Exception){
                trans.Rollback();
                throw;
            }
        });
    }

    public static Repository GetContext() => new();

    public Task<IEnumerable<Cycle>> GetCycles(){
        CheckDisposed();

        return Task.Run(() => {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT StartDate, RecordedDate FROM Cycles";

            var result = new LinkedList<Cycle>();

            using var reader = command.ExecuteReader();
            while(reader.Read()){
                result.AddLast(new Cycle(){
                    RecordedDate = Convert.ToDateTime(reader["RecordedDate"]),
                    StartDate = Convert.ToDateTime(reader["StartDate"]),
                });
            }

            return (IEnumerable<Cycle>) result;
        });
    }
    // =>
    //     Task.Run(() => (IEnumerable<Cycle>) cycles);

    public async Task<Cycle?> GetMostRecentCycle(){
        CheckDisposed();

        return
            (from c in await GetCycles()
            orderby c.StartDate descending
            select c)
            .FirstOrDefault();
    }
    // =>
    //     Task.Run(() => cycles.OrderByDescending(c => c.StartDate).FirstOrDefault());

    public static async Task Initialize() {
        if (hasBeenInitialized) return;

        using var db = GetContext();
        await db.PerformMigrations();

        hasBeenInitialized = true;
    }

}

using Microsoft.Data.Sqlite;

namespace PeriodTracker;

public partial class Repository : IDisposable
{

    private readonly SqliteConnection _connection;
    private bool _disposed;
    private readonly IDbInitializationInfo _initInfo;

    private Repository(IDbInitializationInfo initInfo){
        _initInfo = initInfo;

        _connection = new SqliteConnection($"Data Source={DatabasePath}");
        _connection.Open();
    }

    public string DatabasePath => _initInfo.Database.FullName;

    public Task<bool> AddCycle(Cycle newCycle){
        CheckDisposed();

        return Task.Run(() => {
            using var trans = _connection.BeginTransaction();
            try{
                var alreadyExists =
                    (from c in GetCycles(trans)
                    where c.Equals(newCycle)
                    select true)
                    .Any();

                if (alreadyExists) return false;

                using var command = _connection.CreateCommand();
                command.Transaction = trans;

                command.CommandText = "INSERT INTO Cycles VALUES($startDate, $recordDate)";

                command.Parameters.AddWithValue("$startDate", newCycle.StartDate);
                command.Parameters.AddWithValue("$recordDate", newCycle.RecordedDate);

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

    public void CheckDisposed() =>
        ObjectDisposedException.ThrowIf(_disposed, this);

    public void Dispose(){
        if (_disposed) return;

        if (_connection.State != System.Data.ConnectionState.Closed)
            _connection.Close();
        _connection.Dispose();

        _disposed = true;
    }

    public Task<bool> DeleteCycle(Cycle cycle) {
        CheckDisposed();

        return Task.Run(() => {
            using var trans = _connection.BeginTransaction();
            using var command = _connection.CreateCommand();
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

    public Task<string> GetAppState(AppStateProperty property){
        CheckDisposed();

        return Task.Run(() => {
            using var command = _connection.CreateCommand();

            command.CommandText = "SELECT PropertyValue FROM AppState WHERE PropertyName = $propName";
            command.Parameters.AddWithValue("$propName", property.ToString());

            return command.ExecuteScalar()?.ToString() ?? string.Empty;
        });
    }

    public static async Task<Repository> GetContext(IDbInitializationInfo initInfo) {
        var db = new Repository(initInfo);
        try{
            await db.PerformMigrations();
            return db;
        }
        catch (Exception ex)
        {
            db.Dispose();
            throw new Exception("Unable to initialize database.", ex);
        }
    }

    public Task<IEnumerable<Cycle>> GetCycles(){
        CheckDisposed();

        return Task.Run(() => {
            using var trans = _connection.BeginTransaction();
            try{
                var ret = GetCycles(trans);
                trans.Commit();
                return ret; 
            }
            catch(Exception) {
                trans.Rollback();
                throw;
            }
        });
    }

    private IEnumerable<Cycle> GetCycles(SqliteTransaction transaction){
            using var command = _connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = "SELECT StartDate, RecordedDate FROM Cycles";

            var result = new LinkedList<Cycle>();

            using var reader = command.ExecuteReader();
            while(reader.Read()){
                result.AddLast(new Cycle(){
                    RecordedDate = Convert.ToDateTime(reader["RecordedDate"]),
                    StartDate = Convert.ToDateTime(reader["StartDate"]),
                });
            }

            return result;
    }

    public async Task<Cycle?> GetMostRecentCycle(){
        CheckDisposed();

        return
            (from c in await GetCycles()
            orderby c.StartDate descending
            select c)
            .FirstOrDefault();
    }

    public Task SetAppState(AppStateProperty property, object value){
        CheckDisposed();

        if (value is null)
            throw new ArgumentNullException(
                nameof(value),
                "Expected some value, but was given NULL.");

        var type = property.GetDataTypeAttribute().Value;
        if (value.GetType() != type)
            throw new ArgumentException(
                nameof(value),
                $"Expected type {type.Name}, but got {value.GetType().Name}.");

        return Task.Run(() => {
            using var trans = _connection.BeginTransaction();
            using var command = _connection.CreateCommand();
            command.Transaction = trans;

            try{
                command.CommandText =
                    "UPDATE AppState SET PropertyValue = $propValue WHERE PropertyName = $propName";
                command.Parameters.AddWithValue("$propName", property.ToString());
                command.Parameters.AddWithValue("$propValue", value);

                command.ExecuteNonQuery();

                trans.Commit();
            }
            catch{
                trans.Rollback();
                throw;
            }
        });
    }

}

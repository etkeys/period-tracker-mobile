
using System.Data.Common;

namespace PeriodTracker;

public interface IDbMigration
{
    string Id {get;}
    Task Apply(DbConnection connection, DbTransaction transaction);
}

using System.Data.Common;

namespace PeriodTracker;

public interface IDbMigration
{
    long Id {get;}
    Task Apply(DbConnection connection, DbTransaction transaction);
}
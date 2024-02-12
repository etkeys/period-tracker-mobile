
using Microsoft.EntityFrameworkCore;

namespace PeriodTracker;

public partial class AppDbContext
{
    public async Task<T> GetAppStateValue<T>(
        AppStateProperty targetProperty,
        Func<string, T> converter
    ){
        var dbValue = await
            (from a in AppState
            where a.AppStatePropertyId == targetProperty
            select a.Value)
            .FirstAsync();

        return converter.Invoke(dbValue);
    }

}
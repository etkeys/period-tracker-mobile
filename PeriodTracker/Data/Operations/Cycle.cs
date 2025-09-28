using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace PeriodTracker;

public partial class AppDbContext
{
    public async Task<bool> AddCycle(Cycle newItem)
    {
        try
        {
            Cycles.Add(newItem);
            await SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            // TODO need better logging
            Debug.WriteLine(ex);
            return false;
        }
    }

    public async Task<bool> DeleteCycle(Cycle toDelete)
    {
        try
        {
            Cycles.Remove(toDelete);
            var ret = await SaveChangesAsync();
            return ret > 0;
        }
        catch (Exception ex)
        {
            // TODO need better logging
            Debug.WriteLine(ex);
            return false;
        }
    }

    public async Task<bool> DeleteCycle(CycleHistory toDelete)
    {
        try
        {
            var cycle = await Cycles.FirstOrDefaultAsync(c => c.StartDate == toDelete.StartDate);
            if (cycle is null) return false;

            return await DeleteCycle(cycle);
        }
        catch (Exception ex)
        {
            // TODO need better logging
            Debug.WriteLine(ex);
            return false;
        }
    }

    /// <summary>
    /// Gets cycle history, ordered by most recent first
    /// </summary>
    public async Task<List<CycleHistory>> GetCycleHistory()
    {
        var result = await (
            from c in CycleHistory
            orderby c.StartDate descending
            select c)
            .AsNoTracking()
            .ToListAsync();

        return result;
    }

}
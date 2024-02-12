using System.Diagnostics;

namespace PeriodTracker;

public partial class AppDbContext
{
    public async Task<bool> AddCycle(Cycle newItem){
        try{
            Cycles.Add(newItem);
            await SaveChangesAsync();
            return true;
        }
        catch(Exception ex){
            // TODO need better logging
            Debug.WriteLine(ex);
            return false;
        }
    }

    public async Task<bool> DeleteCycle(Cycle toDelete){
        try{
            Cycles.Remove(toDelete);
            var ret = await SaveChangesAsync();
            return ret > 0;
        }
        catch(Exception ex){
            // TODO need better logging
            Debug.WriteLine(ex);
            return false;
        }
    }

}
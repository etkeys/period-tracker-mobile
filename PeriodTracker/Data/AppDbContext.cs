using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace PeriodTracker;

// To create migrations run the following:
// dotnet ef migrations add "<migration name>" --framework net8.0

public partial class AppDbContext: DbContext
{
    public DbSet<AppState> AppState {get; set;} = null!;
    public DbSet<Cycle> Cycles {get; set;} = null!;

    public AppDbContext(DbContextOptions<AppDbContext> options): base(options) { }
}
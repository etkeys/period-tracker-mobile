using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PeriodTracker;

public class DbContextProviderFactory: IDesignTimeDbContextFactory<AppDbContext>
{
    public static IDbContextProvider Default =>
        new DbContextProvider(new FileInfo(Path.Combine(FileSystem.AppDataDirectory, "app.db")));

    public static IDbContextProvider Create(FileInfo databasePath) =>
        new DbContextProvider(databasePath);

    AppDbContext IDesignTimeDbContextFactory<AppDbContext>.CreateDbContext(string[] args){
        var dbPath = $"{Path.GetTempFileName()}.db";
        var builder = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={dbPath}");

        return new AppDbContext(builder.Options);
    }

    private class InitializationInfo(FileInfo databaseFile): IDbInitializationInfo
    {
        public FileInfo Database => databaseFile;
    }

    private class DbContextProvider(FileInfo databasePath) : IDbContextProvider
    {
        public Task<AppDbContext> GetContext(){
            var builder = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={databasePath.FullName}");

            return Task.Run(() => new AppDbContext(builder.Options));
        }
    }

}

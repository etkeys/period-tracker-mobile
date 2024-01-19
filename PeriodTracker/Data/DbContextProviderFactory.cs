namespace PeriodTracker;

public static class DbContextProviderFactory
{
    public static IDbContextProvider Default =>
        new DbContextProvider(new FileInfo(Path.Combine(FileSystem.AppDataDirectory, "app.db")));

    public static IDbContextProvider Create(FileInfo databasePath) =>
        new DbContextProvider(databasePath);

    private class InitializationInfo(FileInfo databaseFile): IDbInitializationInfo
    {
        public FileInfo Database => databaseFile;
    }

    private class DbContextProvider(FileInfo databasePath) : IDbContextProvider
    {
        public Task<Repository> GetContext(){
            var initInfo = new InitializationInfo(databasePath);
            return Repository.GetContext(initInfo);
        }
    }

}

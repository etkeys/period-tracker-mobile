

namespace PeriodTracker;

public interface IUpdateServiceProvider
{
    IUpdateService GetUpdateService();
}

public class UpdateServiceProvider: IUpdateServiceProvider
{
    private readonly IDbContextProvider _dbContextProvider;
    private readonly IHttpClientFactory _httpClientFactory;

    public UpdateServiceProvider(IHttpClientFactory httpClientFactory, IDbContextProvider dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
        _httpClientFactory = httpClientFactory;
    }

    public IUpdateService GetUpdateService()
    {
        return new UpdateService(_httpClientFactory, _dbContextProvider);
    }
}
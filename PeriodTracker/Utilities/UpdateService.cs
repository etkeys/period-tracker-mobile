using System.Diagnostics;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;

namespace PeriodTracker;

public interface IUpdateService: IDisposable
{

    Task<Version?> GetLatestVersion();
    Task<bool> GetShouldCheckForUpdates();
    Task SetNextNotifyTime();
}

public class UpdateService : IUpdateService
{
    private const string _releaseUrl =
        "https://api.github.com/repos/etkeys/period-tracker-mobile/releases/latest";

    private readonly IDbContextProvider _dbProvider;
    private bool _disposed;
    private readonly HttpClient _httpClient;

    public UpdateService(IHttpClientFactory httpClientFactory, IDbContextProvider dbContextProvider){
        _dbProvider = dbContextProvider;
        _httpClient = httpClientFactory.CreateClient();
    }

    public void Dispose(){
        if (_disposed) return;

        _httpClient.Dispose();

        _disposed = true;
    }

    public async Task<Version?> GetLatestVersion(){
        try{
            var request = new HttpRequestMessage(){
                RequestUri = new Uri(_releaseUrl),
                Method = HttpMethod.Get
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            request.Headers.Add("X-Github-Api-Version", "2022-11-28");

            var cs = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var response = await _httpClient.SendAsync(request, cs.Token);

            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadFromJsonAsync<JsonResponse>();
            if (json is null)
                throw new NullReferenceException("Failed to get json from response content.");

            return new Version(json.tag_name);
        }
        catch(Exception ex){
            Debug.WriteLine($"Attempting to get latest update resulted in exception: {ex}");
            return null;
        }
    }

    public async Task<bool> GetShouldCheckForUpdates(){
        try{
            using var db = await _dbProvider.GetContext();
            var nextNotifyDate = await db.GetAppStateValue(
                AppStateProperty.NotifyUpdateAvailableNextDate,
                Convert.ToDateTime);

            return nextNotifyDate <= DateTime.UtcNow.Date;
        }
        catch (Exception ex){
            Debug.WriteLine($"Attempting to determine if should check for update resulted in error: {ex}");
            // TODO need handling to surface failures
            return false;
        }
    }

    public async Task SetNextNotifyTime(){
        using var db = await _dbProvider.GetContext();

        var nextNotifyInterval = await db.GetAppStateValue(
            AppStateProperty.NotifyUpdateAvailableInterval,
            Convert.ToInt32);

        await db.AppState
            .Where(a => a.AppStatePropertyId == AppStateProperty.NotifyUpdateAvailableNextDate)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(
                    a => a.Value,
                    DateTime.UtcNow.AddDays(nextNotifyInterval).Date.ToString()));
    }

    private record JsonResponse(string tag_name);

}
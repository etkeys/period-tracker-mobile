using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using PeriodTracker.ViewModels;

namespace PeriodTracker;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
            .UseMauiCommunityToolkit()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<IAlertService, AlertService>();
        builder.Services.AddSingleton<AboutPage>();
        builder.Services.AddSingleton<AboutViewModel>();
        builder.Services.AddSingleton<IAppInfo>(_ => AppInfo.Current);
        builder.Services.AddSingleton<IDbContextProvider>(_ => DbContextProviderFactory.Default);
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<HistoryPage>();
        builder.Services.AddSingleton<HistoryViewModel>();

        var app = builder.Build();

        ServiceHelper.Initialize(app.Services);

		return app;
	}
}

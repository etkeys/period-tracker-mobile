using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PeriodTracker.Services;
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

        builder.Services.AddHttpClient();

        builder.Services.AddSingleton<IFilePicker>(FilePicker.Default);
        builder.Services.AddSingleton<IFolderPicker>(FolderPicker.Default);

        builder.Services.AddSingleton<IAlertService, AlertService>();
        builder.Services.AddSingleton<AboutPage>();
        builder.Services.AddSingleton<AboutViewModel>();
        builder.Services.AddSingleton<IAppInfo>(_ => AppInfo.Current);
        builder.Services.AddSingleton<IDbContextProvider>(_ => DbContextProviderFactory.Default);
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<HistoryPage>();
        builder.Services.AddSingleton<HistoryViewModel>();

        builder.Services.AddTransient<ImportExportPage>();
        builder.Services.AddTransient<ImportExportViewModel>(sp => new ImportExportViewModel(sp));
        builder.Services.AddTransient<IImportExportService, ImportExportService>();
        builder.Services.AddTransient<IUpdateService, UpdateService>();

        var app = builder.Build();

        ServiceHelper.Initialize(app.Services);

        using var db = app.Services.GetService<IDbContextProvider>()
            !.GetContext()
            .GetAwaiter()
            .GetResult();

        db.Database.Migrate();

		return app;
	}
}

namespace PeriodTracker;

// Taken from this SO article: Resolve a service registered with builder.Services
// https://stackoverflow.com/a/76439183
public class ServiceHelper
{

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public static IServiceProvider Services {get; private set;}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    public static void Initialize(IServiceProvider serviceProvider) =>
        Services = serviceProvider;

    /// <inheritdoc cref="IServiceProvider.GetService(Type)"/>
    public static T? GetService<T>() => Services.GetService<T>(); 

}

namespace PeriodTracker;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
    }

    public static Page AppMainPage => Current?.Windows?[0]?.Page!;

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    /// <summary>
    /// This is used to create a delay that can be awaited without blocking the
    /// UI thread. This is useful for simulating a loading state or giving the
    /// user feedback that an action is being processed.
    /// </summary>
    public static Task Delay(int milliseconds = 1000)
    {
        return Task.Delay(milliseconds);
    }
}

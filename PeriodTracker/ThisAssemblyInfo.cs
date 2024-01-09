
namespace PeriodTracker;

public partial class ThisAssemblyInfo(IAppInfo appInfo) : IAppInfo
{
    public string PackageName => appInfo.PackageName;

    public string Name => appInfo.Name;

    public string VersionString {
        get{
            var preRelease = string.IsNullOrEmpty(__PreReleaseMoniker) ? "" : $"-{__PreReleaseMoniker}.{__Revision.PadLeft(3,'0')}";
            return $"{Version:3}{preRelease} ({__GitCommitHash})";
        }
    }

    public Version Version => new (__Version);

    public string BuildString => __AndoidBuildNumber;

    public AppTheme RequestedTheme => appInfo.RequestedTheme;

    public AppPackagingModel PackagingModel => appInfo.PackagingModel;

    public LayoutDirection RequestedLayoutDirection => appInfo.RequestedLayoutDirection;

    public void ShowSettingsUI() => appInfo.ShowSettingsUI();
}

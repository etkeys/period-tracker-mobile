using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PeriodTracker.ViewModels;

public partial class AboutViewModel: ViewModelBase
{

    public AboutViewModel(IAppInfo appInfo){
        var commitHash = TryGetGitCommitHash();
        var commitHashText = commitHash.Successful ? $" ({commitHash.Value.Trim()})" : string.Empty;

        DisplayVersionText = $"{appInfo.Version:3}#{appInfo.BuildString}{commitHashText}";
    }

    [ObservableProperty]
    private string _displayVersionText = string.Empty;

    private (bool Successful, string Value) TryGetGitCommitHash(){
        try{
            var asm = typeof(AboutViewModel).Assembly;

            using var stream = asm.GetManifestResourceStream($"{asm.GetName().Name}.commit_hash_txt");
            using var reader = new StreamReader(stream!);
            return (true, reader.ReadToEnd());
        }
        catch(Exception ex){
            Debug.WriteLine(ex.ToString());
            return (false, string.Empty);
        }
    }

}

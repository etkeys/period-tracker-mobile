
using System.IO;
using System.Runtime.CompilerServices;

namespace PeriodTrackerTests;


public class TemporaryDirectoryFixture: IDisposable
{
    private bool _disposed;
    private static int _references = 0;
    private static readonly object _syncRoot = new();
    private static readonly DirectoryInfo _tempDirectory =
        new DirectoryInfo(Path.Combine(Path.GetTempPath(), "PeriodTrackerTests"));

    public TemporaryDirectoryFixture(){
        lock (_syncRoot){
            if (_references < 1){
                Cleanup(true);
                _references++;
            }
        }
    }

    private void CheckDisposed() =>
        ObjectDisposedException.ThrowIf(_disposed, this);

    private void Cleanup(bool recreate){
        if (_tempDirectory.Exists){
            _tempDirectory.Delete(true);
            _tempDirectory.Refresh();
        }

        if (!recreate) return;

        _tempDirectory.Create();
        _tempDirectory.Refresh();
    }

    public DirectoryInfo CreateTestCaseDirectory(
        string testCaseName,
        [CallerMemberName] string testMethodName = "",
        [CallerFilePath] string testCodeFilePath = ""
        )
        {
            var dirNames = new Stack<string>();
            dirNames.Push(testCaseName);
            dirNames.Push(testMethodName);

            var parentDir = new FileInfo(testCodeFilePath).Directory!;
            while(parentDir is not null && parentDir.Name != "Tests"){
                dirNames.Push(parentDir!.Name);
                parentDir = parentDir.Parent;
            }

            if (parentDir is null)
                throw new InvalidOperationException("Code file not in expected 'Tests' directory.");

            var subDirPath = Path.Combine(dirNames.ToArray());

            return _tempDirectory.CreateSubdirectory(subDirPath);
        }

    public void Dispose(){
        if (_disposed) return;

        lock(_syncRoot){
            if (--_references < 1)
                Cleanup(false);
        }

        _disposed = true;
    }

}

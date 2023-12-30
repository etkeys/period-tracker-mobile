using System.Collections.Immutable;

namespace PeriodTracker;

public class Repository : IDisposable
{

    private IEnumerable<Cycle>? cycles;
    private static ImmutableList<Cycle> cyclesInit =>
        [
            new(){
                RecordedDate = DateTime.Parse("9/29/2023"),
                StartDate = DateTime.Parse("9/29/2023"),
                },
            new(){
                RecordedDate = DateTime.Parse("10/31/2023"),
                StartDate = DateTime.Parse("10/30/2023")
                },
            new(){
                RecordedDate = DateTime.Parse("11/27/2023"),
                StartDate = DateTime.Parse("11/27/2023")
                },
        ];

    private bool disposed;

    private Repository() { }

    public void CheckDisposed() =>
        ObjectDisposedException.ThrowIf(disposed, this);

    public void Dispose(){
        if (disposed) return;

        disposed = true;
    }

    public static Repository GetContext() => new();

    public Task<IEnumerable<Cycle>> GetCycles() =>
        Task.Run(() => cycles ??= new List<Cycle>(cyclesInit));

}

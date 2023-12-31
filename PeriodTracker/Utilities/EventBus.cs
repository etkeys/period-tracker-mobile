namespace PeriodTracker;

public enum EventBusBroadcastedEvent
{
    CyclesUpdated
}

public static class EventBus
{
    private static LinkedList<IEventBusListener> listeners = new();

    public static Task BroadcastEvent(EventBusBroadcastedEvent @event) =>
        Task.Run(() => {
            foreach(var listener in listeners)
                listener.HandleEvent(@event);
        });


    public static void RegisterListener(IEventBusListener listener){
        if (!listeners.Contains(listener))
            listeners.AddLast(listener);
    }

}

public interface IEventBusListener
{
    void HandleEvent(EventBusBroadcastedEvent @event);
}

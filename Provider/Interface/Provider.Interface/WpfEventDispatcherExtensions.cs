using System.Windows.Threading;
using Connect.Provider.Message;

namespace Connect.Provider.Interfaces;

public static class WpfEventDispatcherExtensions
{
    /// <summary>Delivers to the specified WPF dispatcher, without relying on a global Application.</summary>
    public static IDisposable SubscribeOnDispatcher<TData>(
        this oldIEventDispatcher eventDispatcher,
        string receiver,
        Dispatcher dispatcher,
        Func<Message<TData>, CancellationToken, Task> handler)
    {
        ArgumentNullException.ThrowIfNull(eventDispatcher);
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(handler);
        return eventDispatcher.Subscribe<TData>(receiver, (message, token) =>
            dispatcher.InvokeAsync(() => handler(message, token), DispatcherPriority.Normal, token).Task.Unwrap());
    }
}

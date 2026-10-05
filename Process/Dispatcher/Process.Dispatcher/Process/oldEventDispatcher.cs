using Caliburn.Micro;

using Connect.Provider.Interfaces;
using Connect.Provider.Message;

namespace Connect.Process.Dispatcher;

/// <summary>One shared instance per active profile, owned by the bootstrapper.</summary>
public sealed class TEventDispatcher : IEventDispatcher
{
    private readonly EventAggregator _aggregator = new();
    private readonly object _gate = new();
    // Caliburn uses weak references; retain subscriptions until explicitly removed.
    private readonly HashSet<IDisposable> _subscriptions = [];
    private bool _disposed;

    public IDisposable Subscribe<TData> (string receiver,
        Func<Message<TData>, CancellationToken, Task> handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace (receiver);
        ArgumentNullException.ThrowIfNull (handler);
        lock (_gate) {
            ObjectDisposedException.ThrowIf (_disposed, this);
            var subscription = new Subscription<TData>(this, receiver, handler);
            _aggregator.Subscribe (subscription, callback => callback ());
            _subscriptions.Add (subscription);
            return subscription;
        }
    }

    public Task PublishAsync<TData> (Message<TData> message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull (message);
        lock (_gate) {
            ObjectDisposedException.ThrowIf (_disposed, this);
            return _aggregator.PublishAsync (message,
                callback => Task.Run (callback, cancellationToken), cancellationToken);
        }
    }

    public void Dispose ()
    {
        lock (_gate) {
            if (_disposed)
                return;
            _disposed = true;
            foreach (var subscription in _subscriptions.ToArray ())
                subscription.Dispose ();
        }
    }

    private void Remove (IDisposable subscription)
    {
        lock (_gate) {
            _aggregator.Unsubscribe (subscription);
            _subscriptions.Remove (subscription);
        }
    }

    private sealed class Subscription<TData> (TEventDispatcher owner, string receiver,
        Func<Message<TData>, CancellationToken, Task> handler) : IHandle<Message<TData>>, IDisposable
    {
        private int _disposed;

        public async Task HandleAsync (Message<TData> message, CancellationToken cancellationToken)
        {
            if (Volatile.Read (ref _disposed) != 0 ||
                (message.Receiver is not null && !StringComparer.Ordinal.Equals (message.Receiver, receiver)))
                return;

            cancellationToken.ThrowIfCancellationRequested ();
            // Each subscriber starts independently, even if another handler blocks
            // before its first await or throws synchronously.
            await Task.Run (() => InvokeAsync (message, cancellationToken), cancellationToken).ConfigureAwait (false);
        }

        private async Task InvokeAsync (Message<TData> message, CancellationToken cancellationToken)
        {
            if (Volatile.Read (ref _disposed) != 0)
                return;
            cancellationToken.ThrowIfCancellationRequested ();
            await handler (message, cancellationToken);
        }

        public void Dispose ()
        {
            if (Interlocked.Exchange (ref _disposed, 1) == 0)
                owner.Remove (this);
        }
    }
}

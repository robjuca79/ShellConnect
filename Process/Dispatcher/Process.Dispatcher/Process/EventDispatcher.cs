/*----------------------------------------------------------------
  Copyright (C) 2001 R&R Soft - All rights reserved.
  author: Roberto Oliveira Jucá    
----------------------------------------------------------------*/

namespace Connect.Process.Dispatcher;

//----- TEventDispatcher
public sealed class TEventDispatcher : IEventDispatcher
{
    #region Members
    public IDisposable Subscribe<TData> (UInternalOperationId receiver, Func<TMessageRecord<TData>, CancellationToken, Task> handler)
    {
        ArgumentNullException.ThrowIfNull (handler);

        lock (m_Gate) {
            ObjectDisposedException.ThrowIf (m_Disposed, this);

            var subscription = new Subscription<TData> (this, receiver, handler);

            m_Aggregator.Subscribe (subscription, callback => callback ());
            m_Subscriptions.Add (subscription);

            return subscription;
        }
    }

    public Task PublishAsync<TData> (TMessageRecord<TData> message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull (message);

        lock (m_Gate) {
            ObjectDisposedException.ThrowIf (m_Disposed, this);

            return m_Aggregator.PublishAsync (message, callback => Task.Run (callback, cancellationToken), cancellationToken);
        }
    }

    public void Dispose ()
    {
        lock (m_Gate) {
            if (m_Disposed) {
                return;
            }

            m_Disposed = true;

            foreach (var subscription in m_Subscriptions.ToArray ()) {
                subscription.Dispose ();
            }
        }
    }
    #endregion

    private sealed class Subscription<TData> (TEventDispatcher owner,
        UInternalOperationId receiver,
        Func<TMessageRecord<TData>, CancellationToken, Task> handler)
        : IHandle<TMessageRecord<TData>>, IDisposable
    {
        private int _disposed;

        public async Task HandleAsync (TMessageRecord<TData> message, CancellationToken cancellationToken)
        {
            if (Volatile.Read (ref _disposed) != 0 || message.Receiver != receiver)
                return;
            cancellationToken.ThrowIfCancellationRequested ();
            await Task.Run (async () =>
            {
                if (Volatile.Read (ref _disposed) != 0)
                    return;
                cancellationToken.ThrowIfCancellationRequested ();
                await handler (message, cancellationToken);
            }, cancellationToken).ConfigureAwait (false);
        }

        public void Remove (IDisposable subscription)
        {
            lock (m_Gate) {
                m_Aggregator.Unsubscribe (subscription);
                m_Subscriptions.Remove (subscription);
            }
        }

        public void Dispose ()
        {
            if (Interlocked.Exchange (ref _disposed, 1) == 0) {
                owner.Remove (this);
            }
        }
    }

    #region Fields
    readonly EventAggregator                        m_Aggregator = new ();
    readonly object                                 m_Gate = new ();
    readonly HashSet<IDisposable>                   m_Subscriptions = [];
    bool                                            m_Disposed;
    #endregion
};
//---------------------------//


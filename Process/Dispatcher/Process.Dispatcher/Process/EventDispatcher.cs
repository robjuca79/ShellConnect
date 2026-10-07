/*----------------------------------------------------------------
  Copyright (C) 2001 R&R Soft - All rights reserved.
  author: Roberto Oliveira Jucá    
----------------------------------------------------------------*/

namespace Connect.Process.Dispatcher;

//----- TEventDispatcher
[Export (typeof (IEventDispatcher))]
[PartCreationPolicy (CreationPolicy.Shared)]
public sealed class TEventDispatcher : IEventDispatcher
{
    #region Interface
    public IDisposable Subscribe (Func<IMessageRecord, Task> handler)
    {
        ArgumentNullException.ThrowIfNull (handler);

        lock (m_Gate) {
            ObjectDisposedException.ThrowIf (m_Disposed, this);

            var subscription = new TSubscription (this, handler);
            m_Aggregator.Subscribe (subscription, callback => callback ());
            m_Subscriptions.Add (subscription);

            return subscription;
        }
    }

    public IDisposable Subscribe<TData> (UInternalOperationId receiver, Func<IMessageRecord<TData>, CancellationToken, Task> handler)
    {
        ArgumentNullException.ThrowIfNull (handler);

        lock (m_Gate) {
            ObjectDisposedException.ThrowIf (m_Disposed, this);

            var subscription = new TSubscription<TData> (this, receiver, handler);

            m_Aggregator.Subscribe (subscription, callback => callback ());
            m_Subscriptions.Add (subscription);

            return subscription;
        }
    }

    public Task PublishAsync<TData> (IMessageRecord<TData> message, CancellationToken cancellationToken = default)
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

    public void Remove (IDisposable subscription)
    {
        lock (m_Gate) {
            m_Aggregator.Unsubscribe (subscription);
            m_Subscriptions.Remove (subscription);
        }
    }
    #endregion

    #region Fields
    readonly EventAggregator                        m_Aggregator = new ();
    readonly HashSet<IDisposable>                   m_Subscriptions = [];
    bool                                            m_Disposed;
    readonly object                                 m_Gate = new ();
    #endregion

    #region Internals
    //----- TSubscription
    sealed class TSubscription (
        TEventDispatcher owner,
        Func<IMessageRecord, Task> handler) : IHandle<IMessageRecord>, IDisposable
    {
        #region Members
        public Task HandleAsync (IMessageRecord message, CancellationToken cancellationToken)
        {
            if (Volatile.Read (ref m_Disposed) != 0) {
                return Task.CompletedTask;
            }

            return handler (message);
        }

        public void Dispose ()
        {
            if (Interlocked.Exchange (ref m_Disposed, 1) == 0) {
                owner.Remove (this);
            }
        }
        #endregion

        #region Fields
        int                     m_Disposed;
        #endregion
    }

    //----- TSubscription<TData>
    sealed class TSubscription<TData> (
        TEventDispatcher owner,
        UInternalOperationId receiver,
        Func<IMessageRecord<TData>, CancellationToken, Task> handler) : IHandle<IMessageRecord<TData>>, IDisposable
    {
        #region Members
        public async Task HandleAsync (IMessageRecord<TData> message, CancellationToken cancellationToken)
        {
            if (Volatile.Read (ref m_Disposed) != 0 || message.Receiver != receiver) {
                return;
            }

            cancellationToken.ThrowIfCancellationRequested ();

            await Task.Run (async () =>
            {
                if (Volatile.Read (ref m_Disposed) != 0) {
                    return;
                }

                cancellationToken.ThrowIfCancellationRequested ();
                await handler (message, cancellationToken);
            }, cancellationToken).ConfigureAwait (false);
        }

        public void Dispose ()
        {
            if (Interlocked.Exchange (ref m_Disposed, 1) == 0) {
                owner.Remove (this);
            }
        }
        #endregion

        #region Fields
        int                     m_Disposed;
        #endregion
    }
    #endregion
};
//---------------------------//

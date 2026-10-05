/*----------------------------------------------------------------
  Copyright (C) 2001 R&R Soft - All rights reserved.
  author: Roberto Oliveira Jucá    
----------------------------------------------------------------*/

namespace Connect.Process.Dispatcher;

//----- TSubscription<TData>
sealed class TSubscription<TData> : IHandle<TMessageRecord<TData>>, IDisposable
{
    #region Constructor
    public TSubscription (TEventDispatcher owner, UInternalOperationId receiver, Func<TMessageRecord<TData>, CancellationToken, Task> handler)
    {
        m_Owner = owner;
        m_Receiver = receiver;
        m_Handler = handler;
    }
    #endregion

    #region Members
    public async Task HandleAsync (TMessageRecord<TData> message, CancellationToken cancellationToken)
    {
        if (Volatile.Read (ref m_Disposed) != 0 || message.Receiver != m_Receiver)
            return;
        cancellationToken.ThrowIfCancellationRequested ();
        await Task.Run (async () =>
        {
            if (Volatile.Read (ref m_Disposed) != 0)
                return;
            cancellationToken.ThrowIfCancellationRequested ();
            await m_Handler (message, cancellationToken);
        }, cancellationToken).ConfigureAwait (false);
    }

    public void Dispose ()
    {
        if (Interlocked.Exchange (ref m_Disposed, 1) == 0) {
            m_Owner.Remove (this);
        }
    }
    #endregion

    #region Fields
    TEventDispatcher                                                    m_Owner;
    UInternalOperationId                                                m_Receiver;
    Func<TMessageRecord<TData>, CancellationToken, Task>                m_Handler;
    int                                                                 m_Disposed;
    #endregion
};
//---------------------------//

/*----------------------------------------------------------------
  Copyright (C) 2001 R&R Soft - All rights reserved.
  author: Roberto Oliveira Jucá    
----------------------------------------------------------------*/

namespace Connect.Provider.Interface;

//----- IEventDispatcher
public interface IEventDispatcher : IDisposable
{
    #region Members
    /// <summary>Receives every message, regardless of receiver or payload type.
    /// Dispose the returned subscription when the subscriber ends.</summary>
    IDisposable Subscribe (Func<IMessageRecord, Task> handler);

    /// <summary>Registers a receiver for an exact payload type. Keep and dispose the
    /// returned token when the module or profile ends.</summary>
    /// <remarks>Delivery uses the thread pool. Use SubscribeOnDispatcher for WPF handlers.
    /// Concurrent publications may invoke the same handler concurrently. Disposing a
    /// subscription prevents future delivery but does not stop an already running handler.</remarks>
    IDisposable Subscribe<TData> (UInternalOperationId receiver, Func<IMessageRecord<TData>, CancellationToken, Task> handler);

    /// <summary>Publishes asynchronously and completes after all matching handlers finish.
    /// Handler errors propagate to the caller. No matching receiver is a successful no-op.</summary>
    /// <remarks>This is in-process delivery, without persistence or ordering guarantees.
    /// Cancellation is cooperative; callers and handlers must observe their tokens.</remarks>
    Task PublishAsync<TData> (IMessageRecord<TData> message, CancellationToken cancellationToken = default);
    #endregion
};
//---------------------------//

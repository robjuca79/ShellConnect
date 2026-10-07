/*----------------------------------------------------------------
  Copyright (C) 2001 R&R Soft - All rights reserved.
  author: Roberto Oliveira Jucá    
----------------------------------------------------------------*/

namespace Connect.Bootstrapper;

[Export (typeof (IBootstrapper))]
[PartCreationPolicy (CreationPolicy.Shared)]
public class TBootstrapper : IBootstrapper
{
    [ImportingConstructor]
    public TBootstrapper (IEventDispatcher eventDispatcher)
    {
        ArgumentNullException.ThrowIfNull (eventDispatcher);
        m_EventDispatcher = eventDispatcher;
    }

    public void Start (object owner)
    {
        ArgumentNullException.ThrowIfNull (owner);
        lock (m_Gate) {
            ObjectDisposedException.ThrowIf (m_Disposed, this);
            if (m_Owners.Contains (owner))
                return;
            if (m_Owners.Count == 0) {
                m_Subscription = m_EventDispatcher.Subscribe (OnMessageReceivedAsync);
            }
            m_Owners.Add (owner);
        }
    }

    public void Stop (object owner)
    {
        ArgumentNullException.ThrowIfNull (owner);
        lock (m_Gate) {
            if (!m_Owners.Remove (owner))
                return;
            if (m_Owners.Count == 0) {
                m_Subscription?.Dispose ();
                m_Subscription = null;
            }
        }
    }

    public void Dispose ()
    {
        lock (m_Gate) {
            if (m_Disposed)
                return;
            m_Disposed = true;
            m_Subscription?.Dispose ();
            m_Subscription = null;
            m_Owners.Clear ();
        }
    }

    protected virtual Task OnMessageReceivedAsync (IMessageRecord message)
    {
        MessageReceived?.Invoke (message);
        return Task.CompletedTask;
    }

    public event Action<IMessageRecord>? MessageReceived;

    readonly IEventDispatcher m_EventDispatcher;
    readonly object m_Gate = new ();
    readonly HashSet<object> m_Owners = new (ReferenceEqualityComparer.Instance);
    IDisposable? m_Subscription;
    bool m_Disposed;
};
//---------------------------//




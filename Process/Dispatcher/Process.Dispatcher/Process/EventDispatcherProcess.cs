/*----------------------------------------------------------------
  Copyright (C) 2001 R&R Soft - All rights reserved.
  author: Roberto Oliveira Jucá    
----------------------------------------------------------------*/

namespace Connect.Process.Dispatcher;

//----- TEventDispatcherProcess
/// <summary>Shared access to the process messaging service through Provider contracts.</summary>
public static class TEventDispatcherProcess
{
    #region MyRegion
    public static IEventDispatcher Current
    {
        get
        {
            lock (m_Gate) {
                return m_Current ?? throw new InvalidOperationException ("The event dispatcher has not been created by Connect.Shell.");
            }
        }
    }
    #endregion

    /// <summary>Only the composition root supplies the concrete implementation.</summary>
    #region Members
    public static IEventDispatcher GetOrCreate (Func<IEventDispatcher> factory)
    {
        ArgumentNullException.ThrowIfNull (factory);

        lock (m_Gate) {
            return m_Current ??= factory () ?? throw new InvalidOperationException ("The dispatcher factory returned null.");
        }
    }
    #endregion

    #region Fields
    static readonly object                                      m_Gate = new ();
    static IEventDispatcher?                                    m_Current;
    #endregion
};
//---------------------------//





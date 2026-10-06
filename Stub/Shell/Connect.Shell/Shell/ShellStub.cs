/*----------------------------------------------------------------
  Copyright (C) 2001 R&R Soft - All rights reserved.
  author: Roberto Oliveira Jucá    
----------------------------------------------------------------*/

using SPAD.neXt.Interfaces;
using SPAD.neXt.Interfaces.Events;

namespace Connect.Shell;

//----- TShellStub
public class TShellStub : ScriptStub
{
    #region Constructor
    public TShellStub ()
        : this (TEventDispatcherProcess.GetOrCreate (() => new Connect.Process.Dispatcher.TEventDispatcher ()))
    {
    }

    public TShellStub (IEventDispatcher eventDispatcher)
    {
        ArgumentNullException.ThrowIfNull (eventDispatcher);
        m_EventDispatcher = eventDispatcher;
    }
    #endregion

    #region Overrides
    protected override void InitializeScript ()
    {
        lock (m_Gate) {
            if (m_Lifetime is not null) {
                return;
            }

            m_Lifetime = new CancellationTokenSource ();
            //ProfileChanged += OnProfileChanged;

            Application.SubscribeToSystemEvent (SPADSystemEvents.ProfileChanged, OnProfileChanged);

        }
    }

    protected override void DeinitializeScript ()
    {
        CancellationTokenSource? lifetime;

        lock (m_Gate) {
            //ProfileChanged -= OnProfileChanged;
            lifetime = m_Lifetime;
            m_Lifetime = null;
        }

        if (lifetime is null) {
            return;
        }

        lifetime.Cancel ();
        lifetime.Dispose ();
    }
    #endregion

    #region Event
    async void OnProfileChanged (object sender, ISPADEventArgs args)//(IProfile profile, string propertyName)
    {
        CancellationToken token;

        lock (m_Gate) {
            if (m_Lifetime is null) {
                return;
            }

            token = m_Lifetime.Token;
        }

        try {
            //await PublishAsync (profile, token).ConfigureAwait (false);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) {
            // Script deinitialization canceled pending delivery.
        }
        catch (Exception error) {
            ScriptLogger?.Error ("Cannot publish PROFILE_CHANGED: {0}", error);
        }
    }
    #endregion

    #region Fields
    protected override string ScriptDataPrefix => "";
    CancellationTokenSource?                                    m_Lifetime;
    readonly IEventDispatcher                                   m_EventDispatcher;
    readonly object                                             m_Gate = new ();
    #endregion

    #region Support
    Task PublishAsync (IProfile profile, CancellationToken cancellationToken)
    {
        var data = new TProfileRecord (profile?.Name, profile?.Filename, profile?.IsDummyProfile ?? true);

        var message = new TMessageRecord<TProfileRecord> (
            UInternalOperationId.STUB_SHELL,
            UInternalOperationId.STUB_BOOTSTRAPPER,
            UInternalMessageId.PROFILE_CHANGED,
            data);

        return m_EventDispatcher.PublishAsync (message, cancellationToken);
    }
    #endregion
};
//---------------------------//


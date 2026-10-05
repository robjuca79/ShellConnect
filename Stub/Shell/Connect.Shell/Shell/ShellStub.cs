/*----------------------------------------------------------------
  Copyright (C) 2001 R&R Soft - All rights reserved.
  author: Roberto Oliveira Jucá    
----------------------------------------------------------------*/

namespace Connect.Shell;

//----- TShellStub
public class TShellStub : ScriptStub
{
    #region Constructor
    public TShellStub ()
        : this (TEventDispatcherProvider.GetOrCreate (() => new Connect.Process.Dispatcher.TEventDispatcher ()))
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
        lock (m_ProfileGate) {
            if (m_ProfileLifetime is not null) {
                return;
            }

            m_ProfileLifetime = new CancellationTokenSource ();
            ProfileChanged += OnProfileChanged;
        }
    }

    protected override void DeinitializeScript ()
    {
        CancellationTokenSource? lifetime;

        lock (m_ProfileGate) {
            ProfileChanged -= OnProfileChanged;
            lifetime = m_ProfileLifetime;
            m_ProfileLifetime = null;
        }

        if (lifetime is null) {
            return;
        }

        lifetime.Cancel ();
        lifetime.Dispose ();
    }
    #endregion

    #region Event
    async void OnProfileChanged (IProfile profile, string propertyName)
    {
        CancellationToken token;

        lock (m_ProfileGate) {
            if (m_ProfileLifetime is null) {
                return;
            }

            token = m_ProfileLifetime.Token;
        }

        try {
            await PublishAsync (profile, token).ConfigureAwait (false);
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
    CancellationTokenSource?                                    m_ProfileLifetime;
    readonly IEventDispatcher                                   m_EventDispatcher;
    readonly object                                             m_ProfileGate = new ();
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


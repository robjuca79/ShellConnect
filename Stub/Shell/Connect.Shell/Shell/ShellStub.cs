/*----------------------------------------------------------------
  Copyright (C) 2001 R&R Soft - All rights reserved.
  author: Roberto Oliveira Jucá    
----------------------------------------------------------------*/

namespace Connect.Shell;

//----- TShellStub
public class TShellStub : ScriptStub
{
    #region Overrides
    protected override void InitializeScript ()
    {
        lock (m_Gate) {
            if (m_Initialized) {
                return;
            }

            m_Initialized = true;

            try {
                Application.SubscribeToSystemEvent (SPADSystemEvents.ProfileChanged, OnProfileChanged);
            }
            catch {
                m_Initialized = false;
                //TCompositionHost.GetExport<IBootstrapper> ().Stop (this);
                throw;
            }
        }

        // A profile may already be active before the script subscribes.
        _ = PublishCurrentProfileAsync ();
    }

    protected override void DeinitializeScript ()
    {
        lock (m_Gate) {
            if (!m_Initialized) {
                return;
            }

            Application.UnsubscribeFromSystemEvent (SPADSystemEvents.ProfileChanged, OnProfileChanged);
            m_Initialized = false;

            //TCompositionHost.GetExport<IBootstrapper> ().Stop (this);
        }
    }
    #endregion

    #region Event
    async void OnProfileChanged (object sender, ISPADEventArgs args)
    {
        await PublishCurrentProfileAsync ().ConfigureAwait (false);
    }
    #endregion

    #region Property
    protected override string ScriptDataPrefix => "";
    #endregion

    #region Fields
    bool                                                        m_Initialized;
    readonly object                                             m_Gate = new ();
    #endregion

    #region Support
    async Task PublishCurrentProfileAsync ()
    {
        try {
            IProfile? profile;

            lock (m_Gate) {
                if (m_Initialized is false) {
                    return;
                }

                profile = Application.ActiveProfile;
            }

            if (profile is null || profile.IsDummyProfile || profile.Name.Equals ("No profile")) {
                return;
            }

            ScriptLogger?.Debug ("Publishing PROFILE_CHANGED for profile: {0}", profile.Name);

            await PublishAsync (profile).ConfigureAwait (false);
        }
        catch (Exception error) {
            ScriptLogger?.Error ("Cannot publish PROFILE_CHANGED: {0}", error);
        }
    }

    Task PublishAsync (IProfile profile)
    {
        var data = new TProfileRecord (profile?.Name, profile?.Filename, profile?.IsDummyProfile ?? true);

        var message = new TMessageRecord<TProfileRecord> (
            UInternalOperationId.STUB_SHELL,
            UInternalOperationId.STUB_BOOTSTRAPPER,
            UInternalMessageId.PROFILE_CHANGED,
            data);

        return TCompositionHost.GetExport<IEventDispatcher> ().PublishAsync<TProfileRecord> (message);
    }
    #endregion
};
//---------------------------//

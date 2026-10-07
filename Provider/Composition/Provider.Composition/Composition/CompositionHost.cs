/*----------------------------------------------------------------
  Copyright (C) 2001 R&R Soft - All rights reserved.
  author: Roberto Oliveira Jucá    
----------------------------------------------------------------*/

namespace Connect.Provider.Composition;

//----- TCompositionHost
public static class TCompositionHost
{
    // Owns composition for the process; script deinitialization must not dispose
    // services still used by other scripts.
    #region Constructor
    static TCompositionHost ()
    {
        AppDomain.CurrentDomain.ProcessExit += (_, _) => Shutdown ();
    }
    #endregion

    #region Members
    public static T GetExport<T> () where T : class
    {
        lock (m_Gate) {
            ObjectDisposedException.ThrowIf (m_ShuttingDown, typeof (TCompositionHost));

            if (m_Container is null) {
                var directory = new DirectoryInfo (@"D:\SPAD.neXt\AddOns");
                var catalog = new DirectoryCatalog (directory.FullName, "Connect.*.dll");

                try {
                    m_Container = new CompositionContainer (catalog, CompositionOptions.DisableSilentRejection | CompositionOptions.IsThreadSafe);
                    m_Catalog = catalog;
                }
                catch {
                    catalog.Dispose ();
                    throw;
                }
            }

            // Missing or duplicate exports are composition errors, never an
            // invitation to silently create a different implementation.
            return m_Container.GetExportedValue<T> ()
                ?? throw new InvalidOperationException ($"The export for {typeof (T).FullName} is null.");
        }
    }
    #endregion

    #region Fields
    static readonly object                                              m_Gate = new ();
    static DirectoryCatalog?                                            m_Catalog;
    static CompositionContainer?                                        m_Container;
    static bool                                                         m_ShuttingDown;
    #endregion

    #region Support
    static void Shutdown ()
    {
        lock (m_Gate) {
            m_ShuttingDown = true;
            try {
                m_Container?.Dispose ();
            }
            finally {
                m_Catalog?.Dispose ();
                m_Container = null;
                m_Catalog = null;
            }
        }
    }
    #endregion
}
//---------------------------//


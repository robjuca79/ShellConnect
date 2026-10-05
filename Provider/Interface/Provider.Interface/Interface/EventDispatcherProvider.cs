namespace Connect.Provider.Interface;

/// <summary>Shared access to the process messaging service through Provider contracts.</summary>
public static class TEventDispatcherProvider
{
    private static readonly object Gate = new ();
    private static IEventDispatcher? _current;

    public static IEventDispatcher Current
    {
        get
        {
            lock (Gate)
                return _current ?? throw new InvalidOperationException ("The event dispatcher has not been created by Connect.Shell.");
        }
    }

    /// <summary>Only the composition root supplies the concrete implementation.</summary>
    public static IEventDispatcher GetOrCreate (Func<IEventDispatcher> factory)
    {
        ArgumentNullException.ThrowIfNull (factory);
        lock (Gate)
            return _current ??= factory () ?? throw new InvalidOperationException ("The dispatcher factory returned null.");
    }
}

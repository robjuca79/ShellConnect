namespace Connect.Provider.Message;

/// <summary>A message shared between independent modules. Treat Data as immutable.</summary>
/// <remarks>Addresses are case-sensitive. A null Receiver broadcasts to all subscribers
/// of the same payload type. Sender identifies the source; it is not an object reference.</remarks>
public sealed record oldMessage<TData>
{
    public oldMessage(string sender, string? receiver, TData data)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sender);
        if (receiver is not null)
            ArgumentException.ThrowIfNullOrWhiteSpace(receiver);

        Sender = sender;
        Receiver = receiver;
        Data = data;
    }

    public string Sender { get; }
    public string? Receiver { get; }
    public TData Data { get; }
}

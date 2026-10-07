/*------------------------------------------------------------
  Copyright (C) 2001 R & R Soft - All rights reserved.
  author: Roberto Oliveira Jucá
---------------------------------------------------------------- */

namespace Connect.Provider.Data;

//----- TMessageRecord<TData>
public sealed record TMessageRecord<TData> : IMessageRecord<TData>
{
    #region Property
    public UInternalOperationId Sender { get; }
    public UInternalOperationId Receiver { get; }
    public UInternalMessageId Message { get; }
    public TData Data { get; }
    object? IMessageRecord.Data => Data;
    #endregion

    #region Constructor
    public TMessageRecord (UInternalOperationId sender, UInternalOperationId receiver, TData data)
        : this (sender, receiver, UInternalMessageId.NONE, data)
    {
    }

    public TMessageRecord (UInternalOperationId sender, UInternalOperationId receiver, UInternalMessageId message, TData data)
    {
        Sender = sender;
        Receiver = receiver;
        Message = message;
        Data = data;
    }
    #endregion
};
//---------------------------//

/*----------------------------------------------------------------
  Copyright (C) 2001 R&R Soft - All rights reserved.
  author: Roberto Oliveira Jucá
----------------------------------------------------------------*/

namespace Connect.Provider.Interface;

/// <summary>A message envelope independent of its payload type.</summary>
/// //----- IMessageRecord
public interface IMessageRecord
{
    #region Property
    UInternalOperationId Sender { get; }
    UInternalOperationId Receiver { get; }
    UInternalMessageId Message { get; }
    object? Data { get; }
    #endregion
};
//---------------------------//

//----- IMessageRecord<out TData>
public interface IMessageRecord<out TData> : IMessageRecord
{
    new TData Data { get; }
};
//---------------------------//

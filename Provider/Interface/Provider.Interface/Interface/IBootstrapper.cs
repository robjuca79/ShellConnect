/*----------------------------------------------------------------
  Copyright (C) 2001 R&R Soft - All rights reserved.
  author: Roberto Oliveira Jucá    
----------------------------------------------------------------*/

namespace Connect.Provider.Interface;

//----- IBootstrapper
public interface IBootstrapper : IDisposable
{
    event Action<IMessageRecord>? MessageReceived;
    void Start (object owner);
    void Stop (object owner);
};
//---------------------------//




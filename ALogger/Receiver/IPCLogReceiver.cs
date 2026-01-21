using System;
using ALogger.Models;

namespace ALogger.Receiver;

public class IPCLogReceiver: ILogReceiver
{
  public Action<LogEntry> OnLogReceived { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

  public void Start()
  {
    throw new NotImplementedException();
  }

  public void Stop()
  {
    throw new NotImplementedException();
  }
}

using System;
using ALogger.Models;

namespace ALogger.Receiver;
public interface ILogReceiver
{
  Action<LogEntry> OnLogReceived { get; set; }  
  void Start();
  void Stop();
}

public class LogReceiverBase : ILogReceiver
{
  public virtual Action<LogEntry> OnLogReceived { get; set; } = entry => { };
  public virtual void Start()
  {
    // 默认实现为空
  }

  public virtual void Stop()
  {
    // 默认实现为空
  }
}
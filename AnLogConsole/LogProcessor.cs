using System;

namespace AnLogConsole;

public class LogProcessor
{
  public static void ProcessLogs(CancellationToken cancellationToken)
  {
    while (!cancellationToken.IsCancellationRequested)
    {
      // Simulate log processing
      Console.WriteLine("Processing logs...");
      Thread.Sleep(1000); // Simulate time-consuming work
    }
    Console.WriteLine("Log processing stopped.");
  }
}

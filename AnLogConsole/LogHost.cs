namespace AnLogConsole;

public class LogHost
{

  private readonly CancellationTokenSource _cancellationTokenSource = new();
  public void StartLogging()
  {
    Task.Run(() => LogProcessor.ProcessLogs(_cancellationTokenSource.Token)); 
  }
  public void StopLogging()
  {
    _cancellationTokenSource.Cancel();
  }

}
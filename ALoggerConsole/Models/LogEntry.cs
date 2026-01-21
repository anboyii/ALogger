namespace ALoggerConsole.Models;

public record class LogEntry
{
  public string Message { get; init; }
  public string Level { get; init; }
  public DateTime Timestamp { get; init; }
  public string Category { get; init; }
}

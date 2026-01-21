namespace ALogger.Models;

public record class LogEntry
{
  public required string Message { get; init; }
  public required DateTime Timestamp { get; init; }
  public required int Level { get; init; }
  public required string Category { get; init; }
}

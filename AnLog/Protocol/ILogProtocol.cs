using System;
using AnLog.Models;

namespace AnLog.Protocol;

/// <summary>
/// 日志协议接口 - 定义日志数据的序列化和反序列化规范
/// </summary>
public interface ILogProtocol
{
  /// <summary> 
  /// 协议名称，用于标识协议类型
  /// </summary>    
  string Name { get; }

  /// <summary>
  /// 将日志条目序列化为字节数组
  /// </summary>
  /// <param name="entry">要序列化的日志条目</param>
  /// <returns>序列化后的字节数组</returns>
  byte[] Serialize(LogEntry entry);

  /// <summary>
  /// 从字节数组反序列化日志条目
  /// </summary>
  /// <param name="data">包含日志数据的字节数组</param>
  /// <returns>反序列化的日志条目，失败返回 null</returns>
  LogEntry? Deserialize(byte[] data);

  /// <summary>
  /// 从流中异步读取并反序列化日志条目
  /// </summary>
  /// <param name="stream">数据流</param>
  /// <param name="cancellationToken">取消令牌</param>
  /// <returns>反序列化的日志条目，流结束或失败返回 null</returns>
  Task<LogEntry?> ReadFromStreamAsync(Stream stream, CancellationToken cancellationToken = default);

  /// <summary>
  /// 解析错误回调，用于记录协议错误
  /// 默认返回 DefaultLogError 实现
  /// </summary>
  public Action<string> OnParseError { get; set; }
}

public class LogProtocolBase : ILogProtocol
{
  public virtual string Name => "Base";

  private Action<string>? _onParseError;

  /// <summary>
  /// 错误回调，获取时为 null 则自动返回 DefaultLogError
  /// </summary>
  public virtual Action<string> OnParseError
  {
    get => _onParseError ?? DefaultLogError;
    set => _onParseError = value;
  }

  public virtual byte[] Serialize(LogEntry entry)
  {
    throw new NotImplementedException();
  }

  public virtual LogEntry? Deserialize(byte[] data)
  {
    throw new NotImplementedException();
  }

  public virtual async Task<LogEntry?> ReadFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }

  /// <summary>
  /// 记录错误到控制台（默认实现）
  /// </summary>
  /// <param name="message">错误消息</param>
  protected virtual void DefaultLogError(string message)
  {
    Console.WriteLine($"[{Name}] {message}");
  }
}

public static class LogprotocolFactory
{
  /// <summary>
  /// 创建默认的日志协议实例
  /// </summary>
  /// <returns>默认日志协议实例</returns>
  public static ILogProtocol CreateDefaultProtocol()
  {
    return new BinaryProtocol();
  }
}

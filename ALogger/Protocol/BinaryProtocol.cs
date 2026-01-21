using System;
using System.Text;
using ALogger.Models;

namespace ALogger.Protocol;

/// <summary>
/// 二进制日志协议
/// 格式: [魔数:2字节][版本:1字节][数据长度:4字节][消息][分类][级别][时间戳:8字节]
/// </summary>
public class BinaryProtocol : LogProtocolBase
{
  public override string Name => "Binary";

  /// <summary>
  /// 协议魔数，用于识别协议类型
  /// </summary>
  private const ushort MagicNumber = 0xABCD;

  /// <summary>
  /// 协议版本号
  /// </summary>
  private const byte Version = 1;

  /// <summary>
  /// 最大允许的数据长度（10MB）
  /// </summary>
  private const int MaxDataLength = 10 * 1024 * 1024;

  /// <summary>
  /// 序列化日志条目为字节数组
  /// 格式: [0xABCD][版本][数据长度][消息][分类][级别][时间戳]
  /// </summary>
  /// <param name="entry">要序列化的日志条目</param>
  /// <returns>序列化后的字节数组</returns>
  public override byte[] Serialize(LogEntry entry)
  {
    using var ms = new MemoryStream();
    using var writer = new BinaryWriter(ms, Encoding.UTF8);
    
    // 1. 写入魔数
    writer.Write(MagicNumber);
    
    // 2. 写入版本号
    writer.Write(Version);
    
    // 3. 预留数据长度位置
    var lengthPosition = ms.Position;
    writer.Write(0);  // 占位符，稍后回写
    
    // 4. 写入数据部分
    var dataStartPosition = ms.Position;
    writer.Write(entry.Message);              // 消息
    writer.Write(entry.Category);             // 分类
    writer.Write(entry.Level);                // 级别
    writer.Write(entry.Timestamp.ToBinary()); // 时间戳
    
    // 5. 计算并回写数据长度
    var dataLength = (int)(ms.Position - dataStartPosition);
    ms.Seek(lengthPosition, SeekOrigin.Begin);
    writer.Write(dataLength);
    
    // 6. 返回完整字节数组
    ms.Seek(0, SeekOrigin.End);
    return ms.ToArray();
  }

  /// <summary>
  /// 从字节数组反序列化日志条目
  /// </summary>
  /// <param name="data">包含序列化数据的字节数组</param>
  /// <returns>反序列化的日志条目，失败返回 null</returns>
  public override LogEntry? Deserialize(byte[] data)
  {
    try
    {
      using var ms = new MemoryStream(data);
      using var reader = new BinaryReader(ms, Encoding.UTF8);
      
      // 1. 验证魔数
      var magic = reader.ReadUInt16();
      if (magic != MagicNumber)
      {
        OnParseError($"无效的魔数: 0x{magic:X4}, 期望值: 0x{MagicNumber:X4}");
        return null;  // 非法协议
      }
      
      // 2. 验证版本
      var version = reader.ReadByte();
      if (version != Version)
      {
        OnParseError($"不支持的版本: {version}, 当前版本: {Version}");
        return null;  // 版本不兼容
      }
      
      // 3. 读取数据长度
      var dataLength = reader.ReadInt32();
      if (dataLength <= 0 || dataLength > MaxDataLength)
      {
        OnParseError($"非法的数据长度: {dataLength}, 最大允许: {MaxDataLength}");
        return null;  // 数据长度非法
      }
      
      // 4. 验证剩余数据是否足够
      var remainingBytes = ms.Length - ms.Position;
      if (remainingBytes < dataLength)
      {
        OnParseError($"数据不完整: 期望 {dataLength} 字节, 实际 {remainingBytes} 字节");
        return null;  // 数据不完整
      }
      
      // 5. 读取数据部分
      var message = reader.ReadString();
      var category = reader.ReadString();
      var level = reader.ReadInt32();
      var timestamp = DateTime.FromBinary(reader.ReadInt64());
      
      return new LogEntry
      {
        Message = message,
        Category = category,
        Level = level,
        Timestamp = timestamp
      };
    }
    catch (Exception ex)
    {
      OnParseError($"反序列化异常: {ex.Message}");
      return null;
    }
  }

  /// <summary>
  /// 从流中异步读取并反序列化日志条目
  /// </summary>
  /// <param name="stream">数据流</param>
  /// <param name="cancellationToken">取消令牌</param>
  /// <returns>反序列化的日志条目，失败返回 null</returns>
  public override async Task<LogEntry?> ReadFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
  {
    try
    {
      // 1. 读取头部 (魔数2字节 + 版本1字节 + 长度4字节 = 7字节)
      var headerBuffer = new byte[7];
      var headerBytesRead = await ReadExactAsync(stream, headerBuffer, cancellationToken);
      if (headerBytesRead < 7)
      {
        OnParseError($"头部数据不完整: 期望 7 字节, 实际 {headerBytesRead} 字节");
        return null;  // 头部不完整
      }
      
      // 2. 验证魔数
      var magic = BitConverter.ToUInt16(headerBuffer, 0);
      if (magic != MagicNumber)
      {
        OnParseError($"无效的魔数: 0x{magic:X4}, 期望值: 0x{MagicNumber:X4}");
        return null;  // 非法协议
      }
      
      // 3. 验证版本
      var version = headerBuffer[2];
      if (version != Version)
      {
        OnParseError($"不支持的版本: {version}, 当前版本: {Version}");
        return null;  // 版本不兼容
      }
      
      // 4. 获取数据长度
      var dataLength = BitConverter.ToInt32(headerBuffer, 3);
      if (dataLength <= 0 || dataLength > MaxDataLength)
      {
        OnParseError($"非法的数据长度: {dataLength}, 最大允许: {MaxDataLength}");
        return null;  // 数据长度非法
      }
      
      // 5. 读取完整数据部分
      var dataBuffer = new byte[dataLength];
      var dataBytesRead = await ReadExactAsync(stream, dataBuffer, cancellationToken);
      if (dataBytesRead < dataLength)
      {
        OnParseError($"数据不完整: 期望 {dataLength} 字节, 实际 {dataBytesRead} 字节");
        return null;  // 数据不完整
      }
      
      // 6. 使用 BinaryReader 解析数据
      using var ms = new MemoryStream(dataBuffer);
      using var reader = new BinaryReader(ms, Encoding.UTF8);

      var message = reader.ReadString();
      var category = reader.ReadString();
      var level = reader.ReadInt32();
      var timestamp = DateTime.FromBinary(reader.ReadInt64());
      
      return new LogEntry
      {
        Message = message,
        Category = category,
        Level = level,
        Timestamp = timestamp
      };
    }
    catch (Exception ex) when (ex is not OperationCanceledException)
    {
      OnParseError($"从流读取异常: {ex.Message}");
      return null;
    }
  }

  /// <summary>
  /// 从流中精确读取指定数量的字节
  /// </summary>
  /// <param name="stream">数据流</param>
  /// <param name="buffer">缓冲区</param>
  /// <param name="cancellationToken">取消令牌</param>
  /// <returns>实际读取的字节数</returns>
  private static async Task<int> ReadExactAsync(Stream stream, byte[] buffer, CancellationToken cancellationToken)
  {
    var totalRead = 0;
    var remaining = buffer.Length;
    
    while (remaining > 0)
    {
      var bytesRead = await stream.ReadAsync(
        buffer.AsMemory(totalRead, remaining), 
        cancellationToken);
      
      if (bytesRead == 0)
      {
        break;  // 流结束
      }
      
      totalRead += bytesRead;
      remaining -= bytesRead;
    }
    
    return totalRead;
  }
}

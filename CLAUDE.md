# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ALogger is a .NET 9.0 logging library with a decoupled architecture for log serialization, transmission, and processing. It consists of two main projects:

- **ALogger** - Core library containing the logging protocol, models, and receiver interfaces
- **ALoggerConsole** - Console application that hosts the logging service

## Build Commands

```bash
# Build the entire solution
dotnet build ALogger.sln

# Build a specific project
dotnet build ALogger/ALogger.csproj
dotnet build ALoggerConsole/ALoggerConsole.csproj

# Run the console application
dotnet run --project ALoggerConsole/ALoggerConsole.csproj

# Build in release mode
dotnet build ALogger.sln --configuration Release
```

## Architecture

### Core Components

**ALogger.Models.LogEntry** (`ALogger/Models/LogEntry.cs`)
- Record type representing a log entry with: Message, Timestamp, Level, Category

**ALogger.Protocol.ILogProtocol** (`ALogger/Protocol/ILogProtocol.cs`)
- Interface defining serialization/deserialization contract for log entries
- `LogProtocolBase` provides base implementation with default error handling (`DefaultLogError`)
- `LogprotocolFactory.CreateDefaultProtocol()` returns `BinaryProtocol` instance

**ALogger.Protocol.BinaryProtocol** (`ALogger/Protocol/BinaryProtocol.cs`)
- Binary serialization format: [Magic:0xABCD][Version:1][Length:4][Message][Category][Level][Timestamp:8]
- Enforces max data length of 10MB
- Validates magic number and version during deserialization
- Provides both byte array and stream-based deserialization (`ReadFromStreamAsync`)

**ALogger.Receiver.ILogReceiver** (`ALogger/Receiver/ILogReciever.cs`)
- Interface for receiving logs with `OnLogReceived` callback
- `LogReceiverBase` provides empty implementations of `Start()` and `Stop()`

### Console Application

**LogHost** (`ALoggerConsole/LogHost.cs`)
- Manages the logging lifecycle using `CancellationTokenSource`
- Starts/stops `LogProcessor.ProcessLogs()` on a background task

**LogProcessor** (`ALoggerConsole/LogProcessor.cs`)
- Static class with `ProcessLogs(CancellationToken)` method
- Currently contains placeholder implementation (simulates log processing)

## Project Status

**IPCLogReceiver** (`ALogger/Receiver/IPCLogReceiver.cs`) - Not yet implemented (throws `NotImplementedException`)
- This class should implement the IPC-based log receiver functionality

## Key Design Patterns

- **Protocol Pattern**: `ILogProtocol` with base class `LogProtocolBase` allows pluggable serialization formats
- **Factory Pattern**: `LogprotocolFactory.CreateDefaultProtocol()` for protocol instantiation
- **Template Method**: `LogProtocolBase` provides default error handling that can be overridden
- **Async/Await**: Stream-based operations in `BinaryProtocol` use async patterns with cancellation support

## Notes

- Project uses implicit usings and nullable reference types enabled
- Code comments are in Chinese (Simplified)
- The `OnParseError` property in `ILogProtocol` uses null-fallback pattern - returns `DefaultLogError` if not explicitly set

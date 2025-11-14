# Debugging Guide for Signal Booster

Advanced debugging techniques and best practices for senior developers working on the Signal Booster application.

## 🎯 Debug Configurations

Pre-configured launch configurations in `.vscode/launch.json`:
- `.NET Core Launch (SignalBooster)` - Standard execution with `physician_note1.txt`
- `.NET Core Launch (SignalBooster - JSON Input)` - Tests JSON-wrapped input format
- `.NET Core Launch (SignalBooster - Custom Args)` - Customizable arguments

Press `F5` to start debugging with the active configuration.

## 🔍 Advanced Breakpoint Techniques

### Conditional Breakpoints
Right-click breakpoint → Edit Breakpoint → Add condition:
```csharp
// Break only when device is CPAP
result.Device == "CPAP"

// Break on specific provider
result.OrderingProvider.Contains("Cuddy")

// Break on extraction errors
result.Device == "Unknown" && !string.IsNullOrEmpty(noteText)
```

### Hit Count Breakpoints
Useful for debugging loops or repeated calls:
- Break after N hits
- Break when hit count equals a multiple
- Break when hit count is greater than or equal to N

### Dependent Breakpoints
Break only when another breakpoint has been hit first. Useful for debugging complex workflows.

### Function Breakpoints
Set breakpoints on method signatures:
- `DmeExtractor.ExtractDmeInfo`
- `ApiClient.PostDmeDataAsync`
- `LlmDmeExtractor.ExtractDmeInfo`

### Logpoints (Non-Breaking Breakpoints)
Right-click margin → Add Logpoint:
```
Extracting device: {result.Device}, Provider: {result.OrderingProvider}
Note length: {noteText.Length}, Device type: {result.Device}
```

## 🎯 Component-Specific Debugging Strategies

### Dependency Injection Container Inspection
```csharp
// In Program.cs, after ConfigureServices()
var services = serviceProvider.GetServices<IDmeExtractor>();
// Inspect registered services in Watch window
```

### Strategy Pattern Debugging
- Set breakpoint in `DeviceExtractorRegistry.GetExtractor()` to inspect strategy selection
- Use conditional breakpoints per device type
- Monitor `IDeviceSpecificExtractor` implementations

### LLM Integration Debugging
- Inspect prompt construction in `LlmDmeExtractor.BuildExtractionPrompt()`
- Monitor token usage and response parsing
- Use logpoints to capture full LLM responses without breaking
- Set breakpoint after `ParseLlmResponse()` to validate JSON extraction

### Input Format Parser Chain
- Breakpoint in `InputFormatParserRegistry.GetParser()` to observe format detection
- Conditional breakpoint: `parser is JsonWrappedParser`
- Monitor parser selection logic

## 📊 Structured Logging & Observability

### Log Level Configuration
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "SignalBooster": "Trace",
      "SignalBooster.Services.Extractors": "Debug",
      "SignalBooster.Services.InputFormats": "Debug"
    }
  }
}
```

### Logging Best Practices
- Use structured logging with parameters: `_logger.LogInformation("Extracted {Device} for {Provider}", device, provider)`
- Leverage log scopes for correlation: `using (_logger.BeginScope("Processing file: {File}", filePath))`
- Monitor performance with timing: `_logger.LogDebug("Extraction took {ElapsedMs}ms", stopwatch.ElapsedMilliseconds)`

### Performance Profiling via Logs
Enable Trace level to capture:
- Method entry/exit
- Dependency resolution timing
- HTTP request/response cycles
- LLM API call latencies

## 🧪 Advanced Test Debugging

### Test Execution Strategies
```bash
# Run with code coverage
dotnet test SignalBooster.sln --collect:"XPlat Code Coverage" --results-directory:./TestResults

# Run specific test with detailed output
dotnet test --filter "FullyQualifiedName~DmeExtractorTests.ExtractDmeInfo_WithValidNote" --verbosity detailed

# Run tests in parallel (disable for debugging)
dotnet test -- --parallel

# Attach debugger to test host
dotnet test -- --debug
```

### Mock Inspection
When debugging tests with Moq:
- Inspect `_mock.Setup()` calls in Watch window
- Verify mock invocations: `_mock.Verify(x => x.Method(), Times.Once)`
- Use `Mock.Of<T>()` for simpler scenarios

### Test Data Debugging
- Set breakpoints in test `[Fact]` methods
- Inspect arranged data before act
- Validate assertions step-by-step

## 🐛 Advanced Debugging Scenarios

### Scenario 1: DI Container Resolution Issues
```csharp
// In Program.cs, after serviceProvider creation
var extractor = serviceProvider.GetService<IDmeExtractor>();
// Inspect in Watch: extractor.GetType().Name
// Verify correct implementation is resolved based on config
```

### Scenario 2: Strategy Pattern Selection
- Breakpoint in `DeviceExtractorRegistry.GetExtractor(string deviceType)`
- Conditional: `deviceType == "CPAP" && _extractors.Count == 0`
- Inspect registry state: `_extractors.Keys`

### Scenario 3: Async/Await Flow
- Set breakpoint in `DmeProcessingService.ProcessPhysicianNoteAsync()`
- Use "Step Into" to follow async flow
- Monitor `Task` state in Watch window
- Check `Task.IsCompleted`, `Task.Exception`

### Scenario 4: HTTP Client Configuration
- Inspect `HttpClient` base address and headers
- Monitor request/response in Debug Console
- Use Fiddler/Charles proxy for external API debugging
- Set breakpoint after `PostAsync()` to inspect `HttpResponseMessage`

### Scenario 5: Configuration Binding
- Breakpoint in `Program.cs` after `config.GetSection(AppSettings.SectionName).Get<AppSettings>()`
- Inspect `appSettings` object graph
- Verify nested `LlmSettings` binding

### Scenario 6: Exception Propagation
- Enable "Break on All Exceptions" (Ctrl+Alt+E)
- Filter to specific exceptions: `HttpRequestException`, `FormatException`
- Inspect exception stack trace and inner exceptions
- Use Exception Settings to break on first-chance exceptions

## 🔧 Production Debugging Techniques

### Remote Debugging
```bash
# Enable remote debugging on target
dotnet run --no-build

# Attach from development machine
# Use "Attach to Process" with process ID or name
```

### Memory Profiling
- Use `dotnet-dump` for memory snapshots
- Analyze heap with PerfView or dotMemory
- Monitor object allocations in Debug → Performance Profiler

### Performance Profiling
```bash
# Profile with dotnet-trace
dotnet-trace collect --providers Microsoft-DotNETCore-SampleProfiler -p <pid>

# Analyze with PerfView or SpeedScope
```

### Thread Debugging
- Use Parallel Stacks window to inspect thread execution
- Set breakpoints on `Task.Run()`, `async` methods
- Monitor thread pool starvation

### Call Stack Analysis
- Use Call Stack window with "Show External Code" enabled
- Navigate through DI container resolution
- Trace async continuation chains

## 📝 Advanced Debugging Techniques

### Expression Evaluation
In Debug Console, evaluate complex expressions:
```csharp
// Inspect service registrations
serviceProvider.GetServices<IDeviceSpecificExtractor>().Select(s => s.GetType().Name)

// Evaluate LINQ queries
result.AddOns?.Where(a => a.Contains("humidifier"))

// Test regex patterns
System.Text.RegularExpressions.Regex.Match(noteText, @"AHI\s*[>:]\s*(\d+)")
```

### Watch Window Expressions
Add to Watch panel:
```csharp
// Type inspection
result.GetType().GetProperties().Select(p => p.Name)

// Collection analysis
result.AddOns?.Count ?? 0

// String analysis
noteText.Split('\n').Length

// Null-coalescing chains
result?.Device ?? result?.OrderingProvider ?? "Unknown"
```

### Data Tips & Hover Evaluation
- Hover over variables for quick inspection
- Expand object graphs in Data Tips
- Use "Copy Value" to export complex objects
- "Copy Expression" to reuse in Watch window

### Debugger Attributes
Consider adding `[DebuggerDisplay]` attributes for complex types:
```csharp
[DebuggerDisplay("Device: {Device}, Provider: {OrderingProvider}")]
public class DmeExtractionResult { ... }
```

## 🚀 Performance & Diagnostic Commands

```bash
# Build with symbols
dotnet build SignalBooster.sln -c Debug /p:DebugType=full

# Run with environment variables
$env:ASPNETCORE_ENVIRONMENT="Development"; dotnet run

# Profile memory allocation
dotnet run --project SignalBooster/SignalBooster.csproj --collect:"GC"

# Generate diagnostic report
dotnet run --project SignalBooster/SignalBooster.csproj --collect:"EventPipe"
```

## 💡 Senior Developer Best Practices

1. **Use DebuggerDisplay Attributes**: Customize object representation in debugger
2. **Leverage DebuggerStepThrough**: Skip boilerplate code (DI setup, logging wrappers)
3. **Use DebuggerHidden**: Hide implementation details from call stack
4. **Conditional Compilation**: `#if DEBUG` for debug-only code paths
5. **Source Link**: Enable for framework source debugging
6. **Symbol Servers**: Configure for third-party library debugging
7. **Just My Code**: Disable to debug framework internals when needed
8. **Hot Reload**: Use `dotnet watch` for rapid iteration during debugging

## 🔍 Architecture-Specific Debugging

### SOLID Principles Validation
- **Single Responsibility**: Verify each class has focused breakpoints
- **Open/Closed**: Test strategy pattern by switching extractors at runtime
- **Dependency Inversion**: Inspect DI container to verify interface-based dependencies

### Design Pattern Debugging
- **Strategy Pattern**: Monitor `IDeviceSpecificExtractor` selection in registry
- **Factory Pattern**: Trace object creation in DI container
- **Repository Pattern**: Inspect `InputFormatParserRegistry` selection logic

### Async/Await Debugging
- Use Parallel Stacks view for async call chains
- Monitor `SynchronizationContext` in async methods
- Check for deadlocks in `Task.Wait()` or `.Result` calls
- Use `ConfigureAwait(false)` awareness when debugging


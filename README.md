# Signal Booster - DME Extraction Tool

A production-ready application that reads physician notes, extracts durable medical equipment (DME) information, and submits structured data to an external API.

## 🏗️ Architecture & Design Principles

This refactored solution follows **SOLID principles**:

- **Single Responsibility Principle**: Each class has a single, well-defined responsibility
- **Open/Closed Principle**: New device types can be added via the Strategy pattern without modifying existing code
- **Liskov Substitution Principle**: Interfaces are properly implemented and substitutable
- **Interface Segregation Principle**: Focused interfaces that clients depend only on what they need
- **Dependency Inversion Principle**: Dependencies are injected through interfaces, not concrete implementations

### Key Design Patterns

- **Strategy Pattern**: Device-specific extractors (`IDeviceSpecificExtractor`) allow extensibility
- **Dependency Injection**: All services are registered and resolved through Microsoft.Extensions.DependencyInjection
- **Repository/Registry Pattern**: `IDeviceExtractorRegistry` manages device-specific extractors

## 📁 Project Structure

```
TechnicalAssessment/
├── SignalBooster.sln                    # Solution file (references both projects)
├── SignalBooster/                       # Main application project
│   ├── SignalBooster.csproj            # Main project file
│   ├── Program.cs                      # Application entry point
│   ├── appsettings.json                # Configuration file
│   ├── Configuration/                   # Application configuration models
│   ├── DependencyInjection/             # DI container setup
│   ├── Models/                          # Domain models (DmeExtractionResult)
│   ├── Services/                        # Core business logic
│   │   ├── Interfaces/                  # Service interfaces (contracts)
│   │   ├── Extractors/                  # Extraction strategies (pattern & LLM)
│   │   │   └── Interfaces/              # Extractor interfaces (contracts)
│   │   ├── InputFormats/                # Input format parsers (plain text, JSON)
│   │   ├── ApiClient.cs                 # HTTP client for API submission
│   │   ├── DmeExtractor.cs              # Pattern-based extraction orchestrator
│   │   ├── DmeProcessingService.cs      # Main workflow coordinator
│   │   └── FileReader.cs                # File I/O operations with format detection
│   └── physician_note*.txt              # Sample input files
└── SignalBooster.Tests/                 # Test project
    ├── SignalBooster.Tests.csproj      # Test project file
    └── Tests/                           # Unit tests
```

## 🛠️ Tools & Technologies

### IDE & Development Tools
- **IDE**: Cursor (AI-powered code editor)
- **AI Development Tools**: Cursor AI Assistant
- **.NET SDK**: 8.0
- **Testing Framework**: xUnit
- **Mocking Framework**: Moq

### NuGet Packages
- Microsoft.Extensions.Logging
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Configuration
- Newtonsoft.Json
- Azure.AI.OpenAI (for LLM integration)
- xUnit, Moq (for testing)

## 🚀 Running the Project

### Prerequisites
- .NET 8.0 SDK or later
- A physician note file (or use the provided sample files)

### Build and Run

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the application (with default file)
dotnet run --project SignalBooster/SignalBooster.csproj

# Run with custom file path
dotnet run --project SignalBooster/SignalBooster.csproj -- physician_note1.txt

# Run with custom file and API endpoint
dotnet run --project SignalBooster/SignalBooster.csproj -- physician_note1.txt https://custom-api.com/endpoint

# Or build/test the entire solution
dotnet build SignalBooster.sln
dotnet test SignalBooster.sln
```

### Run Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal
```

## 📝 Configuration

The application supports configuration through `appsettings.json`:

```json
{
  "AppSettings": {
    "DefaultInputFile": "physician_note.txt",
    "DefaultApiEndpoint": "https://alert-api.com/DrExtract",
    "ExtractionMethod": "Pattern",
    "LlmSettings": {
      "Endpoint": null,
      "ApiKey": null,
      "ModelName": "gpt-4",
      "UseAzureOpenAi": false
    }
  }
}
```

### Configuration Options

- **ExtractionMethod**: Choose `"Pattern"` (pattern-based extraction) or `"LLM"` (AI-based extraction)
- **LlmSettings**: Configuration for LLM-based extraction
  - **Endpoint**: Azure OpenAI endpoint URL (for Azure) or OpenAI API base URL (optional for OpenAI)
  - **ApiKey**: API key for authentication (required for LLM mode)
  - **ModelName**: Model to use (e.g., "gpt-4", "gpt-3.5-turbo")
  - **UseAzureOpenAi**: Set to `true` for Azure OpenAI, `false` for OpenAI

### Input Formats

The application supports multiple input formats:
- **Plain Text**: Standard text files with physician notes
- **JSON-Wrapped**: JSON files with note text in a `data` field (e.g., `{"data": "note text"}`)

The application automatically detects the format and parses accordingly.

Command-line arguments override configuration:
1. First argument: Input file path
2. Second argument: API endpoint URL

## ✅ Completed Requirements

### ✅ Core Requirements
1. **Refactored into well-named, testable methods**
   - Separated concerns into focused services
   - Clear, consistent naming throughout
   - Removed all dead code and misleading comments

2. **Logging and error handling**
   - Comprehensive logging at appropriate levels (Information, Debug, Warning, Error)
   - Proper exception handling without swallowing errors
   - Meaningful error messages and context

3. **Unit tests**
   - Multiple unit tests covering extraction logic
   - Tests for CPAP and Oxygen Tank extractors
   - Tests demonstrate testability of the refactored architecture

4. **Clear documentation**
   - XML documentation comments on all public members
   - Helpful inline comments explaining business logic

5. **Functional requirements**
   - ✅ Reads physician notes from files
   - ✅ Extracts structured DME data
   - ✅ POSTs data to external API

### ✅ Stretch Goals Implemented
- ✅ **Configurability**: File path and API endpoint configurable via command-line args and `appsettings.json`
- ✅ **Extensible architecture**: Easy to add new device types via Strategy pattern
- ✅ **Enhanced extraction**: Extracts patient name, DOB, and diagnosis (beyond original requirements)
- ✅ **LLM Integration**: AI-based extraction using OpenAI or Azure OpenAI (configurable)
- ✅ **Multiple Input Formats**: Supports plain text and JSON-wrapped note formats (auto-detected)

## 🔍 Key Improvements Over Original Code

1. **Separation of Concerns**: Logic split into focused, single-responsibility classes
2. **Testability**: All services are testable through interfaces and dependency injection
3. **Extensibility**: New device types can be added by implementing `IDeviceSpecificExtractor`
4. **Error Handling**: Comprehensive error handling with proper logging
5. **Configuration**: Support for configuration files and command-line arguments
6. **Code Quality**: Clear naming, proper null checks, and comprehensive documentation

## 📋 Assumptions & Limitations

### Assumptions
- Physician notes are in plain text format
- API endpoint accepts JSON payloads in the expected format
- File encoding is UTF-8
- Network connectivity is available for API calls

### Limitations
- Pattern-based extraction may miss variations in note formatting (use LLM mode for better accuracy)
- No retry logic for failed API calls (could be added)
- No support for batch processing multiple files
- Limited device type support (CPAP, Oxygen Tank, Wheelchair) - easily extensible
- LLM integration requires API key and may incur costs

### Future Improvements

1. ✅ **LLM Integration**: Implemented - AI-based extraction using OpenAI/Azure OpenAI
2. ✅ **Multiple Input Formats**: Implemented - Supports JSON-wrapped notes (plain text already supported)
3. **PDF Parsing**: Add support for PDF input files
4. **Retry Logic**: Implement exponential backoff for API failures
4. **Batch Processing**: Process multiple files in a single run
5. **Validation**: Add input validation and schema validation for extracted data
6. **Metrics & Monitoring**: Add application metrics and health checks
7. **Async File Processing**: Support processing multiple files concurrently
8. **Configuration Validation**: Validate configuration on startup
9. **More Device Types**: Add extractors for wheelchairs, hospital beds, etc.
10. **Integration Tests**: Add end-to-end integration tests

## 📊 Example Usage

### Input File (physician_note1.txt)
```
Patient Name: Harold Finch
DOB: 04/12/1952
Diagnosis: COPD
Prescription: Requires a portable oxygen tank delivering 2 L per minute.
Usage: During sleep and exertion.
Ordering Physician: Dr. Cuddy
```

### Extracted Output (JSON)
```json
{
  "device": "Oxygen Tank",
  "liters": "2 L",
  "usage": "sleep and exertion",
  "diagnosis": "COPD",
  "ordering_provider": "Dr. Cuddy",
  "patient_name": "Harold Finch",
  "dob": "04/12/1952"
}
```

## 🧪 Testing

The solution includes comprehensive unit tests demonstrating:
- Extraction logic for different device types
- Edge cases (empty input, unknown devices)
- Mocking of dependencies for isolated testing

Run tests with:
```bash
dotnet test
```


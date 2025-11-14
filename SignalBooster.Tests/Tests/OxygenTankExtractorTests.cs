using Microsoft.Extensions.Logging;
using Moq;
using SignalBooster.Models;
using SignalBooster.Services.Extractors;
using Xunit;

namespace SignalBooster.Tests;

/// <summary>
/// Unit tests for Oxygen Tank-specific extraction logic.
/// </summary>
public class OxygenTankExtractorTests
{
    private readonly Mock<ILogger<OxygenTankExtractor>> _loggerMock;
    private readonly OxygenTankExtractor _extractor;

    public OxygenTankExtractorTests()
    {
        _loggerMock = new Mock<ILogger<OxygenTankExtractor>>();
        _extractor = new OxygenTankExtractor(_loggerMock.Object);
    }

    [Fact]
    public void ExtractDeviceSpecificFields_WithLiters_ExtractsLiters()
    {
        // Arrange
        var noteText = "Patient requires portable oxygen tank delivering 2.5 L per minute.";
        var result = new DmeExtractionResult { Device = "Oxygen Tank" };

        // Act
        _extractor.ExtractDeviceSpecificFields(noteText, result);

        // Assert
        Assert.Equal("2.5 L", result.Liters);
    }

    [Fact]
    public void ExtractDeviceSpecificFields_WithSleepAndExertion_ExtractsUsage()
    {
        // Arrange
        var noteText = "Oxygen required during sleep and exertion.";
        var result = new DmeExtractionResult { Device = "Oxygen Tank" };

        // Act
        _extractor.ExtractDeviceSpecificFields(noteText, result);

        // Assert
        Assert.Equal("sleep and exertion", result.Usage);
    }

    [Fact]
    public void ExtractDeviceSpecificFields_WithSleepOnly_ExtractsUsage()
    {
        // Arrange
        var noteText = "Oxygen needed during sleep.";
        var result = new DmeExtractionResult { Device = "Oxygen Tank" };

        // Act
        _extractor.ExtractDeviceSpecificFields(noteText, result);

        // Assert
        Assert.Equal("sleep", result.Usage);
    }
}


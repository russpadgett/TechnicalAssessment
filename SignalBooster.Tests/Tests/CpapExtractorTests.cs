using Microsoft.Extensions.Logging;
using Moq;
using SignalBooster.Models;
using SignalBooster.Services.Extractors;
using Xunit;

namespace SignalBooster.Tests;

/// <summary>
/// Unit tests for CPAP-specific extraction logic.
/// </summary>
public class CpapExtractorTests
{
    private readonly Mock<ILogger<CpapExtractor>> _loggerMock;
    private readonly CpapExtractor _extractor;

    public CpapExtractorTests()
    {
        _loggerMock = new Mock<ILogger<CpapExtractor>>();
        _extractor = new CpapExtractor(_loggerMock.Object);
    }

    [Fact]
    public void ExtractDeviceSpecificFields_WithFullFaceMask_ExtractsMaskType()
    {
        // Arrange
        var noteText = "Patient needs CPAP with full face mask and humidifier.";
        var result = new DmeExtractionResult { Device = "CPAP" };

        // Act
        _extractor.ExtractDeviceSpecificFields(noteText, result);

        // Assert
        Assert.Equal("full face", result.MaskType);
        Assert.NotNull(result.AddOns);
        Assert.Contains("humidifier", result.AddOns);
    }

    [Fact]
    public void ExtractDeviceSpecificFields_WithHeatedHumidifier_ExtractsCorrectAddOn()
    {
        // Arrange
        var noteText = "CPAP therapy with heated humidifier recommended.";
        var result = new DmeExtractionResult { Device = "CPAP" };

        // Act
        _extractor.ExtractDeviceSpecificFields(noteText, result);

        // Assert
        Assert.NotNull(result.AddOns);
        Assert.Contains("heated humidifier", result.AddOns);
    }

    [Fact]
    public void ExtractDeviceSpecificFields_WithAHIQualifier_ExtractsQualifier()
    {
        // Arrange
        var noteText = "Patient has severe sleep apnea. AHI > 20. CPAP recommended.";
        var result = new DmeExtractionResult { Device = "CPAP" };

        // Act
        _extractor.ExtractDeviceSpecificFields(noteText, result);

        // Assert
        Assert.NotNull(result.Qualifier);
        Assert.Contains("AHI", result.Qualifier);
        Assert.Contains("20", result.Qualifier);
    }

    [Fact]
    public void ExtractDeviceSpecificFields_WithNasalPillow_ExtractsCorrectMaskType()
    {
        // Arrange
        var noteText = "CPAP with nasal pillow mask.";
        var result = new DmeExtractionResult { Device = "CPAP" };

        // Act
        _extractor.ExtractDeviceSpecificFields(noteText, result);

        // Assert
        Assert.Equal("nasal pillow", result.MaskType);
    }
}


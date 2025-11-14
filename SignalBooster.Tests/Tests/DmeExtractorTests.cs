using Microsoft.Extensions.Logging;
using Moq;
using SignalBooster.Models;
using SignalBooster.Services;
using SignalBooster.Services.Extractors;
using Xunit;

namespace SignalBooster.Tests;

/// <summary>
/// Unit tests for DME extraction functionality.
/// Demonstrates testability of the refactored code.
/// </summary>
public class DmeExtractorTests
{
    private readonly Mock<ILogger<DmeExtractor>> _loggerMock;
    private readonly Mock<ICommonFieldExtractor> _commonFieldExtractorMock;
    private readonly Mock<IDeviceExtractorRegistry> _deviceExtractorRegistryMock;

    public DmeExtractorTests()
    {
        _loggerMock = new Mock<ILogger<DmeExtractor>>();
        _commonFieldExtractorMock = new Mock<ICommonFieldExtractor>();
        _deviceExtractorRegistryMock = new Mock<IDeviceExtractorRegistry>();
    }

    [Fact]
    public void ExtractDmeInfo_WithValidNote_ReturnsExtractedData()
    {
        // Arrange
        var noteText = "Patient Name: John Doe\nDOB: 01/01/1980\nDiagnosis: Sleep Apnea\n" +
                      "Patient needs a CPAP with full face mask and heated humidifier. AHI > 20. " +
                      "Ordering Physician: Dr. Smith";

        var commonFields = new DmeExtractionResult
        {
            Device = "CPAP",
            OrderingProvider = "Dr. Smith",
            PatientName = "John Doe",
            DateOfBirth = "01/01/1980",
            Diagnosis = "Sleep Apnea"
        };

        var cpapExtractor = new Mock<IDeviceSpecificExtractor>();
        cpapExtractor.Setup(e => e.DeviceType).Returns("CPAP");
        cpapExtractor.Setup(e => e.ExtractDeviceSpecificFields(
            It.IsAny<string>(), 
            It.IsAny<DmeExtractionResult>()));

        _commonFieldExtractorMock
            .Setup(x => x.ExtractCommonFields(noteText))
            .Returns(commonFields);

        _deviceExtractorRegistryMock
            .Setup(x => x.GetExtractor("CPAP"))
            .Returns(cpapExtractor.Object);

        var extractor = new DmeExtractor(
            _commonFieldExtractorMock.Object,
            _deviceExtractorRegistryMock.Object,
            _loggerMock.Object);

        // Act
        var result = extractor.ExtractDmeInfo(noteText);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("CPAP", result.Device);
        Assert.Equal("Dr. Smith", result.OrderingProvider);
        Assert.Equal("John Doe", result.PatientName);
        Assert.Equal("01/01/1980", result.DateOfBirth);
        Assert.Equal("Sleep Apnea", result.Diagnosis);

        // Verify device-specific extraction was called
        cpapExtractor.Verify(
            e => e.ExtractDeviceSpecificFields(noteText, It.IsAny<DmeExtractionResult>()),
            Times.Once);
    }

    [Fact]
    public void ExtractDmeInfo_WithEmptyNote_ReturnsEmptyResult()
    {
        // Arrange
        var extractor = new DmeExtractor(
            _commonFieldExtractorMock.Object,
            _deviceExtractorRegistryMock.Object,
            _loggerMock.Object);

        // Act
        var result = extractor.ExtractDmeInfo(string.Empty);

        // Assert
        Assert.NotNull(result);
        _commonFieldExtractorMock.Verify(
            x => x.ExtractCommonFields(It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public void ExtractDmeInfo_WithUnknownDevice_DoesNotCallDeviceSpecificExtractor()
    {
        // Arrange
        var noteText = "Patient needs some medical equipment. Ordered by Dr. Unknown.";

        var commonFields = new DmeExtractionResult
        {
            Device = "Unknown",
            OrderingProvider = "Dr. Unknown"
        };

        _commonFieldExtractorMock
            .Setup(x => x.ExtractCommonFields(noteText))
            .Returns(commonFields);

        _deviceExtractorRegistryMock
            .Setup(x => x.GetExtractor("Unknown"))
            .Returns((IDeviceSpecificExtractor?)null);

        var extractor = new DmeExtractor(
            _commonFieldExtractorMock.Object,
            _deviceExtractorRegistryMock.Object,
            _loggerMock.Object);

        // Act
        var result = extractor.ExtractDmeInfo(noteText);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Unknown", result.Device);
        _deviceExtractorRegistryMock.Verify(
            x => x.GetExtractor("Unknown"),
            Times.Once);
    }
}


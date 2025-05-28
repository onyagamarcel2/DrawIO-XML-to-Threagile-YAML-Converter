using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using ThreagileConverter.Core.Parsers;
using Xunit;

namespace ThreagileConverter.Tests.Parsers;

public class DrawIOParserTests
{
    private readonly Mock<ILogger<DrawIOParser>> _loggerMock;
    private readonly DrawIOParser _parser;

    public DrawIOParserTests()
    {
        _loggerMock = new Mock<ILogger<DrawIOParser>>();
        _parser = new DrawIOParser(_loggerMock.Object);
    }

    [Fact]
    public async Task ParseAsync_EmptyXml_ReturnsEmptyModel()
    {
        // Arrange
        var xml = "<diagram></diagram>";

        // Act
        var result = await _parser.ParseAsync(xml);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Shapes);
        Assert.Empty(result.Relations);
    }
} 
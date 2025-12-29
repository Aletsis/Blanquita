using Blanquita.Infrastructure.Services.Parsing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Blanquita.Infrastructure.Tests.Services.Parsing;

public class DbfStringParserTests
{
    private readonly Mock<ILogger<DbfStringParser>> _loggerMock;
    private readonly DbfStringParser _parser;

    public DbfStringParserTests()
    {
        _loggerMock = new Mock<ILogger<DbfStringParser>>();
        _parser = new DbfStringParser(_loggerMock.Object);
    }

    [Fact]
    public void ParsearDocumentos_ShouldReturnEmpty_WhenStringEmpty()
    {
        var result = _parser.ParsearDocumentos("");

        Assert.Empty(result);
    }

    [Fact]
    public void ParsearDocumentos_ShouldReturnEmpty_WhenStringNull()
    {
        var result = _parser.ParsearDocumentos(null!);

        Assert.Empty(result);
    }

    [Fact]
    public void ParsearDocumentos_ShouldReturnEmpty_WhenStringTooShort()
    {
        var result = _parser.ParsearDocumentos("Short");

        Assert.Empty(result);
    }

    [Fact]
    public void ParsearDocumentos_ShouldParseSingleLine_WhenFormatValid()
    {
        // Format: IdDocumento(10) + Serie(20) + Folio(resto)
        var input = "4         FGIFS               4821";

        var result = _parser.ParsearDocumentos(input);

        Assert.Single(result);
        Assert.Equal("4", result[0].IdDocumento);
        Assert.Equal("FGIFS", result[0].Serie);
        Assert.Equal("4821", result[0].Folio);
    }

    [Fact]
    public void ParsearDocumentos_ShouldParseMultipleLines_WhenNewlinesPresent()
    {
        var input = "1         SERIE1              100\n2         SERIE2              200";

        var result = _parser.ParsearDocumentos(input);

        Assert.Equal(2, result.Count);
        Assert.Equal("1", result[0].IdDocumento);
        Assert.Equal("SERIE1", result[0].Serie);
        Assert.Equal("100", result[0].Folio);
        Assert.Equal("2", result[1].IdDocumento);
        Assert.Equal("SERIE2", result[1].Serie);
        Assert.Equal("200", result[1].Folio);
    }

    [Fact]
    public void ParsearDocumentos_ShouldSkipInvalidLines_InMultilineInput()
    {
        var input = "1         SERIE1              100\nTooShort\n2         SERIE2              200";

        var result = _parser.ParsearDocumentos(input);

        Assert.Equal(2, result.Count);
        Assert.Equal("1", result[0].IdDocumento);
        Assert.Equal("2", result[1].IdDocumento);
    }

    [Fact]
    public void ParsearDocumentos_ShouldHandleExtraSpaces()
    {
        var input = "123       MYSERIE             9999";

        var result = _parser.ParsearDocumentos(input);

        Assert.Single(result);
        Assert.Equal("123", result[0].IdDocumento);
        Assert.Equal("MYSERIE", result[0].Serie);
        Assert.Equal("9999", result[0].Folio);
    }
}

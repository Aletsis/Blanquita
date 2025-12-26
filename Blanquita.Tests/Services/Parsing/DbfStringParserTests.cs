using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Blanquita.Services.Parsing;
using Blanquita.Models;
using System.Collections.Generic;
using System.Linq;

namespace Blanquita.Tests.Services.Parsing
{
    public class DbfStringParserTests
    {
        private readonly Mock<ILogger<DbfStringParser>> _mockLogger;
        private readonly DbfStringParser _parser;

        public DbfStringParserTests()
        {
            _mockLogger = new Mock<ILogger<DbfStringParser>>();
            _parser = new DbfStringParser(_mockLogger.Object);
        }

        [Fact]
        public void ParsearDocumentos_EmptyString_ReturnsEmptyList()
        {
            var result = _parser.ParsearDocumentos("");
            Assert.Empty(result);
        }

        [Fact]
        public void ParsearDocumentos_NullString_ReturnsEmptyList()
        {
            var result = _parser.ParsearDocumentos(null);
            Assert.Empty(result);
        }

        [Fact]
        public void ParsearDocumentos_SingleLine_ValidFormat_ReturnsOneDocument()
        {
            // "1234567890          FGIFS                 4821"
            string input = "1234567890          FGIFS                 4821";
            var result = _parser.ParsearDocumentos(input);

            Assert.Single(result);
            var doc = result.First();
            Assert.Equal("1234567890", doc.IdDocumento);
            Assert.Equal("FGIFS", doc.Serie);
            Assert.Equal("4821", doc.Folio);
        }

        [Fact]
        public void ParsearDocumentos_MultipleLines_ValidFormat_ReturnsMultipleDocuments()
        {
            string input = "1234567890          FGIFS                 4821\n" +
                           "0987654321          COH                   1234";
            var result = _parser.ParsearDocumentos(input);

            Assert.Equal(2, result.Count);
            Assert.Equal("1234567890", result[0].IdDocumento);
            Assert.Equal("FGIFS", result[0].Serie);
            Assert.Equal("0987654321", result[1].IdDocumento);
            Assert.Equal("COH", result[1].Serie);
        }

         [Fact]
        public void ParsearDocumentos_ShortLine_Ignored()
        {
            string input = "TooShort";
            var result = _parser.ParsearDocumentos(input);
            Assert.Empty(result);
        }

        [Fact]
        public void ParsearDocumentos_MixedValidAndInvalidLines_ReturnsOnlyValid()
        {
            string input = "1234567890          FGIFS                 4821\n" +
                           "Short\n" +
                           "0987654321          COH                   1234";
            
            var result = _parser.ParsearDocumentos(input);

            Assert.Equal(2, result.Count);
        }
    }
}

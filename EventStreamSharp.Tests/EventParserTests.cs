using EventStreamSharp.Domain;
using EventStreamSharp.Ingest;
using Xunit;

namespace EventStreamSharp.Tests
{
    public class EventParserTests
    {
        [Fact]
        public void Parse_ComLinhaValida_DeveRetornarEventoCorreto()
        {
            // Arrange
            var parser = new EventParser();
            var linhaCsv = "2025-11-28T10:00:00Z,auth-service,login,120,500,true";
            
            // Act
            var resultado = parser.Parse(linhaCsv);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal("auth-service", resultado.ServiceName);
            Assert.Equal(120, resultado.DurationMs);
            Assert.True(resultado.Success);
        }

        [Fact]
        public void Parse_ComLinhaInvalida_DeveRetornarNulo()
        {
            // Arrange
            var parser = new EventParser();
            var linhaCsvInvalida = "2025-11-28T10:00:00Z,auth-service,login,120"; // faltando campos

            // Act
            var resultado = parser.Parse(linhaCsvInvalida);

            // Assert
            Assert.Null(resultado);
        }
    }
}

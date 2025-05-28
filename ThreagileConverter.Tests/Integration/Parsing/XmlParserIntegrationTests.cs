using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Xunit;
using ThreagileConverter.Core.Parsing;

namespace ThreagileConverter.Tests.Integration.Parsing
{
    /// <summary>
    /// Tests d'intégration pour la classe XmlParser.
    /// Ces tests vérifient l'interaction du parser avec le système de fichiers et les dépendances réelles.
    /// </summary>
    public class XmlParserIntegrationTests : IDisposable
    {
        private readonly ILogger<XmlParser> _logger;
        private readonly XmlParser _parser;
        private readonly string _testDataPath;

        public XmlParserIntegrationTests()
        {
            // Configuration du logger réel
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddConsole()
                    .SetMinimumLevel(LogLevel.Debug);
            });
            _logger = loggerFactory.CreateLogger<XmlParser>();

            _parser = new XmlParser(_logger);
            _testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "Xml");
            Directory.CreateDirectory(_testDataPath);
        }

        public void Dispose()
        {
            if (Directory.Exists(_testDataPath))
            {
                Directory.Delete(_testDataPath, true);
            }
        }

        #region Tests d'Intégration avec le Système de Fichiers

        /// <summary>
        /// Vérifie que le parser peut lire et traiter un fichier XML depuis le système de fichiers.
        /// </summary>
        [Fact]
        public void ParseXml_WithRealFileSystem_ReturnsXDocument()
        {
            // Arrange
            var filePath = Path.Combine(_testDataPath, "test.xml");
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root>
    <element>test</element>
</root>";
            File.WriteAllText(filePath, xmlContent);

            // Act
            var result = _parser.ParseXml(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("root", result.Root.Name.LocalName);
            Assert.Equal("test", result.Root.Element("element").Value);
        }

        /// <summary>
        /// Vérifie que le parser gère correctement les permissions de fichier.
        /// </summary>
        [Fact]
        public void ParseXml_WithFilePermissions_HandlesCorrectly()
        {
            // Arrange
            var filePath = Path.Combine(_testDataPath, "permissions.xml");
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root>
    <element>test</element>
</root>";
            File.WriteAllText(filePath, xmlContent);
            var fileInfo = new FileInfo(filePath);
            fileInfo.IsReadOnly = true;

            try
            {
                // Act
                var result = _parser.ParseXml(filePath);

                // Assert
                Assert.NotNull(result);
                Assert.Equal("root", result.Root.Name.LocalName);
            }
            finally
            {
                fileInfo.IsReadOnly = false;
            }
        }

        #endregion

        #region Tests d'Intégration avec les Dépendances

        /// <summary>
        /// Vérifie que le parser peut traiter un fichier XML avec des caractères spéciaux.
        /// </summary>
        [Fact]
        public void ParseXml_WithSpecialCharacters_HandlesCorrectly()
        {
            // Arrange
            var filePath = Path.Combine(_testDataPath, "special.xml");
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root>
    <element>éèàçù€£¥</element>
</root>";
            File.WriteAllText(filePath, xmlContent);

            // Act
            var result = _parser.ParseXml(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("éèàçù€£¥", result.Root.Element("element").Value);
        }

        /// <summary>
        /// Vérifie que le parser peut traiter un fichier XML avec des entités XML.
        /// </summary>
        [Fact]
        public void ParseXml_WithXmlEntities_HandlesCorrectly()
        {
            // Arrange
            var filePath = Path.Combine(_testDataPath, "entities.xml");
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root>
    <element>&lt;&gt;&amp;&quot;&apos;</element>
</root>";
            File.WriteAllText(filePath, xmlContent);

            // Act
            var result = _parser.ParseXml(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("<>\"'", result.Root.Element("element").Value);
        }

        #endregion

        #region Tests de Performance en Conditions Réelles

        /// <summary>
        /// Vérifie que le parser peut gérer un grand fichier XML en conditions réelles.
        /// </summary>
        [Fact]
        public void ParseXml_WithLargeFileInRealConditions_HandlesCorrectly()
        {
            // Arrange
            var filePath = Path.Combine(_testDataPath, "large.xml");
            var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<root>";
            for (int i = 0; i < 10000; i++)
            {
                xmlContent += $@"
    <element id=""{i}"">
        <data>{new string('x', 1000)}</data>
    </element>";
            }
            xmlContent += @"
</root>";
            File.WriteAllText(filePath, xmlContent);

            // Act
            var result = _parser.ParseXml(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10000, result.Root.Elements("element").Count());
        }

        #endregion
    }
} 
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using ThreagileConverter.Core.Factories;
using ThreagileConverter.Core.Parsing;
using ThreagileConverter.Core.Validation;
using ThreagileConverter.Core.Generation;

namespace ThreagileConverter.IntegrationTests.Factories
{
    public class ParserFactoryIntegrationTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ParserFactory _factory;

        public ParserFactoryIntegrationTests()
        {
            var services = new ServiceCollection();
            
            // Enregistrer les services
            services.AddSingleton<ILogger<ParserFactory>>(provider =>
                LoggerFactory.Create(builder => builder.AddConsole())
                    .CreateLogger<ParserFactory>());
            services.AddSingleton<ILogger<XmlParser>>(provider =>
                LoggerFactory.Create(builder => builder.AddConsole())
                    .CreateLogger<XmlParser>());
            services.AddSingleton<ILogger<YamlParser>>(provider =>
                LoggerFactory.Create(builder => builder.AddConsole())
                    .CreateLogger<YamlParser>());
            services.AddSingleton<ILogger<XmlValidator>>(provider =>
                LoggerFactory.Create(builder => builder.AddConsole())
                    .CreateLogger<XmlValidator>());
            services.AddSingleton<ILogger<YamlValidator>>(provider =>
                LoggerFactory.Create(builder => builder.AddConsole())
                    .CreateLogger<YamlValidator>());
            services.AddSingleton<ILogger<YamlGenerator>>(provider =>
                LoggerFactory.Create(builder => builder.AddConsole())
                    .CreateLogger<YamlGenerator>());

            // Enregistrer les implémentations
            services.AddSingleton<XmlParser>();
            services.AddSingleton<YamlParser>();
            services.AddSingleton<XmlValidator>();
            services.AddSingleton<YamlValidator>();
            services.AddSingleton<YamlGenerator>();

            _serviceProvider = services.BuildServiceProvider();
            _factory = new ParserFactory(
                _serviceProvider.GetRequiredService<ILogger<ParserFactory>>(),
                _serviceProvider);
        }

        [Fact]
        public void CreateParser_WithXmlType_ReturnsXmlParser()
        {
            // Act
            var parser = _factory.CreateParser("xml");

            // Assert
            Assert.NotNull(parser);
            Assert.IsType<XmlParser>(parser);
        }

        [Fact]
        public void CreateParser_WithYamlType_ReturnsYamlParser()
        {
            // Act
            var parser = _factory.CreateParser("yaml");

            // Assert
            Assert.NotNull(parser);
            Assert.IsType<YamlParser>(parser);
        }

        [Fact]
        public void CreateValidator_WithXmlType_ReturnsXmlValidator()
        {
            // Act
            var validator = _factory.CreateValidator("xml");

            // Assert
            Assert.NotNull(validator);
            Assert.IsType<XmlValidator>(validator);
        }

        [Fact]
        public void CreateValidator_WithYamlType_ReturnsYamlValidator()
        {
            // Act
            var validator = _factory.CreateValidator("yaml");

            // Assert
            Assert.NotNull(validator);
            Assert.IsType<YamlValidator>(validator);
        }

        [Fact]
        public void CreateGenerator_WithYamlType_ReturnsYamlGenerator()
        {
            // Act
            var generator = _factory.CreateGenerator("yaml");

            // Assert
            Assert.NotNull(generator);
            Assert.IsType<YamlGenerator>(generator);
        }

        [Fact]
        public void CreateParser_WithInvalidType_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _factory.CreateParser("invalid"));
        }

        [Fact]
        public void CreateValidator_WithInvalidType_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _factory.CreateValidator("invalid"));
        }

        [Fact]
        public void CreateGenerator_WithInvalidType_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _factory.CreateGenerator("invalid"));
        }

        [Fact]
        public void CreateParser_WithXmlType_ReturnsSameInstance()
        {
            // Act
            var parser1 = _factory.CreateParser("xml");
            var parser2 = _factory.CreateParser("xml");

            // Assert
            Assert.Same(parser1, parser2);
        }

        [Fact]
        public void CreateValidator_WithXmlType_ReturnsSameInstance()
        {
            // Act
            var validator1 = _factory.CreateValidator("xml");
            var validator2 = _factory.CreateValidator("xml");

            // Assert
            Assert.Same(validator1, validator2);
        }

        [Fact]
        public void CreateGenerator_WithYamlType_ReturnsSameInstance()
        {
            // Act
            var generator1 = _factory.CreateGenerator("yaml");
            var generator2 = _factory.CreateGenerator("yaml");

            // Assert
            Assert.Same(generator1, generator2);
        }
    }
} 
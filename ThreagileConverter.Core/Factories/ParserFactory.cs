using System;
using Microsoft.Extensions.Logging;
using ThreagileConverter.Core.Parsing;
using ThreagileConverter.Core.Validation;
using ThreagileConverter.Core.Generation;

namespace ThreagileConverter.Core.Factories
{
    /// <summary>
    /// Implementation of the factory for creating different types of parsers
    /// </summary>
    public class ParserFactory : IParserFactory
    {
        private readonly ILogger<ParserFactory> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ParserFactory(ILogger<ParserFactory> logger, IServiceProvider serviceProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IParser CreateParser(string type)
        {
            _logger.LogDebug("Creating parser of type {Type}", type);

            return type.ToLower() switch
            {
                "xml" => CreateXmlParser(),
                _ => throw new ArgumentException($"Unsupported parser type: {type}", nameof(type))
            };
        }

        public IValidator CreateValidator(string type)
        {
            _logger.LogDebug("Creating validator of type {Type}", type);

            return type.ToLower() switch
            {
                "xml" => CreateXmlValidator(),
                _ => throw new ArgumentException($"Unsupported validator type: {type}", nameof(type))
            };
        }

        public IGenerator CreateGenerator(string type)
        {
            _logger.LogDebug("Creating generator of type {Type}", type);

            return type.ToLower() switch
            {
                "yaml" => CreateYamlGenerator(),
                _ => throw new ArgumentException($"Unsupported generator type: {type}", nameof(type))
            };
        }

        private IParser CreateXmlParser()
        {
            return _serviceProvider.GetService(typeof(XmlParser)) as IParser
                ?? throw new InvalidOperationException("Failed to create XML parser");
        }

        private IValidator CreateXmlValidator()
        {
            return _serviceProvider.GetService(typeof(XmlValidator)) as IValidator
                ?? throw new InvalidOperationException("Failed to create XML validator");
        }

        private IGenerator CreateYamlGenerator()
        {
            return _serviceProvider.GetService(typeof(YamlGenerator)) as IGenerator
                ?? throw new InvalidOperationException("Failed to create YAML generator");
        }
    }
} 
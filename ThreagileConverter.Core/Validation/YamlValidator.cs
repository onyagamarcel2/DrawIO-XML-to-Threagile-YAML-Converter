using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using ThreagileConverter.Core.Models;
using System.Collections.Generic;

namespace ThreagileConverter.Core.Validation;

/// <summary>
/// Implementation of the YAML validator
/// </summary>
public class YamlValidator : IValidator
{
    private readonly ILogger<YamlValidator> _logger;
    private readonly IDeserializer _deserializer;

    public YamlValidator(ILogger<YamlValidator> logger)
    {
        _logger = logger;
        _deserializer = new DeserializerBuilder()
            .Build();
    }

    /// <summary>
    /// Validates a YAML document
    /// </summary>
    /// <param name="yamlContent">The YAML content to validate</param>
    /// <returns>The validation result</returns>
    public async Task<ValidationResult> ValidateAsync(string yamlContent)
    {
        _logger.LogInformation("Validating YAML content");
        var model = _deserializer.Deserialize<ThreagileModel>(yamlContent);
        return await ValidateModelAsync(model);
    }

    /// <summary>
    /// Validates an XML document
    /// </summary>
    /// <param name="xmlDocument">The XML document to validate</param>
    /// <returns>The validation result</returns>
    public Task<ValidationResult> ValidateAsync(XDocument xmlDocument)
    {
        throw new NotImplementedException("XML validation is not supported by the YAML validator");
    }

    /// <summary>
    /// Validates an XML document against a schema
    /// </summary>
    /// <param name="xmlDocument">The XML document to validate</param>
    /// <param name="schemaPath">The path to the schema file</param>
    /// <returns>The validation result</returns>
    public Task<ValidationResult> ValidateAgainstSchemaAsync(XDocument xmlDocument, string schemaPath)
    {
        throw new NotImplementedException("XML schema validation is not supported by the YAML validator");
    }

    /// <summary>
    /// Validates a ThreagileModel
    /// </summary>
    /// <param name="model">The model to validate</param>
    /// <returns>The validation result</returns>
    private async Task<ValidationResult> ValidateModelAsync(ThreagileModel model)
    {
        var errors = new List<ValidationError>();
        var warnings = new List<ValidationWarning>();

        if (model.TechnicalAssets == null)
        {
            errors.Add(new ValidationError("No technical assets found", ValidationSeverity.Error, "TechnicalAssets"));
        }
        else
        {
            foreach (var asset in model.TechnicalAssets)
            {
                if (string.IsNullOrEmpty(asset.Id))
                {
                    errors.Add(new ValidationError("Technical asset has no ID", ValidationSeverity.Error, $"TechnicalAssets[{asset.Name}]"));
                }
                if (string.IsNullOrEmpty(asset.Name))
                {
                    errors.Add(new ValidationError("Technical asset has no name", ValidationSeverity.Error, $"TechnicalAssets[{asset.Id}]"));
                }
            }
        }

        if (model.TrustBoundaries == null)
        {
            warnings.Add(new ValidationWarning("No trust boundaries found", "W001", "TrustBoundaries"));
        }
        else
        {
            foreach (var boundary in model.TrustBoundaries)
            {
                if (string.IsNullOrEmpty(boundary.Id))
                {
                    errors.Add(new ValidationError("Trust boundary has no ID", ValidationSeverity.Error, $"TrustBoundaries[{boundary.Name}]"));
                }
                if (string.IsNullOrEmpty(boundary.Name))
                {
                    errors.Add(new ValidationError("Trust boundary has no name", ValidationSeverity.Error, $"TrustBoundaries[{boundary.Id}]"));
                }
            }
        }

        if (model.CommunicationLinks == null)
        {
            warnings.Add(new ValidationWarning("No communication links found", "W002", "CommunicationLinks"));
        }
        else
        {
            foreach (var link in model.CommunicationLinks)
            {
                if (string.IsNullOrEmpty(link.Id))
                {
                    errors.Add(new ValidationError("Communication link has no ID", ValidationSeverity.Error, $"CommunicationLinks[{link.Name}]"));
                }
                if (string.IsNullOrEmpty(link.Name))
                {
                    errors.Add(new ValidationError("Communication link has no name", ValidationSeverity.Error, $"CommunicationLinks[{link.Id}]"));
                }
                if (string.IsNullOrEmpty(link.SourceId))
                {
                    errors.Add(new ValidationError("Communication link has no source ID", ValidationSeverity.Error, $"CommunicationLinks[{link.Id}]"));
                }
                if (string.IsNullOrEmpty(link.TargetId))
                {
                    errors.Add(new ValidationError("Communication link has no target ID", ValidationSeverity.Error, $"CommunicationLinks[{link.Id}]"));
                }
            }
        }

        if (errors.Count > 0)
        {
            return ValidationResult.CreateFailure(errors);
        }
        
        if (warnings.Count > 0)
        {
            return ValidationResult.WithWarnings(warnings);
        }

        return ValidationResult.Success();
    }
} 
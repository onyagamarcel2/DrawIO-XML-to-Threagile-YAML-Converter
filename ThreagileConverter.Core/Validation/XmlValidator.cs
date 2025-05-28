using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using System.Xml.Schema;
using System;
using System.Collections.Generic;
using System.IO;

namespace ThreagileConverter.Core.Validation;

/// <summary>
/// Implémentation du validateur XML
/// </summary>
public class XmlValidator : IValidator
{
    private readonly ILogger<XmlValidator> _logger;

    public XmlValidator(ILogger<XmlValidator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ValidationResult> ValidateAsync(XDocument document)
    {
        _logger.LogInformation("Validating XML document");
        try
        {
            if (document?.Root == null)
            {
                return ValidationResult.CreateFailure(new[] { new ValidationError("Invalid XML document: root element is null", ValidationSeverity.Error) });
            }

            var errors = new List<ValidationError>();
            var warnings = new List<ValidationWarning>();

            // Validate basic structure
            if (document.Root.Name.LocalName != "mxfile")
            {
                errors.Add(new ValidationError("Root element must be 'mxfile'", ValidationSeverity.Error));
            }

            var diagram = document.Root.Element("diagram");
            if (diagram == null)
            {
                errors.Add(new ValidationError("Missing 'diagram' element", ValidationSeverity.Error));
            }
            else
            {
                // Validate diagram attributes
                if (string.IsNullOrEmpty(diagram.Attribute("id")?.Value))
                {
                    warnings.Add(new ValidationWarning("Diagram missing 'id' attribute", "WARN_MISSING_ID"));
                }

                // Validate mxGraphModel
                var graphModel = diagram.Element("mxGraphModel");
                if (graphModel == null)
                {
                    errors.Add(new ValidationError("Missing 'mxGraphModel' element", ValidationSeverity.Error));
                }
                else
                {
                    var root = graphModel.Element("root");
                    if (root == null)
                    {
                        errors.Add(new ValidationError("Missing 'root' element", ValidationSeverity.Error));
                    }
                    else
                    {
                        // Validate cells
                        var cells = root.Elements("mxCell").ToList();
                        if (!cells.Any())
                        {
                            warnings.Add(new ValidationWarning("No cells found in diagram", "WARN_NO_CELLS"));
                        }
                        else
                        {
                            foreach (var cell in cells)
                            {
                                ValidateCell(cell, errors, warnings);
                            }
                        }
                    }
                }
            }

            return errors.Any()
                ? ValidationResult.CreateFailure(errors)
                : warnings.Any()
                    ? ValidationResult.WithWarnings(warnings)
                    : ValidationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating XML document");
            return ValidationResult.CreateFailure(new[] { new ValidationError($"Error validating XML: {ex.Message}", ValidationSeverity.Error) });
        }
    }

    public async Task<ValidationResult> ValidateAgainstSchemaAsync(XDocument document, string schemaPath)
    {
        _logger.LogInformation("Validating XML document against schema: {SchemaPath}", schemaPath);
        try
        {
            if (!File.Exists(schemaPath))
            {
                return ValidationResult.CreateFailure(new[] { new ValidationError($"Schema file not found: {schemaPath}", ValidationSeverity.Error) });
            }

            var schema = new XmlSchemaSet();
            schema.Add(null, schemaPath);

            var errors = new List<ValidationError>();
            document.Validate(schema, (sender, args) =>
            {
                var severity = args.Severity == XmlSeverityType.Error ? ValidationSeverity.Error : ValidationSeverity.Warning;
                errors.Add(new ValidationError(args.Message, severity, args.Exception?.SourceUri));
            });

            return errors.Any()
                ? ValidationResult.CreateFailure(errors)
                : ValidationResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating XML against schema");
            return ValidationResult.CreateFailure(new[] { new ValidationError($"Error validating XML against schema: {ex.Message}", ValidationSeverity.Error) });
        }
    }

    private void ValidateCell(XElement cell, List<ValidationError> errors, List<ValidationWarning> warnings)
    {
        var id = cell.Attribute("id")?.Value;
        if (string.IsNullOrEmpty(id))
        {
            errors.Add(new ValidationError("Cell missing 'id' attribute", ValidationSeverity.Error));
            return;
        }

        // Validate vertex cells
        if (cell.Attribute("vertex")?.Value == "1")
        {
            ValidateVertexCell(cell, errors, warnings);
        }
        // Validate edge cells
        else if (cell.Attribute("edge")?.Value == "1")
        {
            ValidateEdgeCell(cell, errors, warnings);
        }
    }

    private void ValidateVertexCell(XElement cell, List<ValidationError> errors, List<ValidationWarning> warnings)
    {
        var id = cell.Attribute("id")?.Value;
        var value = cell.Attribute("value")?.Value;
        var style = cell.Attribute("style")?.Value;

        if (string.IsNullOrEmpty(value))
        {
            warnings.Add(new ValidationWarning($"Vertex cell {id} missing 'value' attribute", "WARN_MISSING_VALUE"));
        }

        if (string.IsNullOrEmpty(style))
        {
            warnings.Add(new ValidationWarning($"Vertex cell {id} missing 'style' attribute", "WARN_MISSING_STYLE"));
        }

        var geometry = cell.Element("mxGeometry");
        if (geometry == null)
        {
            errors.Add(new ValidationError($"Vertex cell {id} missing 'mxGeometry' element", ValidationSeverity.Error));
        }
        else
        {
            ValidateGeometry(geometry, errors, warnings);
        }
    }

    private void ValidateEdgeCell(XElement cell, List<ValidationError> errors, List<ValidationWarning> warnings)
    {
        var id = cell.Attribute("id")?.Value;
        var source = cell.Attribute("source")?.Value;
        var target = cell.Attribute("target")?.Value;

        if (string.IsNullOrEmpty(source))
        {
            errors.Add(new ValidationError($"Edge cell {id} missing 'source' attribute", ValidationSeverity.Error));
        }

        if (string.IsNullOrEmpty(target))
        {
            errors.Add(new ValidationError($"Edge cell {id} missing 'target' attribute", ValidationSeverity.Error));
        }

        var geometry = cell.Element("mxGeometry");
        if (geometry != null)
        {
            ValidateGeometry(geometry, errors, warnings);
        }
    }

    private void ValidateGeometry(XElement geometry, List<ValidationError> errors, List<ValidationWarning> warnings)
    {
        var x = geometry.Attribute("x")?.Value;
        var y = geometry.Attribute("y")?.Value;
        var width = geometry.Attribute("width")?.Value;
        var height = geometry.Attribute("height")?.Value;

        if (string.IsNullOrEmpty(x) || string.IsNullOrEmpty(y))
        {
            warnings.Add(new ValidationWarning("Geometry missing position attributes", "WARN_MISSING_POSITION"));
        }

        if (string.IsNullOrEmpty(width) || string.IsNullOrEmpty(height))
        {
            warnings.Add(new ValidationWarning("Geometry missing size attributes", "WARN_MISSING_SIZE"));
        }
    }
} 
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using ThreagileConverter.Core.Logging;
using ThreagileConverter.Core.Models;

namespace ThreagileConverter.Core.Adapters;

/// <summary>
/// Implementation of the DrawIO adapter
/// </summary>
public class DrawIOAdapter : IDrawIOAdapter
{
    private readonly ILogger<DrawIOAdapter> _logger;
    private readonly IMetadataExtractor _metadataExtractor;

    public DrawIOAdapter(ILogger<DrawIOAdapter> logger, IMetadataExtractor metadataExtractor)
    {
        _logger = logger;
        _metadataExtractor = metadataExtractor;
    }

    /// <summary>
    /// Converts a DrawIO diagram to XML
    /// </summary>
    /// <param name="drawioContent">The DrawIO diagram content</param>
    /// <returns>The XML document</returns>
    public async Task<XDocument> ConvertToXmlAsync(string drawioContent)
    {
        _logger.LogInformation("Converting DrawIO diagram to XML");
        return XDocument.Parse(drawioContent);
    }

    /// <summary>
    /// Converts an XML document to a DrawIO diagram
    /// </summary>
    /// <param name="xmlDocument">The XML document</param>
    /// <returns>The DrawIO diagram content</returns>
    public async Task<string> ConvertToDrawIOAsync(XDocument xmlDocument)
    {
        _logger.LogInformation("Converting XML document to DrawIO diagram");
        return xmlDocument.ToString();
    }

    /// <summary>
    /// Extracts metadata from a DrawIO diagram
    /// </summary>
    /// <param name="drawioContent">The DrawIO diagram content</param>
    /// <returns>The metadata</returns>
    public async Task<DrawIOMetadata> ExtractMetadataAsync(string drawioContent)
    {
        _logger.LogInformation("Extracting metadata from DrawIO diagram");
        var doc = XDocument.Parse(drawioContent);
        return ExtractMetadata(doc);
    }

    /// <summary>
    /// Validates a DrawIO diagram
    /// </summary>
    /// <param name="drawioContent">The DrawIO diagram content</param>
    /// <returns>True if the diagram is valid, false otherwise</returns>
    public async Task<bool> ValidateAsync(string drawioContent)
    {
        _logger.LogInformation("Validating DrawIO diagram");
        try
        {
            var doc = XDocument.Parse(drawioContent);
            return doc.Root?.Name == "mxfile" && doc.Root.Element("diagram") != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating DrawIO diagram");
            return false;
        }
    }

    /// <summary>
    /// Converts a DrawIO XML document to a DrawIO model
    /// </summary>
    /// <param name="doc">The DrawIO XML document</param>
    /// <returns>The DrawIO model</returns>
    public DrawIOModel ConvertToModel(XDocument doc)
    {
        var model = new DrawIOModel
        {
            Metadata = ExtractMetadata(doc),
            Shapes = new List<DrawIOShape>(),
            Relations = new List<DrawIORelation>()
        };

        var elements = doc.Root?.Element("diagram")?.Element("mxGraphModel")?.Element("root")?.Elements("mxCell").ToList();
        if (elements != null)
        {
            ProcessShapes(elements);
            ProcessConnections(elements);
        }
        return model;
    }

    /// <summary>
    /// Extracts styles from a DrawIO element
    /// </summary>
    /// <param name="element">The DrawIO element</param>
    /// <returns>The extracted styles</returns>
    public DrawIOStyle ExtractStyles(XElement element)
    {
        var style = new DrawIOStyle();
        var styleStr = element.Attribute("style")?.Value;
        if (!string.IsNullOrEmpty(styleStr))
        {
            var parts = styleStr.Split(';');
            foreach (var part in parts)
            {
                var keyValue = part.Split('=');
                if (keyValue.Length == 2)
                {
                    var key = keyValue[0];
                    var value = keyValue[1];
                    switch (key)
                    {
                        case "fillColor":
                            style.FillColor = value;
                            break;
                        case "strokeColor":
                            style.StrokeColor = value;
                            break;
                        case "fontColor":
                            style.FontColor = value;
                            break;
                        case "fontStyle":
                            style.FontStyle = value;
                            break;
                        case "fontSize":
                            if (int.TryParse(value, out var size))
                                style.FontSize = size;
                            break;
                        case "shape":
                            style.Shape = value;
                            break;
                        default:
                            style.Properties[key] = value;
                            break;
                    }
                }
            }
        }
        return style;
    }

    /// <summary>
    /// Extracts metadata from a DrawIO document
    /// </summary>
    /// <param name="doc">The DrawIO XML document</param>
    /// <returns>The extracted metadata</returns>
    public DrawIOMetadata ExtractMetadata(XDocument doc)
    {
        var metadata = new DrawIOMetadata();
        var diagram = doc.Root?.Element("diagram");
        if (diagram != null)
        {
            metadata.Title = diagram.Attribute("name")?.Value ?? string.Empty;
            metadata.Description = diagram.Attribute("description")?.Value ?? string.Empty;
            metadata.Version = diagram.Attribute("version")?.Value ?? string.Empty;
            metadata.Author = diagram.Attribute("author")?.Value ?? string.Empty;
        }
        return metadata;
    }

    /// <summary>
    /// Processes shapes from a list of DrawIO elements
    /// </summary>
    /// <param name="elements">The list of DrawIO elements</param>
    public void ProcessShapes(List<XElement> elements)
    {
        var shapes = elements.Where(e => e.Attribute("vertex")?.Value == "1").ToList();
        foreach (var shape in shapes)
        {
            var id = shape.Attribute("id")?.Value;
            var value = shape.Attribute("value")?.Value;
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(value))
            {
                var drawioShape = new DrawIOShape
                {
                    Id = id,
                    Value = value,
                    Style = ExtractStyles(shape),
                    Geometry = new DrawIOGeometry
                    {
                        X = double.Parse(shape.Element("mxGeometry")?.Attribute("x")?.Value ?? "0"),
                        Y = double.Parse(shape.Element("mxGeometry")?.Attribute("y")?.Value ?? "0"),
                        Width = double.Parse(shape.Element("mxGeometry")?.Attribute("width")?.Value ?? "0"),
                        Height = double.Parse(shape.Element("mxGeometry")?.Attribute("height")?.Value ?? "0")
                    }
                };
            }
        }
    }

    /// <summary>
    /// Processes connections from a list of DrawIO elements
    /// </summary>
    /// <param name="elements">The list of DrawIO elements</param>
    public void ProcessConnections(List<XElement> elements)
    {
        var connections = elements.Where(e => e.Attribute("edge")?.Value == "1").ToList();
        foreach (var connection in connections)
        {
            var id = connection.Attribute("id")?.Value;
            var value = connection.Attribute("value")?.Value;
            var source = connection.Attribute("source")?.Value;
            var target = connection.Attribute("target")?.Value;
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(source) && !string.IsNullOrEmpty(target))
            {
                var drawioRelation = new DrawIORelation
                {
                    Id = id,
                    Value = value,
                    SourceId = source,
                    TargetId = target,
                    Style = ExtractStyles(connection),
                    Geometry = new DrawIOGeometry
                    {
                        X = double.Parse(connection.Element("mxGeometry")?.Attribute("x")?.Value ?? "0"),
                        Y = double.Parse(connection.Element("mxGeometry")?.Attribute("y")?.Value ?? "0"),
                        Width = double.Parse(connection.Element("mxGeometry")?.Attribute("width")?.Value ?? "0"),
                        Height = double.Parse(connection.Element("mxGeometry")?.Attribute("height")?.Value ?? "0")
                    }
                };
            }
        }
    }
} 
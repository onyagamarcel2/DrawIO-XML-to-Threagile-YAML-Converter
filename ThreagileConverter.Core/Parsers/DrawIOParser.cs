using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using ThreagileConverter.Core.Logging;
using ThreagileConverter.Core.Models;

namespace ThreagileConverter.Core.Parsers;

/// <summary>
/// Parser for DrawIO diagrams
/// </summary>
public class DrawIOParser : IDrawIOParser
{
    private readonly ILogger<DrawIOParser> _logger;

    public DrawIOParser(ILogger<DrawIOParser> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Parses a DrawIO diagram
    /// </summary>
    /// <param name="content">The DrawIO diagram content</param>
    /// <returns>The parsed DrawIO model</returns>
    public async Task<DrawIOModel> ParseAsync(string content)
    {
        try
        {
            _logger.LogDebug("Starting to parse DrawIO content");
            var doc = XDocument.Parse(content);
            var diagram = doc.Root?.Element("diagram");
            if (diagram == null)
            {
                _logger.LogWarning("No diagram element found in the XML");
                throw new InvalidOperationException("No diagram element found in the XML");
            }

            // Get the mxGraphModel element
            var graphModel = diagram.Element("mxGraphModel");
            if (graphModel == null)
            {
                _logger.LogWarning("No mxGraphModel element found in the diagram");
                throw new InvalidOperationException("No mxGraphModel element found in the diagram");
            }

            // Get the root element
            var root = graphModel.Element("root");
            if (root == null)
            {
                _logger.LogWarning("No root element found in the mxGraphModel");
                throw new InvalidOperationException("No root element found in the mxGraphModel");
            }

            _logger.LogDebug("Found root element in the mxGraphModel");

            var model = new DrawIOModel
            {
                Metadata = ExtractMetadata(diagram),
                Shapes = ExtractShapes(root),
                Relations = ExtractRelations(root)
            };

            _logger.LogDebug("Extracted {ShapeCount} shapes and {RelationCount} relations", 
                model.Shapes.Count, model.Relations.Count);

            return model;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing DrawIO diagram");
            throw;
        }
    }

    /// <summary>
    /// Extracts metadata from a DrawIO document
    /// </summary>
    /// <param name="diagram">The DrawIO XML diagram element</param>
    /// <returns>The extracted metadata</returns>
    private DrawIOMetadata ExtractMetadata(XElement diagram)
    {
        _logger.LogDebug("Extracting metadata from diagram");
        var metadata = new DrawIOMetadata
        {
            Title = diagram.Attribute("name")?.Value ?? string.Empty,
            Description = diagram.Attribute("description")?.Value ?? string.Empty,
            Version = diagram.Parent?.Attribute("version")?.Value ?? string.Empty,
            Author = diagram.Parent?.Attribute("author")?.Value ?? string.Empty,
            Created = diagram.Parent?.Attribute("created")?.Value ?? string.Empty,
            Modified = diagram.Parent?.Attribute("modified")?.Value ?? string.Empty,
            Tags = diagram.Attribute("tags")?.Value ?? string.Empty,
            Properties = new Dictionary<string, string>()
        };

        _logger.LogDebug("Extracted metadata: Title={Title}, Version={Version}", 
            metadata.Title, metadata.Version);

        var properties = diagram.Elements("property");
        foreach (var prop in properties)
        {
            var key = prop.Attribute("key")?.Value;
            var value = prop.Attribute("value")?.Value;
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
            {
                metadata.Properties[key] = value;
            }
        }

        return metadata;
    }

    /// <summary>
    /// Extracts shapes from a DrawIO element
    /// </summary>
    /// <param name="element">The DrawIO element</param>
    /// <returns>The list of extracted shapes</returns>
    public List<DrawIOShape> ExtractShapes(XElement element)
    {
        _logger.LogDebug("Extracting shapes from element");
        var shapes = new List<DrawIOShape>();
        
        // Get all mxCell elements that represent shapes (vertex="1")
        var cellElements = element.Elements("mxCell")
            .Where(e => e.Attribute("vertex")?.Value == "1");
        
        _logger.LogDebug("Found {Count} mxCell elements with vertex=1", cellElements.Count());

        foreach (var cellElement in cellElements)
        {
            var shape = new DrawIOShape
            {
                Id = cellElement.Attribute("id")?.Value ?? string.Empty,
                Value = cellElement.Attribute("value")?.Value ?? string.Empty,
                Style = ExtractStyle(cellElement),
                Geometry = ExtractGeometry(cellElement),
                Children = new List<DrawIOShape>()
            };

            _logger.LogDebug("Extracted shape: Id={Id}, Value={Value}", shape.Id, shape.Value);

            // Extract children if any
            var children = cellElement.Elements("mxCell")
                .Where(e => e.Attribute("vertex")?.Value == "1");
            
            foreach (var child in children)
            {
                var childShape = new DrawIOShape
                {
                    Id = child.Attribute("id")?.Value ?? string.Empty,
                    Value = child.Attribute("value")?.Value ?? string.Empty,
                    Style = ExtractStyle(child),
                    Geometry = ExtractGeometry(child),
                    Children = new List<DrawIOShape>()
                };
                shape.Children.Add(childShape);
                _logger.LogDebug("Added child shape: Id={Id}, Value={Value}", childShape.Id, childShape.Value);
            }

            shapes.Add(shape);
        }
        
        _logger.LogDebug("Extracted {Count} shapes in total", shapes.Count);
        return shapes;
    }

    /// <summary>
    /// Extracts relations from a DrawIO element
    /// </summary>
    /// <param name="element">The DrawIO element</param>
    /// <returns>The list of extracted relations</returns>
    public List<DrawIORelation> ExtractRelations(XElement element)
    {
        _logger.LogDebug("Extracting relations from element");
        var relations = new List<DrawIORelation>();
        
        // Get all mxCell elements that represent edges (edge="1")
        var edgeElements = element.Elements("mxCell")
            .Where(e => e.Attribute("edge")?.Value == "1");
        
        _logger.LogDebug("Found {Count} mxCell elements with edge=1", edgeElements.Count());

        foreach (var edgeElement in edgeElements)
        {
            var relation = new DrawIORelation
            {
                Id = edgeElement.Attribute("id")?.Value ?? string.Empty,
                Value = edgeElement.Attribute("value")?.Value ?? string.Empty,
                SourceId = edgeElement.Attribute("source")?.Value ?? string.Empty,
                TargetId = edgeElement.Attribute("target")?.Value ?? string.Empty,
                Style = ExtractStyle(edgeElement),
                Geometry = ExtractGeometry(edgeElement)
            };

            _logger.LogDebug("Extracted relation: Id={Id}, Value={Value}, Source={Source}, Target={Target}", 
                relation.Id, relation.Value, relation.SourceId, relation.TargetId);

            relations.Add(relation);
        }
        
        _logger.LogDebug("Extracted {Count} relations in total", relations.Count);
        return relations;
    }

    /// <summary>
    /// Extracts styles from a DrawIO element
    /// </summary>
    /// <param name="element">The DrawIO element</param>
    /// <returns>The extracted styles</returns>
    private DrawIOStyle ExtractStyle(XElement element)
    {
        var style = new DrawIOStyle();
        var styleAttr = element.Attribute("style")?.Value;
        if (!string.IsNullOrEmpty(styleAttr))
        {
            var properties = styleAttr.Split(';');
            foreach (var prop in properties)
            {
                var parts = prop.Split('=');
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();
                    style.Properties[key] = value;

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
                            if (int.TryParse(value, out int size))
                            {
                                style.FontSize = size;
                            }
                            break;
                        case "shape":
                            style.Shape = value;
                            break;
                    }
                }
            }
        }
        return style;
    }

    /// <summary>
    /// Extracts geometry from a DrawIO element
    /// </summary>
    /// <param name="element">The DrawIO element</param>
    /// <returns>The extracted geometry</returns>
    private DrawIOGeometry ExtractGeometry(XElement element)
    {
        var geometry = new DrawIOGeometry();
        var geoElement = element.Element("mxGeometry");
        if (geoElement != null)
        {
            geometry.X = double.TryParse(geoElement.Attribute("x")?.Value, out double x) ? x : 0;
            geometry.Y = double.TryParse(geoElement.Attribute("y")?.Value, out double y) ? y : 0;
            geometry.Width = double.TryParse(geoElement.Attribute("width")?.Value, out double w) ? w : 0;
            geometry.Height = double.TryParse(geoElement.Attribute("height")?.Value, out double h) ? h : 0;
        }
        return geometry;
    }
} 
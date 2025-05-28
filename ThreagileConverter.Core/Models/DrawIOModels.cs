using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace ThreagileConverter.Core.Models;

/// <summary>
/// Represents metadata for a DrawIO diagram
/// </summary>
public class DrawIOMetadata
{
    /// <summary>
    /// Gets or sets the title of the diagram
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the diagram
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the diagram
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author of the diagram
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the created date
    /// </summary>
    public string Created { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the modified date
    /// </summary>
    public string Modified { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tags
    /// </summary>
    public string Tags { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional properties
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();
}

/// <summary>
/// Represents the style of a DrawIO element
/// </summary>
public class DrawIOStyle
{
    /// <summary>
    /// Gets or sets the fill color
    /// </summary>
    public string FillColor { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stroke color
    /// </summary>
    public string StrokeColor { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the font color
    /// </summary>
    public string FontColor { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the font style
    /// </summary>
    public string FontStyle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the font size
    /// </summary>
    public int FontSize { get; set; }

    /// <summary>
    /// Gets or sets the shape
    /// </summary>
    public string Shape { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional properties
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();

    public static DrawIOStyle Parse(string styleString)
    {
        var style = new DrawIOStyle();
        
        if (string.IsNullOrEmpty(styleString))
            return style;
            
        var properties = styleString.Split(';');
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
        
        return style;
    }
}

/// <summary>
/// Represents a shape in DrawIO
/// </summary>
public class DrawIOShape
{
    /// <summary>
    /// Gets or sets the ID of the shape
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the shape
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the style of the shape
    /// </summary>
    public DrawIOStyle Style { get; set; } = new();

    /// <summary>
    /// Gets or sets the geometry of the shape
    /// </summary>
    public DrawIOGeometry Geometry { get; set; } = new();

    public List<DrawIOShape> Children { get; set; } = new();

    public static DrawIOShape Parse(string xml)
    {
        if (string.IsNullOrEmpty(xml))
            return new DrawIOShape();
            
        try
        {
            var element = XElement.Parse(xml);
            var shape = new DrawIOShape
            {
                Id = element.Attribute("id")?.Value ?? string.Empty,
                Value = element.Attribute("value")?.Value ?? string.Empty,
                Style = DrawIOStyle.Parse(element.Attribute("style")?.Value ?? string.Empty),
                Geometry = new DrawIOGeometry()
            };
            
            // Parse geometry if available
            var geometryElement = element.Element("mxGeometry");
            if (geometryElement != null)
            {
                shape.Geometry.X = double.TryParse(geometryElement.Attribute("x")?.Value, out var x) ? x : 0;
                shape.Geometry.Y = double.TryParse(geometryElement.Attribute("y")?.Value, out var y) ? y : 0;
                shape.Geometry.Width = double.TryParse(geometryElement.Attribute("width")?.Value, out var width) ? width : 0;
                shape.Geometry.Height = double.TryParse(geometryElement.Attribute("height")?.Value, out var height) ? height : 0;
            }
            
            // Parse children if available
            foreach (var childElement in element.Elements())
            {
                if (childElement.Name.LocalName != "mxGeometry")
                {
                    var childShape = new DrawIOShape
                    {
                        Id = childElement.Attribute("id")?.Value ?? string.Empty,
                        Value = childElement.Attribute("value")?.Value ?? string.Empty,
                        Style = DrawIOStyle.Parse(childElement.Attribute("style")?.Value ?? string.Empty)
                    };
                    shape.Children.Add(childShape);
                }
            }
            
            return shape;
        }
        catch (Exception)
        {
            // Log error or handle exception
            return new DrawIOShape();
        }
    }
}

/// <summary>
/// Represents a relation between shapes in DrawIO
/// </summary>
public class DrawIORelation
{
    /// <summary>
    /// Gets or sets the ID of the relation
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the relation
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source shape ID
    /// </summary>
    public string SourceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target shape ID
    /// </summary>
    public string TargetId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the style of the relation
    /// </summary>
    public DrawIOStyle Style { get; set; } = new();

    /// <summary>
    /// Gets or sets the geometry of the relation
    /// </summary>
    public DrawIOGeometry Geometry { get; set; } = new();

    public static DrawIORelation Parse(string xml)
    {
        if (string.IsNullOrEmpty(xml))
            return new DrawIORelation();
            
        try
        {
            var element = XElement.Parse(xml);
            var relation = new DrawIORelation
            {
                Id = element.Attribute("id")?.Value ?? string.Empty,
                Value = element.Attribute("value")?.Value ?? string.Empty,
                SourceId = element.Attribute("source")?.Value ?? string.Empty,
                TargetId = element.Attribute("target")?.Value ?? string.Empty,
                Style = DrawIOStyle.Parse(element.Attribute("style")?.Value ?? string.Empty),
                Geometry = new DrawIOGeometry()
            };
            
            // Parse geometry if available
            var geometryElement = element.Element("mxGeometry");
            if (geometryElement != null)
            {
                relation.Geometry.X = double.TryParse(geometryElement.Attribute("x")?.Value, out var x) ? x : 0;
                relation.Geometry.Y = double.TryParse(geometryElement.Attribute("y")?.Value, out var y) ? y : 0;
                relation.Geometry.Width = double.TryParse(geometryElement.Attribute("width")?.Value, out var width) ? width : 0;
                relation.Geometry.Height = double.TryParse(geometryElement.Attribute("height")?.Value, out var height) ? height : 0;
            }
            
            return relation;
        }
        catch (Exception)
        {
            // Log error or handle exception
            return new DrawIORelation();
        }
    }
}

/// <summary>
/// Represents the geometry of a DrawIO shape
/// </summary>
public class DrawIOGeometry
{
    /// <summary>
    /// Gets or sets the X coordinate
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Gets or sets the width
    /// </summary>
    public double Width { get; set; }

    /// <summary>
    /// Gets or sets the height
    /// </summary>
    public double Height { get; set; }
} 
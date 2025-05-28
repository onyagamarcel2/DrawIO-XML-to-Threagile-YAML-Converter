using System.Collections.Generic;

namespace ThreagileConverter.Core.Models;

/// <summary>
/// Represents a DrawIO model
/// </summary>
public class DrawIOModel
{
    /// <summary>
    /// Gets or sets the metadata of the model
    /// </summary>
    public DrawIOMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the shapes in the model
    /// </summary>
    public List<DrawIOShape> Shapes { get; set; } = new();

    /// <summary>
    /// Gets or sets the relations in the model
    /// </summary>
    public List<DrawIORelation> Relations { get; set; } = new();

    /// <summary>
    /// Gets or sets the properties of the model
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();
} 
using System.Collections.Generic;

namespace ThreagileConverter.Core.Models;

/// <summary>
/// Represents a Threagile model
/// </summary>
public class ThreagileModel
{
    /// <summary>
    /// Gets or sets the title of the model
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the model
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the model
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author of the model
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the technical assets
    /// </summary>
    public List<TechnicalAsset> TechnicalAssets { get; set; } = new();

    /// <summary>
    /// Gets or sets the trust boundaries
    /// </summary>
    public List<TrustBoundary> TrustBoundaries { get; set; } = new();

    /// <summary>
    /// Gets or sets the shared runtimes
    /// </summary>
    public List<SharedRuntime> SharedRuntimes { get; set; } = new();

    /// <summary>
    /// Gets or sets the data assets
    /// </summary>
    public List<DataAsset> DataAssets { get; set; } = new();

    /// <summary>
    /// Gets or sets the communication links
    /// </summary>
    public List<CommunicationLink> CommunicationLinks { get; set; } = new();

    /// <summary>
    /// Gets or sets the styles
    /// </summary>
    public Dictionary<string, ThreagileStyle> Styles { get; set; } = new();

    /// <summary>
    /// Gets or sets the properties
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();
}

/// <summary>
/// Represents a technical asset
/// </summary>
public class TechnicalAsset
{
    /// <summary>
    /// Gets or sets the ID of the asset
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the asset
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title of the asset
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the asset
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the asset
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the usage of the asset
    /// </summary>
    public string Usage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the style of the asset
    /// </summary>
    public ThreagileStyle Style { get; set; } = new();

    /// <summary>
    /// Gets or sets whether the asset is used for data protection
    /// </summary>
    public bool UsedForDataProtection { get; set; }

    /// <summary>
    /// Gets or sets whether the asset is used for data retention
    /// </summary>
    public bool UsedForDataRetention { get; set; }

    /// <summary>
    /// Gets or sets whether the asset is used for data destruction
    /// </summary>
    public bool UsedForDataDestruction { get; set; }

    /// <summary>
    /// Gets or sets whether the asset is used for data archiving
    /// </summary>
    public bool UsedForDataArchiving { get; set; }

    /// <summary>
    /// Gets or sets the properties of the asset
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();
}

/// <summary>
/// Represents a trust boundary
/// </summary>
public class TrustBoundary
{
    /// <summary>
    /// Gets or sets the ID of the boundary
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title of the boundary
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the boundary
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the boundary
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the boundary
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the style of the boundary
    /// </summary>
    public ThreagileStyle Style { get; set; } = new();

    /// <summary>
    /// Gets or sets the technical assets within the boundary
    /// </summary>
    public List<string> TechnicalAssets { get; set; } = new();

    /// <summary>
    /// Gets or sets the properties of the boundary
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();
}

/// <summary>
/// Represents a shared runtime
/// </summary>
public class SharedRuntime
{
    /// <summary>
    /// Gets or sets the ID of the runtime
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the runtime
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the runtime
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the technical assets within the runtime
    /// </summary>
    public List<string> TechnicalAssets { get; set; } = new();

    /// <summary>
    /// Gets or sets the style of the runtime
    /// </summary>
    public ThreagileStyle Style { get; set; } = new();

    /// <summary>
    /// Gets or sets the properties of the runtime
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();
}

/// <summary>
/// Represents a data asset
/// </summary>
public class DataAsset
{
    /// <summary>
    /// Gets or sets the ID of the asset
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the asset
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the asset
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the asset
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the usage of the asset
    /// </summary>
    public string Usage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the origin of the asset
    /// </summary>
    public string Origin { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owner of the asset
    /// </summary>
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity of the asset
    /// </summary>
    public string Quantity { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confidentiality of the asset
    /// </summary>
    public string Confidentiality { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the integrity of the asset
    /// </summary>
    public string Integrity { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the availability of the asset
    /// </summary>
    public string Availability { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the style of the asset
    /// </summary>
    public ThreagileStyle Style { get; set; } = new();

    /// <summary>
    /// Gets or sets the properties of the asset
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();
}

/// <summary>
/// Represents a communication link
/// </summary>
public class CommunicationLink
{
    /// <summary>
    /// Gets or sets the ID of the link
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the link
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title of the link
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the link
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the style of the link
    /// </summary>
    public ThreagileStyle Style { get; set; } = new();

    /// <summary>
    /// Gets or sets the source ID of the link
    /// </summary>
    public string SourceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target ID of the link
    /// </summary>
    public string TargetId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source of the link
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target of the link
    /// </summary>
    public string Target { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type of the link
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the protocol of the link
    /// </summary>
    public string Protocol { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authentication of the link
    /// </summary>
    public string Authentication { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authorization of the link
    /// </summary>
    public string Authorization { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the encryption of the link
    /// </summary>
    public string Encryption { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the properties of the link
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();
}

/// <summary>
/// Represents a Threagile style
/// </summary>
public class ThreagileStyle
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
    public int FontSize { get; set; } = 0;

    /// <summary>
    /// Gets or sets the shape
    /// </summary>
    public string Shape { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the properties of the style
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();
} 
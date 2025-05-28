using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using ThreagileConverter.Core.Logging;
using ThreagileConverter.Core.Models;

namespace ThreagileConverter.Core.Adapters;

/// <summary>
/// Implementation of the metadata extractor for DrawIO diagrams
/// </summary>
public class MetadataExtractor : IMetadataExtractor
{
    private readonly ILogger<MetadataExtractor> _logger;

    public MetadataExtractor(ILogger<MetadataExtractor> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Extracts metadata from a DrawIO diagram
    /// </summary>
    /// <param name="content">The DrawIO diagram content</param>
    /// <returns>The extracted metadata</returns>
    public async Task<DrawIOMetadata> ExtractAsync(string content)
    {
        _logger.LogInformation("Extracting metadata from DrawIO diagram");
        var doc = XDocument.Parse(content);
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
} 
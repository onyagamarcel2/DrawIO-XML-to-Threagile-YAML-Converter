using System.Threading.Tasks;
using ThreagileConverter.Core.Models;

namespace ThreagileConverter.Core.Adapters;

/// <summary>
/// Interface for extracting metadata from DrawIO diagrams
/// </summary>
public interface IMetadataExtractor
{
    /// <summary>
    /// Extracts metadata from a DrawIO diagram
    /// </summary>
    /// <param name="content">The DrawIO diagram content</param>
    /// <returns>The extracted metadata</returns>
    Task<DrawIOMetadata> ExtractAsync(string content);
} 
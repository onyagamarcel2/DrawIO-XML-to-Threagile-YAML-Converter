using System.Xml.Linq;

namespace ThreagileConverter.Core.Generation;

/// <summary>
/// Interface for file generators
/// </summary>
public interface IGenerator
{
    /// <summary>
    /// Generates a file from an XML document
    /// </summary>
    /// <param name="document">Source XML document</param>
    /// <param name="outputPath">Output file path</param>
    /// <returns>Task</returns>
    Task GenerateAsync(XDocument document, string outputPath);

    /// <summary>
    /// Generates a file from an XML document using streaming
    /// </summary>
    /// <param name="document">Source XML document</param>
    /// <param name="outputPath">Output file path</param>
    /// <param name="chunkSize">Chunk size for streaming</param>
    /// <returns>Task</returns>
    Task GenerateStreamingAsync(XDocument document, string outputPath, int chunkSize = 8192);
} 
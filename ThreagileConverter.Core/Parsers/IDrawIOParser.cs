using System.Threading.Tasks;
using ThreagileConverter.Core.Models;

namespace ThreagileConverter.Core.Parsers;

/// <summary>
/// Interface for parsing DrawIO diagrams
/// </summary>
public interface IDrawIOParser
{
    /// <summary>
    /// Parses a DrawIO diagram
    /// </summary>
    /// <param name="content">The DrawIO diagram content</param>
    /// <returns>The parsed DrawIO model</returns>
    Task<DrawIOModel> ParseAsync(string content);
} 
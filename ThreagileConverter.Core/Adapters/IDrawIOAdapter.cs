using System.Threading.Tasks;
using System.Xml.Linq;
using ThreagileConverter.Core.Models;
using System.Collections.Generic;

namespace ThreagileConverter.Core.Adapters;

/// <summary>
/// Interface for DrawIO adapter
/// </summary>
public interface IDrawIOAdapter
{
    /// <summary>
    /// Converts a DrawIO diagram to XML
    /// </summary>
    /// <param name="drawioContent">The DrawIO diagram content</param>
    /// <returns>The XML document</returns>
    Task<XDocument> ConvertToXmlAsync(string drawioContent);

    /// <summary>
    /// Converts an XML document to a DrawIO diagram
    /// </summary>
    /// <param name="xmlDocument">The XML document</param>
    /// <returns>The DrawIO diagram content</returns>
    Task<string> ConvertToDrawIOAsync(XDocument xmlDocument);

    /// <summary>
    /// Extracts metadata from a DrawIO diagram
    /// </summary>
    /// <param name="drawioContent">The DrawIO diagram content</param>
    /// <returns>The metadata</returns>
    Task<DrawIOMetadata> ExtractMetadataAsync(string drawioContent);

    /// <summary>
    /// Validates a DrawIO diagram
    /// </summary>
    /// <param name="drawioContent">The DrawIO diagram content</param>
    /// <returns>True if the diagram is valid, false otherwise</returns>
    Task<bool> ValidateAsync(string drawioContent);

    /// <summary>
    /// Converts a DrawIO XML document to a DrawIO model
    /// </summary>
    /// <param name="doc">The DrawIO XML document</param>
    /// <returns>The DrawIO model</returns>
    DrawIOModel ConvertToModel(XDocument doc);

    /// <summary>
    /// Extracts styles from a DrawIO element
    /// </summary>
    /// <param name="element">The DrawIO element</param>
    /// <returns>The extracted styles</returns>
    DrawIOStyle ExtractStyles(XElement element);

    /// <summary>
    /// Extracts metadata from a DrawIO document
    /// </summary>
    /// <param name="doc">The DrawIO XML document</param>
    /// <returns>The extracted metadata</returns>
    DrawIOMetadata ExtractMetadata(XDocument doc);

    /// <summary>
    /// Processes shapes from a list of DrawIO elements
    /// </summary>
    /// <param name="elements">The list of DrawIO elements</param>
    void ProcessShapes(List<XElement> elements);

    /// <summary>
    /// Processes connections from a list of DrawIO elements
    /// </summary>
    /// <param name="elements">The list of DrawIO elements</param>
    void ProcessConnections(List<XElement> elements);
} 
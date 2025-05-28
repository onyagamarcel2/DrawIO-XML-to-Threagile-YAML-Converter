using System.Xml.Linq;
using ThreagileConverter.Core.Models;

namespace ThreagileConverter.Core.Generation;

/// <summary>
/// Interface for YAML generator
/// </summary>
public interface IYamlGenerator : IGenerator
{
    /// <summary>
    /// Generates a YAML string from a Threagile model
    /// </summary>
    /// <param name="model">The Threagile model to convert</param>
    /// <returns>The generated YAML string</returns>
    string GenerateYaml(ThreagileModel model);

    /// <summary>
    /// Generates a YAML string from a Threagile model and writes it to a file
    /// </summary>
    /// <param name="model">The Threagile model to convert</param>
    /// <param name="filePath">The output file path</param>
    void GenerateYamlToFile(ThreagileModel model, string filePath);

    /// <summary>
    /// Asynchronously generates a YAML string from a Threagile model and writes it to a file
    /// </summary>
    /// <param name="model">The Threagile model to convert</param>
    /// <param name="filePath">The output file path</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task GenerateYamlToFileAsync(ThreagileModel model, string filePath);

    /// <summary>
    /// Validates the generated YAML schema
    /// </summary>
    /// <param name="yaml">The YAML string to validate</param>
    /// <returns>True if the schema is valid, false otherwise</returns>
    bool ValidateSchema(string yaml);

    /// <summary>
    /// Optimizes the YAML output
    /// </summary>
    /// <param name="yaml">The YAML string to optimize</param>
    /// <returns>The optimized YAML string</returns>
    string OptimizeOutput(string yaml);

    /// <summary>
    /// Processes references in the YAML model
    /// </summary>
    /// <param name="yaml">The YAML string to process</param>
    /// <returns>The YAML string with processed references</returns>
    string ProcessReferences(string yaml);

    /// <summary>
    /// Adds comments to the YAML
    /// </summary>
    /// <param name="yaml">The YAML string to comment</param>
    /// <param name="comments">The comments to add</param>
    /// <returns>The YAML string with comments</returns>
    string AddComments(string yaml, Dictionary<string, string> comments);

    /// <summary>
    /// Extracts comments from a YAML string
    /// </summary>
    /// <param name="yaml">The YAML string to analyze</param>
    /// <returns>A dictionary mapping lines to their comments</returns>
    Dictionary<string, string> ExtractComments(string yaml);

    /// <summary>
    /// Checks for circular references in the model
    /// </summary>
    /// <param name="model">The Threagile model to analyze</param>
    /// <returns>True if circular references are detected, false otherwise</returns>
    bool HasCircularReferences(ThreagileModel model);

    /// <summary>
    /// Resolves references in the model
    /// </summary>
    /// <param name="model">The Threagile model to process</param>
    /// <returns>The model with resolved references</returns>
    ThreagileModel ResolveReferences(ThreagileModel model);
} 
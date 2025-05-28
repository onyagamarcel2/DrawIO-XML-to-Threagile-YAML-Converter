using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using ThreagileConverter.Core.Models;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization.ObjectFactories;

namespace ThreagileConverter.Core.Generation;

/// <summary>
/// Implementation of the YAML generator
/// </summary>
public class YamlGenerator : IYamlGenerator
{
    private readonly ILogger<YamlGenerator> _logger;
    private readonly ISerializer _serializer;
    private readonly IDeserializer _deserializer;

    public YamlGenerator(ILogger<YamlGenerator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeConverter(new YamlStyleConverter())
            .WithTypeConverter(new YamlPropertiesConverter())
            .Build();
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    public async Task GenerateAsync(XDocument document, string outputPath)
    {
        _logger.LogInformation("Generating YAML file: {OutputPath}", outputPath);

        try
        {
            if (document?.Root == null)
            {
                throw new ArgumentException("Invalid XML document", nameof(document));
            }

            var yamlContent = ConvertToYaml(document.Root);
            await File.WriteAllTextAsync(outputPath, yamlContent);
            _logger.LogInformation("YAML file generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating YAML file");
            throw;
        }
    }

    private string ConvertToYaml(XElement element)
    {
        if (element == null)
            return "{}";

        var result = $"{element.Name.LocalName}: ";
        if (!element.HasElements && !element.HasAttributes)
        {
            result += "{}";
        }
        else
        {
            result += "{\n";
            foreach (var attr in element.Attributes())
            {
                result += $"  {attr.Name.LocalName}: {attr.Value}\n";
            }
            foreach (var child in element.Elements())
            {
                result += $"  {ConvertToYaml(child)}\n";
            }
            result += "}";
        }
        return result;
    }

    public async Task GenerateStreamingAsync(XDocument document, string outputPath, int chunkSize = 8192)
    {
        _logger.LogInformation("Streaming generation of YAML file: {OutputPath}", outputPath);
        try
        {
            if (document?.Root == null)
            {
                throw new ArgumentException("Invalid XML document", nameof(document));
            }

            using var writer = new StreamWriter(outputPath);
            var yamlContent = ConvertToYaml(document.Root);
            var buffer = new char[chunkSize];
            var reader = new StringReader(yamlContent);
            int read;
            while ((read = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await writer.WriteAsync(buffer, 0, read);
            }
            _logger.LogInformation("YAML file generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating YAML file");
            throw;
        }
    }

    public string GenerateYaml(ThreagileModel model)
    {
        try
        {
            _logger.LogInformation("Generating YAML from Threagile model");
            var yaml = _serializer.Serialize(model);
            return OptimizeOutput(yaml);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating YAML");
            throw;
        }
    }

    public void GenerateYamlToFile(ThreagileModel model, string filePath)
    {
        try
        {
            _logger.LogInformation("Generating YAML to file {FilePath}", filePath);
            var yaml = GenerateYaml(model);
            File.WriteAllText(filePath, yaml, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing YAML to file {FilePath}", filePath);
            throw;
        }
    }

    public async Task GenerateYamlToFileAsync(ThreagileModel model, string filePath)
    {
        try
        {
            _logger.LogInformation("Asynchronously generating YAML to file {FilePath}", filePath);
            var yaml = GenerateYaml(model);
            await File.WriteAllTextAsync(filePath, yaml, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing YAML to file {FilePath}", filePath);
            throw;
        }
    }

    public bool ValidateSchema(string yaml)
    {
        try
        {
            _logger.LogInformation("Validating YAML schema");
            var model = _deserializer.Deserialize<ThreagileModel>(yaml);
            return ValidateModel(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating YAML schema");
            return false;
        }
    }

    public string OptimizeOutput(string yaml)
    {
        try
        {
            _logger.LogInformation("Optimizing YAML output");
            var model = _deserializer.Deserialize<ThreagileModel>(yaml);
            return _serializer.Serialize(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing YAML output");
            return yaml;
        }
    }

    public string ProcessReferences(string yaml)
    {
        _logger.LogInformation("Processing references in YAML");
        try
        {
            var model = _deserializer.Deserialize<ThreagileModel>(yaml);
            if (model == null)
            {
                _logger.LogWarning("Unable to deserialize YAML model");
                return yaml;
            }

            // Check for circular references
            if (HasCircularReferences(model))
            {
                _logger.LogWarning("Circular references detected");
            }

            // Resolve references
            model = ResolveReferences(model);

            // Regenerate YAML
            return _serializer.Serialize(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing references");
            return yaml;
        }
    }

    public bool HasCircularReferences(ThreagileModel model)
    {
        if (model == null)
            return false;

        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();

        // Check for circular references in technical assets
        foreach (var asset in model.TechnicalAssets ?? new List<TechnicalAsset>())
        {
            if (HasCircularReferencesInAsset(asset, model, visited, recursionStack))
                return true;
        }

        // Check for circular references in trust boundaries
        foreach (var boundary in model.TrustBoundaries ?? new List<TrustBoundary>())
        {
            if (HasCircularReferencesInBoundary(boundary, model, visited, recursionStack))
                return true;
        }

        return false;
    }

    private bool HasCircularReferencesInAsset(TechnicalAsset asset, ThreagileModel model, HashSet<string> visited, HashSet<string> recursionStack)
    {
        if (asset == null || string.IsNullOrEmpty(asset.Id))
            return false;

        if (recursionStack.Contains(asset.Id))
            return true;

        if (visited.Contains(asset.Id))
            return false;

        visited.Add(asset.Id);
        recursionStack.Add(asset.Id);

        // Check for references in properties
        foreach (var prop in asset.Properties ?? new Dictionary<string, string>())
        {
            if (prop.Value is string strValue && strValue.StartsWith("ref:"))
            {
                var refId = strValue.Substring(4);
                var refAsset = FindAssetById(model, refId);
                if (refAsset != null && HasCircularReferencesInAsset(refAsset, model, visited, recursionStack))
                {
                    recursionStack.Remove(asset.Id);
                    return true;
                }
            }
        }

        recursionStack.Remove(asset.Id);
        return false;
    }

    private bool HasCircularReferencesInBoundary(TrustBoundary boundary, ThreagileModel model, HashSet<string> visited, HashSet<string> recursionStack)
    {
        if (boundary == null || string.IsNullOrEmpty(boundary.Id))
            return false;

        if (recursionStack.Contains(boundary.Id))
            return true;

        if (visited.Contains(boundary.Id))
            return false;

        visited.Add(boundary.Id);
        recursionStack.Add(boundary.Id);

        // Check for references in technical assets of the boundary
        foreach (var assetId in boundary.TechnicalAssets ?? new List<string>())
        {
            var asset = FindAssetById(model, assetId);
            if (asset != null && HasCircularReferencesInAsset(asset, model, visited, recursionStack))
            {
                recursionStack.Remove(boundary.Id);
                return true;
            }
        }

        recursionStack.Remove(boundary.Id);
        return false;
    }

    public ThreagileModel ResolveReferences(ThreagileModel model)
    {
        if (model == null)
            return null;

        var resolvedModel = new ThreagileModel
        {
            Title = model.Title,
            Description = model.Description,
            TechnicalAssets = new List<TechnicalAsset>(),
            TrustBoundaries = new List<TrustBoundary>(),
            CommunicationLinks = new List<CommunicationLink>()
        };

        // Resolve references in technical assets
        foreach (var asset in model.TechnicalAssets ?? new List<TechnicalAsset>())
        {
            var resolvedAsset = ResolveAssetReferences(asset, model);
            resolvedModel.TechnicalAssets.Add(resolvedAsset);
        }

        // Resolve references in trust boundaries
        foreach (var boundary in model.TrustBoundaries ?? new List<TrustBoundary>())
        {
            var resolvedBoundary = ResolveBoundaryReferences(boundary, model);
            resolvedModel.TrustBoundaries.Add(resolvedBoundary);
        }

        // Resolve references in communication links
        foreach (var link in model.CommunicationLinks ?? new List<CommunicationLink>())
        {
            var resolvedLink = ResolveLinkReferences(link, model);
            resolvedModel.CommunicationLinks.Add(resolvedLink);
        }

        return resolvedModel;
    }

    private TechnicalAsset ResolveAssetReferences(TechnicalAsset asset, ThreagileModel model)
    {
        if (asset == null)
            return null;

        var resolvedAsset = new TechnicalAsset
        {
            Id = asset.Id,
            Title = asset.Title,
            Description = asset.Description,
            Type = asset.Type,
            Usage = asset.Usage,
            Style = asset.Style,
            Properties = new Dictionary<string, string>()
        };

        // Resolve references in properties
        foreach (var prop in asset.Properties ?? new Dictionary<string, string>())
        {
            if (prop.Value is string strValue && strValue.StartsWith("ref:"))
            {
                var refId = strValue.Substring(4);
                var refAsset = FindAssetById(model, refId);
                if (refAsset != null)
                {
                    resolvedAsset.Properties[prop.Key] = refAsset.Title;
                }
                else
                {
                    resolvedAsset.Properties[prop.Key] = prop.Value;
                }
            }
            else
            {
                resolvedAsset.Properties[prop.Key] = prop.Value;
            }
        }

        return resolvedAsset;
    }

    private TrustBoundary ResolveBoundaryReferences(TrustBoundary boundary, ThreagileModel model)
    {
        if (boundary == null)
            return null;

        var resolvedBoundary = new TrustBoundary
        {
            Id = boundary.Id,
            Title = boundary.Title,
            Description = boundary.Description,
            Type = boundary.Type,
            TechnicalAssets = new List<string>()
        };

        // Resolve references to technical assets
        foreach (var assetId in boundary.TechnicalAssets ?? new List<string>())
        {
            var asset = FindAssetById(model, assetId);
            if (asset != null)
            {
                resolvedBoundary.TechnicalAssets.Add(asset.Id);
            }
        }

        return resolvedBoundary;
    }

    private CommunicationLink ResolveLinkReferences(CommunicationLink link, ThreagileModel model)
    {
        if (link == null)
            return null;

        var resolvedLink = new CommunicationLink
        {
            Id = link.Id,
            Title = link.Title,
            Description = link.Description,
            Source = link.Source,
            Target = link.Target,
            Protocol = link.Protocol,
            Authentication = link.Authentication,
            Authorization = link.Authorization,
            Encryption = link.Encryption
        };

        // Resolve source and target references
        var sourceAsset = FindAssetById(model, link.Source);
        var targetAsset = FindAssetById(model, link.Target);

        if (sourceAsset != null)
            resolvedLink.Source = sourceAsset.Id;
        if (targetAsset != null)
            resolvedLink.Target = targetAsset.Id;

        return resolvedLink;
    }

    private TechnicalAsset? FindAssetById(ThreagileModel model, string id)
    {
        if (model?.TechnicalAssets == null || string.IsNullOrEmpty(id))
            return null;

        return model.TechnicalAssets.FirstOrDefault(a => a.Id == id);
    }

    public string AddComments(string yaml, Dictionary<string, string> comments)
    {
        try
        {
            _logger.LogInformation("Adding comments to YAML");
            var lines = yaml.Split('\n');
            var result = new List<string>();
            var indentStack = new Stack<int>();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var trimmedLine = line.TrimStart();
                var indent = line.Length - trimmedLine.Length;

                // Handle indentation
                while (indentStack.Count > 0 && indent <= indentStack.Peek())
                {
                    indentStack.Pop();
                }
                indentStack.Push(indent);

                // Add the line
                result.Add(line);

                // Add comment if present
                if (comments.TryGetValue(trimmedLine, out var comment))
                {
                    var commentIndent = indent + 2;
                    result.Add(new string(' ', commentIndent) + "# " + comment);
                }
            }

            return string.Join("\n", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comments");
            return yaml;
        }
    }

    public Dictionary<string, string> ExtractComments(string yaml)
    {
        try
        {
            _logger.LogInformation("Extracting comments from YAML");
            var comments = new Dictionary<string, string>();
            var lines = yaml.Split('\n');
            var currentIndent = 0;
            var lastNonCommentLine = string.Empty;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].TrimStart();
                var indent = lines[i].Length - line.Length;

                if (line.StartsWith("#"))
                {
                    // This is a comment
                    if (!string.IsNullOrEmpty(lastNonCommentLine))
                    {
                        var comment = line.Substring(1).Trim();
                        comments[lastNonCommentLine] = comment;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    // This is a content line
                    lastNonCommentLine = line;
                    currentIndent = indent;
                }
            }

            return comments;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting comments");
            return new Dictionary<string, string>();
        }
    }

    private bool ValidateModel(ThreagileModel model)
    {
        if (model == null)
        {
            _logger.LogWarning("The model is null");
            return false;
        }

        bool isValid = true;

        // Validate metadata
        if (string.IsNullOrEmpty(model.Title))
        {
            _logger.LogWarning("The model has no title");
            isValid = false;
        }

        // Validate technical assets
        if (model.TechnicalAssets == null || !model.TechnicalAssets.Any())
        {
            _logger.LogWarning("The model contains no technical assets");
            isValid = false;
        }
        else
        {
            foreach (var asset in model.TechnicalAssets)
            {
                if (string.IsNullOrEmpty(asset.Id))
                {
                    _logger.LogWarning("A technical asset has no ID");
                    isValid = false;
                }
                
                if (string.IsNullOrEmpty(asset.Title))
                {
                    _logger.LogWarning("Technical asset {Id} has no title", asset.Id);
                    isValid = false;
                }
                
                if (string.IsNullOrEmpty(asset.Type))
                {
                    _logger.LogWarning("Technical asset {Id} has no type", asset.Id);
                    isValid = false;
                }
            }
            
            // Check for duplicate IDs
            var duplicateAssetIds = model.TechnicalAssets
                .GroupBy(a => a.Id)
                .Where(g => g.Count() > 1 && !string.IsNullOrEmpty(g.Key))
                .Select(g => g.Key)
                .ToList();
                
            if (duplicateAssetIds.Any())
            {
                _logger.LogWarning("Duplicate technical asset IDs found: {DuplicateIds}", string.Join(", ", duplicateAssetIds));
                isValid = false;
            }
        }

        // Validate trust boundaries
        if (model.TrustBoundaries != null)
        {
            foreach (var boundary in model.TrustBoundaries)
            {
                if (string.IsNullOrEmpty(boundary.Id))
                {
                    _logger.LogWarning("A trust boundary has no ID");
                    isValid = false;
                }
                
                if (string.IsNullOrEmpty(boundary.Title))
                {
                    _logger.LogWarning("Trust boundary {Id} has no title", boundary.Id);
                    isValid = false;
                }
                
                if (string.IsNullOrEmpty(boundary.Type))
                {
                    _logger.LogWarning("Trust boundary {Id} has no type", boundary.Id);
                    isValid = false;
                }
                
                // Validate that referenced technical assets exist
                if (boundary.TechnicalAssets != null)
                {
                    foreach (var assetId in boundary.TechnicalAssets)
                    {
                        if (model.TechnicalAssets == null || !model.TechnicalAssets.Any(a => a.Id == assetId))
                        {
                            _logger.LogWarning("Trust boundary {Id} references non-existent technical asset: {AssetId}", boundary.Id, assetId);
                            isValid = false;
                        }
                    }
                }
            }
        }

        // Validate communication links
        if (model.CommunicationLinks != null)
        {
            foreach (var link in model.CommunicationLinks)
            {
                if (string.IsNullOrEmpty(link.Id))
                {
                    _logger.LogWarning("A communication link has no ID");
                    isValid = false;
                }
                
                if (string.IsNullOrEmpty(link.SourceId))
                {
                    _logger.LogWarning("Communication link {Id} has no source", link.Id);
                    isValid = false;
                }
                
                if (string.IsNullOrEmpty(link.TargetId))
                {
                    _logger.LogWarning("Communication link {Id} has no target", link.Id);
                    isValid = false;
                }
                
                // Validate that source and target exist
                if (!string.IsNullOrEmpty(link.SourceId) && (model.TechnicalAssets == null || !model.TechnicalAssets.Any(a => a.Id == link.SourceId)))
                {
                    _logger.LogWarning("Communication link {Id} references non-existent source: {Source}", link.Id, link.SourceId);
                    isValid = false;
                }
                
                if (!string.IsNullOrEmpty(link.TargetId) && (model.TechnicalAssets == null || !model.TechnicalAssets.Any(a => a.Id == link.TargetId)))
                {
                    _logger.LogWarning("Communication link {Id} references non-existent target: {Target}", link.Id, link.TargetId);
                    isValid = false;
                }
            }
        }

        // Validate shared runtimes
        if (model.SharedRuntimes != null)
        {
            foreach (var runtime in model.SharedRuntimes)
            {
                if (string.IsNullOrEmpty(runtime.Id))
                {
                    _logger.LogWarning("A shared runtime has no ID");
                    isValid = false;
                }
                
                if (string.IsNullOrEmpty(runtime.Name))
                {
                    _logger.LogWarning("Shared runtime {Id} has no name", runtime.Id);
                    isValid = false;
                }
            }
        }

        // Validate data assets
        if (model.DataAssets != null)
        {
            foreach (var asset in model.DataAssets)
            {
                if (string.IsNullOrEmpty(asset.Id))
                {
                    _logger.LogWarning("A data asset has no ID");
                    isValid = false;
                }
                
                if (string.IsNullOrEmpty(asset.Name))
                {
                    _logger.LogWarning("Data asset {Id} has no name", asset.Id);
                    isValid = false;
                }
                
                if (string.IsNullOrEmpty(asset.Type))
                {
                    _logger.LogWarning("Data asset {Id} has no type", asset.Id);
                    isValid = false;
                }
            }
        }

        return isValid;
    }
}

/// <summary>
/// Converts ThreagileStyle to and from YAML
/// </summary>
public class YamlStyleConverter : IYamlTypeConverter
{
    /// <summary>
    /// Determines whether this converter can convert the specified type
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if the type can be converted, false otherwise</returns>
    public bool Accepts(Type type)
    {
        return type == typeof(ThreagileStyle);
    }

    /// <summary>
    /// Reads a YAML value and converts it to a ThreagileStyle
    /// </summary>
    /// <param name="parser">The YAML parser</param>
    /// <param name="type">The type to convert to</param>
    /// <param name="deserializer">The object deserializer</param>
    /// <returns>The converted ThreagileStyle</returns>
    public object ReadYaml(IParser parser, Type type, ObjectDeserializer deserializer)
    {
        var style = new ThreagileStyle();
        var dict = deserializer(typeof(Dictionary<string, string>));
        if (dict is Dictionary<string, string> properties)
        {
            if (properties.TryGetValue("fillColor", out var fillColor))
                style.FillColor = fillColor;
            if (properties.TryGetValue("strokeColor", out var strokeColor))
                style.StrokeColor = strokeColor;
            if (properties.TryGetValue("fontColor", out var fontColor))
                style.FontColor = fontColor;
            if (properties.TryGetValue("fontStyle", out var fontStyle))
                style.FontStyle = fontStyle;
            if (properties.TryGetValue("fontSize", out var fontSize) && int.TryParse(fontSize, out var size))
                style.FontSize = size;
            if (properties.TryGetValue("shape", out var shape))
                style.Shape = shape;
            style.Properties = properties;
        }
        return style;
    }

    /// <summary>
    /// Writes a ThreagileStyle to YAML
    /// </summary>
    /// <param name="emitter">The YAML emitter</param>
    /// <param name="value">The value to write</param>
    /// <param name="type">The type of the value</param>
    /// <param name="serializer">The object serializer</param>
    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (value is ThreagileStyle style)
        {
            var dict = new Dictionary<string, string>
            {
                ["fillColor"] = style.FillColor,
                ["strokeColor"] = style.StrokeColor,
                ["fontColor"] = style.FontColor,
                ["fontStyle"] = style.FontStyle,
                ["fontSize"] = style.FontSize.ToString(),
                ["shape"] = style.Shape
            };
            foreach (var prop in style.Properties)
            {
                dict[prop.Key] = prop.Value;
            }
            serializer(dict, typeof(Dictionary<string, string>));
        }
    }
}

/// <summary>
/// Converts Dictionary<string, string> to and from YAML
/// </summary>
public class YamlPropertiesConverter : IYamlTypeConverter
{
    /// <summary>
    /// Determines whether this converter can convert the specified type
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if the type can be converted, false otherwise</returns>
    public bool Accepts(Type type)
    {
        return type == typeof(Dictionary<string, string>);
    }

    /// <summary>
    /// Reads a YAML value and converts it to a Dictionary<string, string>
    /// </summary>
    /// <param name="parser">The YAML parser</param>
    /// <param name="type">The type to convert to</param>
    /// <param name="deserializer">The object deserializer</param>
    /// <returns>The converted Dictionary<string, string></returns>
    public object ReadYaml(IParser parser, Type type, ObjectDeserializer deserializer)
    {
        var dict = new Dictionary<string, string>();
        var node = parser.Current;
        if (node is YamlDotNet.Core.Events.MappingStart)
        {
            parser.MoveNext();
            while (parser.Current is not YamlDotNet.Core.Events.MappingEnd)
            {
                var key = parser.Current.ToString();
                parser.MoveNext();
                var value = parser.Current.ToString();
                dict[key] = value;
                parser.MoveNext();
            }
            parser.MoveNext();
        }
        return dict;
    }

    /// <summary>
    /// Writes a Dictionary<string, string> to YAML
    /// </summary>
    /// <param name="emitter">The YAML emitter</param>
    /// <param name="value">The value to write</param>
    /// <param name="type">The type of the value</param>
    /// <param name="serializer">The object serializer</param>
    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (value is Dictionary<string, string> dict)
        {
            emitter.Emit(new YamlDotNet.Core.Events.MappingStart(null, null, false, YamlDotNet.Core.Events.MappingStyle.Block));
            foreach (var kvp in dict)
            {
                emitter.Emit(new YamlDotNet.Core.Events.Scalar(null, kvp.Key));
                emitter.Emit(new YamlDotNet.Core.Events.Scalar(null, kvp.Value));
            }
            emitter.Emit(new YamlDotNet.Core.Events.MappingEnd());
        }
    }
} 
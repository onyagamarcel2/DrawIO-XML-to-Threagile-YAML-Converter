using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using ThreagileConverter.Core.Models;

namespace ThreagileConverter.Core.Parsing;

/// <summary>
/// Implementation of the YAML parser
/// </summary>
public class YamlParser : IParser
{
    private readonly ILogger<YamlParser> _logger;
    private readonly IDeserializer _deserializer;

    public YamlParser(ILogger<YamlParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    /// <summary>
    /// Parses a YAML file and converts it to XML
    /// </summary>
    /// <param name="filePath">The path to the YAML file</param>
    /// <returns>The XML document</returns>
    public async Task<XDocument> ParseXmlAsync(string filePath)
    {
        _logger.LogInformation("Parsing YAML file: {FilePath}", filePath);
        try
        {
            var yamlContent = await File.ReadAllTextAsync(filePath);
            var model = _deserializer.Deserialize<ThreagileModel>(yamlContent);
            return await ConvertToXmlAsync(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing YAML file");
            throw;
        }
    }

    /// <summary>
    /// Converts a ThreagileModel to XML
    /// </summary>
    /// <param name="model">The model to convert</param>
    /// <returns>The XML document</returns>
    private async Task<XDocument> ConvertToXmlAsync(ThreagileModel model)
    {
        var doc = new XDocument(
            new XElement("mxfile",
                new XElement("diagram",
                    new XElement("mxGraphModel",
                        new XElement("root",
                            new XElement("mxCell", new XAttribute("id", "0")),
                            new XElement("mxCell", new XAttribute("id", "1"), new XAttribute("parent", "0")),
                            await ConvertTechnicalAssetsAsync(model),
                            await ConvertTrustBoundariesAsync(model),
                            await ConvertCommunicationLinksAsync(model)
                        )
                    )
                )
            )
        );

        return doc;
    }

    /// <summary>
    /// Converts technical assets to XML elements
    /// </summary>
    /// <param name="model">The model containing technical assets</param>
    /// <returns>The XML elements for technical assets</returns>
    private async Task<XElement> ConvertTechnicalAssetsAsync(ThreagileModel model)
    {
        var assets = new XElement("mxCell");
        foreach (var asset in model.TechnicalAssets)
        {
            assets.Add(new XElement("mxCell",
                new XAttribute("id", asset.Id),
                new XAttribute("value", asset.Title),
                new XAttribute("style", ConvertStyle(asset.Style)),
                new XAttribute("vertex", "1"),
                new XAttribute("parent", "1"),
                new XElement("mxGeometry",
                    new XAttribute("x", "120"),
                    new XAttribute("y", "120"),
                    new XAttribute("width", "120"),
                    new XAttribute("height", "60"),
                    new XAttribute("as", "geometry")
                )
            ));
        }
        return assets;
    }

    /// <summary>
    /// Converts trust boundaries to XML elements
    /// </summary>
    /// <param name="model">The model containing trust boundaries</param>
    /// <returns>The XML elements for trust boundaries</returns>
    private async Task<XElement> ConvertTrustBoundariesAsync(ThreagileModel model)
    {
        var boundaries = new XElement("mxCell");
        foreach (var boundary in model.TrustBoundaries)
        {
            boundaries.Add(new XElement("mxCell",
                new XAttribute("id", boundary.Id),
                new XAttribute("value", boundary.Title),
                new XAttribute("style", "swimlane;fontStyle=0;childLayout=stackLayout;horizontal=1;startSize=26;horizontalStack=0;resizeParent=1;resizeParentMax=0;resizeLast=0;collapsible=1;marginBottom=0;"),
                new XAttribute("vertex", "1"),
                new XAttribute("parent", "1"),
                new XElement("mxGeometry",
                    new XAttribute("x", "40"),
                    new XAttribute("y", "40"),
                    new XAttribute("width", "200"),
                    new XAttribute("height", "200"),
                    new XAttribute("as", "geometry")
                )
            ));
        }
        return boundaries;
    }

    /// <summary>
    /// Converts communication links to XML elements
    /// </summary>
    /// <param name="model">The model containing communication links</param>
    /// <returns>The XML elements for communication links</returns>
    private async Task<XElement> ConvertCommunicationLinksAsync(ThreagileModel model)
    {
        var links = new XElement("mxCell");
        foreach (var link in model.CommunicationLinks)
        {
            links.Add(new XElement("mxCell",
                new XAttribute("id", link.Id),
                new XAttribute("value", link.Title),
                new XAttribute("style", "endArrow=classic;html=1;"),
                new XAttribute("edge", "1"),
                new XAttribute("parent", "1"),
                new XAttribute("source", link.Source),
                new XAttribute("target", link.Target),
                new XElement("mxGeometry",
                    new XAttribute("width", "50"),
                    new XAttribute("height", "50"),
                    new XAttribute("relative", "1"),
                    new XAttribute("as", "geometry")
                )
            ));
        }
        return links;
    }

    /// <summary>
    /// Converts a style object to a style string
    /// </summary>
    /// <param name="style">The style to convert</param>
    /// <returns>The style string</returns>
    private string ConvertStyle(ThreagileStyle style)
    {
        if (style == null) return string.Empty;

        var styleParts = new List<string>();
        if (!string.IsNullOrEmpty(style.FillColor))
            styleParts.Add($"fillColor={style.FillColor}");
        if (!string.IsNullOrEmpty(style.StrokeColor))
            styleParts.Add($"strokeColor={style.StrokeColor}");
        if (!string.IsNullOrEmpty(style.FontColor))
            styleParts.Add($"fontColor={style.FontColor}");
        if (!string.IsNullOrEmpty(style.FontStyle))
            styleParts.Add($"fontStyle={style.FontStyle}");
        if (style.FontSize > 0)
            styleParts.Add($"fontSize={style.FontSize}");
        if (!string.IsNullOrEmpty(style.Shape))
            styleParts.Add($"shape={style.Shape}");

        return string.Join(";", styleParts);
    }

    public Task ProcessXmlStreamingAsync(string filePath, Func<XElement, Task> onElement, string? elementName = null)
    {
        // Implémentation basique : lit le fichier et appelle onElement pour chaque élément racine
        var doc = XDocument.Load(filePath);
        if (doc.Root != null)
        {
            return onElement(doc.Root);
        }
        return Task.CompletedTask;
    }

    public Task<XDocument> ParseXmlStreamingAsync(string filePath, int batchSize)
    {
        // Implémentation basique : lit le fichier et retourne le XDocument
        var doc = XDocument.Load(filePath);
        return Task.FromResult(doc);
    }
} 
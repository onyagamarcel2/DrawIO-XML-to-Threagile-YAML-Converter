using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using ThreagileConverter.Core.Models;

namespace ThreagileConverter.Core.Parsing
{
    /// <summary>
    /// Classe pour parser les éléments DrawIO
    /// </summary>
    public class DrawIOParser
    {
        private readonly ILogger<DrawIOParser> _logger;

        public DrawIOParser(ILogger<DrawIOParser> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Extrait les styles d'un élément DrawIO
        /// </summary>
        /// <param name="element">L'élément DrawIO</param>
        /// <returns>Un dictionnaire des styles extraits</returns>
        public Dictionary<string, DrawIOStyle> ExtractStyles(XElement element)
        {
            _logger.LogInformation("Extraction des styles du diagramme DrawIO");
            var styles = new Dictionary<string, DrawIOStyle>();

            try
            {
                // Extraire les styles des cellules
                var cells = element.Descendants("mxCell");
                foreach (var cell in cells)
                {
                    var style = cell.Attribute("style")?.Value;
                    if (!string.IsNullOrEmpty(style))
                    {
                        var drawIOStyle = DrawIOStyle.Parse(style);
                        styles[cell.Attribute("id")?.Value ?? Guid.NewGuid().ToString()] = drawIOStyle;
                    }
                }

                // Extraire les styles des objets
                var objects = element.Descendants("UserObject");
                foreach (var obj in objects)
                {
                    var style = obj.Attribute("style")?.Value;
                    if (!string.IsNullOrEmpty(style))
                    {
                        var drawIOStyle = DrawIOStyle.Parse(style);
                        styles[obj.Attribute("id")?.Value ?? Guid.NewGuid().ToString()] = drawIOStyle;
                    }
                }

                _logger.LogInformation("Extraction des styles terminée : {Count} styles trouvés", styles.Count);
                return styles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'extraction des styles");
                return styles;
            }
        }

        /// <summary>
        /// Extrait les métadonnées d'un élément DrawIO
        /// </summary>
        /// <param name="element">L'élément DrawIO</param>
        /// <returns>Les métadonnées extraites</returns>
        public DrawIOMetadata ExtractMetadata(XElement element)
        {
            _logger.LogInformation("Extraction des métadonnées du diagramme DrawIO");
            var metadata = new DrawIOMetadata();

            try
            {
                var diagram = element.Element("diagram");
                if (diagram != null)
                {
                    metadata.Title = diagram.Attribute("name")?.Value;
                    metadata.Description = diagram.Attribute("description")?.Value;
                    metadata.Author = diagram.Attribute("author")?.Value;
                    metadata.Created = diagram.Attribute("created")?.Value;
                    metadata.Modified = diagram.Attribute("modified")?.Value;
                    metadata.Version = diagram.Attribute("version")?.Value;

                    var tags = diagram.Attribute("tags")?.Value;
                    if (!string.IsNullOrEmpty(tags))
                    {
                        metadata.Tags = tags;
                    }

                    // Extraire les propriétés supplémentaires
                    foreach (var attr in diagram.Attributes())
                    {
                        if (!IsStandardAttribute(attr.Name.LocalName))
                        {
                            metadata.Properties[attr.Name.LocalName] = attr.Value;
                        }
                    }
                }

                _logger.LogInformation("Extraction des métadonnées terminée");
                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'extraction des métadonnées");
                return metadata;
            }
        }

        /// <summary>
        /// Extrait les formes d'un élément DrawIO
        /// </summary>
        /// <param name="element">L'élément DrawIO</param>
        /// <returns>La liste des formes extraites</returns>
        public List<DrawIOShape> ExtractShapes(XElement element)
        {
            _logger.LogInformation("Extraction des formes DrawIO");
            var shapes = new List<DrawIOShape>();

            try
            {
                var diagram = element.Element("diagram");
                if (diagram == null)
                {
                    _logger.LogWarning("Élément diagram non trouvé");
                    return shapes;
                }

                var mxGraphModel = diagram.Element("mxGraphModel");
                if (mxGraphModel == null)
                {
                    _logger.LogWarning("Élément mxGraphModel non trouvé");
                    return shapes;
                }

                var root = mxGraphModel.Element("root");
                if (root == null)
                {
                    _logger.LogWarning("Élément root non trouvé");
                    return shapes;
                }

                // Extraire les formes de premier niveau
                foreach (var child in root.Elements())
                {
                    if (child.Name.LocalName != "mxCell")
                    {
                        var shape = new DrawIOShape
                        {
                            Id = child.Attribute("id")?.Value ?? string.Empty,
                            Value = child.Attribute("value")?.Value ?? string.Empty,
                            Style = ExtractStyle(child),
                            Geometry = ExtractGeometry(child),
                            Children = new List<DrawIOShape>()
                        };
                        ExtractChildShapes(child, shape);
                        shapes.Add(shape);
                    }
                }

                _logger.LogInformation("{Count} formes extraites", shapes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'extraction des formes");
            }

            return shapes;
        }

        private void ExtractChildShapes(XElement parentElement, DrawIOShape parentShape)
        {
            foreach (var child in parentElement.Elements())
            {
                if (child.Name.LocalName != "mxCell")
                {
                    var shape = new DrawIOShape
                    {
                        Id = child.Attribute("id")?.Value ?? string.Empty,
                        Value = child.Attribute("value")?.Value ?? string.Empty,
                        Style = ExtractStyle(child),
                        Geometry = ExtractGeometry(child),
                        Children = new List<DrawIOShape>()
                    };
                    ExtractChildShapes(child, shape);
                    parentShape.Children.Add(shape);
                }
            }
        }

        /// <summary>
        /// Extrait les relations d'un élément DrawIO
        /// </summary>
        /// <param name="element">L'élément DrawIO</param>
        /// <returns>La liste des relations extraites</returns>
        public List<DrawIORelation> ExtractRelations(XElement element)
        {
            _logger.LogInformation("Extraction des relations DrawIO");
            var relations = new List<DrawIORelation>();

            try
            {
                var diagram = element.Element("diagram");
                if (diagram == null)
                {
                    _logger.LogWarning("Élément diagram non trouvé");
                    return relations;
                }

                var mxGraphModel = diagram.Element("mxGraphModel");
                if (mxGraphModel == null)
                {
                    _logger.LogWarning("Élément mxGraphModel non trouvé");
                    return relations;
                }

                var root = mxGraphModel.Element("root");
                if (root == null)
                {
                    _logger.LogWarning("Élément root non trouvé");
                    return relations;
                }

                // Extraire les relations (éléments mxCell avec source et target)
                foreach (var cell in root.Elements("mxCell"))
                {
                    var source = cell.Attribute("source")?.Value;
                    var target = cell.Attribute("target")?.Value;

                    if (!string.IsNullOrEmpty(source) && !string.IsNullOrEmpty(target))
                    {
                        var relation = new DrawIORelation
                        {
                            Id = cell.Attribute("id")?.Value ?? string.Empty,
                            Value = cell.Attribute("value")?.Value ?? string.Empty,
                            SourceId = source,
                            TargetId = target,
                            Style = ExtractStyle(cell),
                            Geometry = ExtractGeometry(cell)
                        };
                        relations.Add(relation);
                    }
                }

                _logger.LogInformation("{Count} relations extraites", relations.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'extraction des relations");
            }

            return relations;
        }

        private bool IsStandardAttribute(string name)
        {
            return name switch
            {
                "name" or "description" or "author" or "created" or "modified" or "version" or "tags" => true,
                _ => false
            };
        }

        private DrawIOStyle ExtractStyle(XElement element)
        {
            var style = new DrawIOStyle();
            var styleAttr = element.Attribute("style")?.Value;
            if (!string.IsNullOrEmpty(styleAttr))
            {
                var properties = styleAttr.Split(';');
                foreach (var prop in properties)
                {
                    var parts = prop.Split('=');
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var value = parts[1].Trim();
                        style.Properties[key] = value;
                        switch (key)
                        {
                            case "fillColor":
                                style.FillColor = value;
                                break;
                            case "strokeColor":
                                style.StrokeColor = value;
                                break;
                            case "fontColor":
                                style.FontColor = value;
                                break;
                            case "fontStyle":
                                style.FontStyle = value;
                                break;
                            case "fontSize":
                                if (int.TryParse(value, out int size))
                                    style.FontSize = size;
                                break;
                            case "shape":
                                style.Shape = value;
                                break;
                        }
                    }
                }
            }
            return style;
        }

        private DrawIOGeometry ExtractGeometry(XElement element)
        {
            var geometry = new DrawIOGeometry();
            var geoElement = element.Element("mxGeometry");
            if (geoElement != null)
            {
                geometry.X = double.TryParse(geoElement.Attribute("x")?.Value, out double x) ? x : 0;
                geometry.Y = double.TryParse(geoElement.Attribute("y")?.Value, out double y) ? y : 0;
                geometry.Width = double.TryParse(geoElement.Attribute("width")?.Value, out double w) ? w : 0;
                geometry.Height = double.TryParse(geoElement.Attribute("height")?.Value, out double h) ? h : 0;
            }
            return geometry;
        }
    }
} 
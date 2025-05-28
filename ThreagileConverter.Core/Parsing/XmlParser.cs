using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Microsoft.Extensions.Logging;
using ThreagileConverter.Core.Logging;
using System.Threading.Tasks;

namespace ThreagileConverter.Core.Parsing;

public class XmlParser
{
    private readonly ILogger<XmlParser> _logger;
    private readonly string _xsdPath;

    public XmlParser(ILogger<XmlParser> logger)
    {
        _logger = logger;
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var projectDir = Path.GetDirectoryName(Path.GetDirectoryName(baseDir));
        _xsdPath = Path.Combine(projectDir, "ThreagileConverter.Tests", "bin", "Debug", "net9.0", "Schemas", "drawio.xsd");
    }

    public XDocument ParseXml(string filePath)
    {
        try
        {
            _logger.LogInformation("Début du parsing du fichier XML : {FilePath}", filePath);

            if (!File.Exists(filePath))
            {
                var message = $"Le fichier XML n'existe pas : {filePath}";
                _logger.LogError(message);
                throw new XmlParserException(message, filePath, XmlParserErrorType.FileNotFound);
            }

            try
            {
                var xmlContent = File.ReadAllText(filePath);
                var doc = XDocument.Parse(xmlContent);
                
                // Gestion des namespaces
                try
                {
                    var namespaces = ExtractNamespaces(doc);
                    if (namespaces.Any())
                    {
                        _logger.LogInformation("Namespaces trouvés dans le document : {Namespaces}", 
                            string.Join(", ", namespaces.Select(n => n.ToString())));
                    }
                }
                catch (Exception ex)
                {
                    var message = $"Erreur lors de l'extraction des namespaces : {ex.Message}";
                    _logger.LogError(ex, message);
                    throw new XmlParserException(message, filePath, XmlParserErrorType.NamespaceError, ex);
                }

                // Validation XSD
                try
                {
                    var validationResult = ValidateAgainstXsd(doc);
                    if (!validationResult.IsValid)
                    {
                        var errors = string.Join(", ", validationResult.Errors);
                        var message = $"Le document XML ne respecte pas le schéma XSD : {errors}";
                        _logger.LogWarning(message);
                        throw new XmlParserException(message, filePath, XmlParserErrorType.ValidationError);
                    }
                }
                catch (XmlParserException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    var message = $"Erreur lors de la validation XSD : {ex.Message}";
                    _logger.LogError(ex, message);
                    throw new XmlParserException(message, filePath, XmlParserErrorType.ValidationError, ex);
                }

                _logger.LogInformation("Parsing XML réussi pour le fichier : {FilePath}", filePath);
                return doc;
            }
            catch (XmlException ex)
            {
                var message = $"Le fichier XML est invalide : {ex.Message}";
                _logger.LogError(ex, message);
                throw new XmlParserException(message, filePath, XmlParserErrorType.InvalidXml, ex);
            }
        }
        catch (XmlParserException)
        {
            throw;
        }
        catch (UnauthorizedAccessException ex)
        {
            var message = $"Accès refusé au fichier XML : {filePath}";
            _logger.LogError(ex, message);
            throw new XmlParserException(message, filePath, XmlParserErrorType.FileAccessDenied, ex);
        }
        catch (Exception ex)
        {
            var message = $"Erreur inattendue lors du parsing du fichier XML : {ex.Message}";
            _logger.LogError(ex, message);
            throw new XmlParserException(message, filePath, XmlParserErrorType.Unknown, ex);
        }
    }

    /// <summary>
    /// Valide un document XML contre le schéma XSD
    /// </summary>
    /// <param name="doc">Le document XML à valider</param>
    /// <returns>Un résultat de validation contenant les erreurs éventuelles</returns>
    public ValidationResult ValidateAgainstXsd(XDocument doc)
    {
        if (doc == null)
            throw new ArgumentNullException(nameof(doc));

        if (!File.Exists(_xsdPath))
        {
            var message = $"Le fichier de schéma XSD n'existe pas : {_xsdPath}";
            _logger.LogError(message);
            throw new XmlParserException(message, _xsdPath, XmlParserErrorType.FileNotFound);
        }

        var errors = new List<string>();
        var schemas = new XmlSchemaSet();
        schemas.Add(null, _xsdPath);

        doc.Validate(schemas, (sender, args) =>
        {
            errors.Add(args.Message);
            _logger.LogWarning("Erreur de validation XSD : {Message}", args.Message);
        });

        return new ValidationResult
        {
            IsValid = !errors.Any(),
            Errors = errors
        };
    }

    /// <summary>
    /// Extrait tous les namespaces déclarés dans le document XML
    /// </summary>
    /// <param name="doc">Le document XML à analyser</param>
    /// <returns>Une collection de namespaces</returns>
    public IEnumerable<XNamespace> ExtractNamespaces(XDocument doc)
    {
        if (doc?.Root == null)
            return Enumerable.Empty<XNamespace>();

        var namespaces = new HashSet<XNamespace>();

        // Récupère les namespaces déclarés dans l'élément racine
        foreach (var attr in doc.Root.Attributes())
        {
            if (attr.IsNamespaceDeclaration)
            {
                namespaces.Add(XNamespace.Get(attr.Value));
            }
        }

        // Récupère les namespaces utilisés dans les éléments
        foreach (var element in doc.Descendants())
        {
            if (element.Name.Namespace != XNamespace.None)
            {
                namespaces.Add(element.Name.Namespace);
            }
        }

        return namespaces;
    }

    /// <summary>
    /// Vérifie si un namespace est déclaré dans le document
    /// </summary>
    /// <param name="doc">Le document XML à analyser</param>
    /// <param name="namespaceUri">L'URI du namespace à vérifier</param>
    /// <returns>True si le namespace est déclaré, False sinon</returns>
    public bool IsNamespaceDeclared(XDocument doc, string namespaceUri)
    {
        if (doc?.Root == null || string.IsNullOrEmpty(namespaceUri))
            return false;

        return doc.Root.Attributes()
            .Where(attr => attr.IsNamespaceDeclaration)
            .Any(attr => attr.Value == namespaceUri);
    }

    /// <summary>
    /// Récupère le préfixe associé à un namespace dans le document
    /// </summary>
    /// <param name="doc">Le document XML à analyser</param>
    /// <param name="namespaceUri">L'URI du namespace</param>
    /// <returns>Le préfixe du namespace ou null si non trouvé</returns>
    public string GetNamespacePrefix(XDocument doc, string namespaceUri)
    {
        if (doc?.Root == null || string.IsNullOrEmpty(namespaceUri))
            return null;

        var attr = doc.Root.Attributes()
            .FirstOrDefault(a => a.IsNamespaceDeclaration && a.Value == namespaceUri);

        return attr?.Name.LocalName;
    }

    /// <summary>
    /// Extrait les références externes du document XML
    /// </summary>
    /// <param name="doc">Le document XML à analyser</param>
    /// <returns>Une collection de références externes</returns>
    public IEnumerable<ExternalReference> ExtractExternalReferences(XDocument doc)
    {
        if (doc?.Root == null)
            return Enumerable.Empty<ExternalReference>();

        try
        {
            var references = new List<ExternalReference>();

            // Recherche des références dans les attributs href
            foreach (var element in doc.Descendants())
            {
                var hrefAttr = element.Attribute("href");
                if (hrefAttr != null)
                {
                    references.Add(new ExternalReference
                    {
                        SourceElement = element,
                        ReferencePath = hrefAttr.Value,
                        ReferenceType = ExternalReferenceType.Href
                    });
                }

                // Recherche des références dans les attributs src
                var srcAttr = element.Attribute("src");
                if (srcAttr != null)
                {
                    references.Add(new ExternalReference
                    {
                        SourceElement = element,
                        ReferencePath = srcAttr.Value,
                        ReferenceType = ExternalReferenceType.Source
                    });
                }

                // Recherche des références dans les attributs data
                var dataAttr = element.Attribute("data");
                if (dataAttr != null && dataAttr.Value.StartsWith("data:"))
                {
                    references.Add(new ExternalReference
                    {
                        SourceElement = element,
                        ReferencePath = dataAttr.Value,
                        ReferenceType = ExternalReferenceType.DataUri
                    });
                }
            }

            _logger.LogInformation("Références externes trouvées : {Count}", references.Count);
            return references;
        }
        catch (Exception ex)
        {
            var message = $"Erreur lors de l'extraction des références externes : {ex.Message}";
            _logger.LogError(ex, message);
            throw new XmlParserException(message, doc.BaseUri, XmlParserErrorType.ExternalReferenceError, ex);
        }
    }

    /// <summary>
    /// Vérifie si une référence externe est valide
    /// </summary>
    /// <param name="reference">La référence à vérifier</param>
    /// <returns>True si la référence est valide, False sinon</returns>
    public bool IsExternalReferenceValid(ExternalReference reference)
    {
        if (reference == null)
            return false;

        try
        {
            switch (reference.ReferenceType)
            {
                case ExternalReferenceType.Href:
                case ExternalReferenceType.Source:
                    return Uri.IsWellFormedUriString(reference.ReferencePath, UriKind.RelativeOrAbsolute);
                case ExternalReferenceType.DataUri:
                    return reference.ReferencePath.StartsWith("data:") && 
                           reference.ReferencePath.Contains(";base64,");
                default:
                    return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erreur lors de la validation de la référence : {Reference}", reference.ReferencePath);
            return false;
        }
    }

    /// <summary>
    /// Résout une référence externe en chemin absolu
    /// </summary>
    /// <param name="reference">La référence à résoudre</param>
    /// <param name="basePath">Le chemin de base pour les références relatives</param>
    /// <returns>Le chemin absolu de la référence</returns>
    public string ResolveExternalReference(ExternalReference reference, string basePath)
    {
        if (reference == null || string.IsNullOrEmpty(basePath))
            return null;

        try
        {
            switch (reference.ReferenceType)
            {
                case ExternalReferenceType.Href:
                case ExternalReferenceType.Source:
                    if (Uri.IsWellFormedUriString(reference.ReferencePath, UriKind.Absolute))
                    {
                        return reference.ReferencePath;
                    }
                    else if (Uri.IsWellFormedUriString(reference.ReferencePath, UriKind.Relative))
                    {
                        return Path.GetFullPath(Path.Combine(basePath, reference.ReferencePath));
                    }
                    break;
                case ExternalReferenceType.DataUri:
                    return reference.ReferencePath; // Les Data URI sont déjà absolus
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erreur lors de la résolution de la référence : {Reference}", reference.ReferencePath);
        }

        return null;
    }

    /// <summary>
    /// Parse un fichier XML en utilisant le streaming pour une meilleure gestion de la mémoire
    /// </summary>
    /// <param name="filePath">Chemin du fichier XML à parser</param>
    /// <param name="chunkSize">Taille des chunks à lire (en octets)</param>
    /// <returns>Un document XML</returns>
    public async Task<XDocument> ParseXmlStreamingAsync(string filePath, int chunkSize = 8192)
    {
        try
        {
            _logger.LogInformation("Début du parsing en streaming du fichier XML : {FilePath}", filePath);

            if (!File.Exists(filePath))
            {
                var message = $"Le fichier XML n'existe pas : {filePath}";
                _logger.LogError(message);
                throw new XmlParserException(message, filePath, XmlParserErrorType.FileNotFound);
            }

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream);
            var settings = new XmlReaderSettings
            {
                Async = true,
                DtdProcessing = DtdProcessing.Ignore,
                ValidationType = ValidationType.None
            };

            using var xmlReader = XmlReader.Create(reader, settings);
            var doc = await XDocument.LoadAsync(xmlReader, LoadOptions.None, CancellationToken.None);

            // Validation XSD
            try
            {
                var validationResult = ValidateAgainstXsd(doc);
                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors);
                    var message = $"Le document XML ne respecte pas le schéma XSD : {errors}";
                    _logger.LogWarning(message);
                    throw new XmlParserException(message, filePath, XmlParserErrorType.ValidationError);
                }
            }
            catch (XmlParserException)
            {
                throw;
            }
            catch (Exception ex)
            {
                var message = $"Erreur lors de la validation XSD : {ex.Message}";
                _logger.LogError(ex, message);
                throw new XmlParserException(message, filePath, XmlParserErrorType.ValidationError, ex);
            }

            _logger.LogInformation("Parsing XML en streaming réussi pour le fichier : {FilePath}", filePath);
            return doc;
        }
        catch (XmlParserException)
        {
            throw;
        }
        catch (XmlException ex)
        {
            var message = $"Le fichier XML est invalide : {ex.Message}";
            _logger.LogError(ex, message);
            throw new XmlParserException(message, filePath, XmlParserErrorType.InvalidXml, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            var message = $"Accès refusé au fichier XML : {filePath}";
            _logger.LogError(ex, message);
            throw new XmlParserException(message, filePath, XmlParserErrorType.FileAccessDenied, ex);
        }
        catch (Exception ex)
        {
            var message = $"Erreur inattendue lors du parsing en streaming du fichier XML : {ex.Message}";
            _logger.LogError(ex, message);
            throw new XmlParserException(message, filePath, XmlParserErrorType.Unknown, ex);
        }
    }

    /// <summary>
    /// Parse un fichier XML en streaming et traite chaque élément au fur et à mesure
    /// </summary>
    /// <param name="filePath">Chemin du fichier XML à parser</param>
    /// <param name="elementHandler">Fonction de traitement pour chaque élément</param>
    /// <param name="elementName">Nom de l'élément à traiter (null pour tous les éléments)</param>
    /// <returns>Une tâche représentant l'opération asynchrone</returns>
    public async Task ProcessXmlStreamingAsync(string filePath, Func<XElement, Task> elementHandler, string elementName = null)
    {
        try
        {
            _logger.LogInformation("Début du traitement en streaming du fichier XML : {FilePath}", filePath);

            if (!File.Exists(filePath))
            {
                var message = $"Le fichier XML n'existe pas : {filePath}";
                _logger.LogError(message);
                throw new XmlParserException(message, filePath, XmlParserErrorType.FileNotFound);
            }

            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream);
            var settings = new XmlReaderSettings
            {
                Async = true,
                DtdProcessing = DtdProcessing.Ignore,
                ValidationType = ValidationType.None
            };

            using var xmlReader = XmlReader.Create(reader, settings);
            while (await xmlReader.ReadAsync())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    if (elementName == null || xmlReader.Name == elementName)
                    {
                        var element = XElement.ReadFrom(xmlReader) as XElement;
                        if (element != null)
                        {
                            await elementHandler(element);
                        }
                    }
                }
            }

            _logger.LogInformation("Traitement en streaming réussi pour le fichier : {FilePath}", filePath);
        }
        catch (XmlParserException)
        {
            throw;
        }
        catch (XmlException ex)
        {
            var message = $"Le fichier XML est invalide : {ex.Message}";
            _logger.LogError(ex, message);
            throw new XmlParserException(message, filePath, XmlParserErrorType.InvalidXml, ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            var message = $"Accès refusé au fichier XML : {filePath}";
            _logger.LogError(ex, message);
            throw new XmlParserException(message, filePath, XmlParserErrorType.FileAccessDenied, ex);
        }
        catch (Exception ex)
        {
            var message = $"Erreur inattendue lors du traitement en streaming du fichier XML : {ex.Message}";
            _logger.LogError(ex, message);
            throw new XmlParserException(message, filePath, XmlParserErrorType.Unknown, ex);
        }
    }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}

public class ExternalReference
{
    public XElement SourceElement { get; set; }
    public string ReferencePath { get; set; }
    public ExternalReferenceType ReferenceType { get; set; }
}

public enum ExternalReferenceType
{
    Href,
    Source,
    DataUri
} 
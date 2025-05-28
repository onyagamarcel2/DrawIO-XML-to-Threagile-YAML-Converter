using System.Xml.Linq;

namespace ThreagileConverter.Core.Validation;

/// <summary>
/// Interface pour les validateurs XML
/// </summary>
public interface IValidator
{
    /// <summary>
    /// Valide un document XML
    /// </summary>
    /// <param name="document">Document XML à valider</param>
    /// <returns>Résultat de la validation</returns>
    Task<ValidationResult> ValidateAsync(XDocument document);

    /// <summary>
    /// Valide un document XML contre un schéma XSD
    /// </summary>
    /// <param name="document">Document XML à valider</param>
    /// <param name="schemaPath">Chemin du schéma XSD</param>
    /// <returns>Résultat de la validation</returns>
    Task<ValidationResult> ValidateAgainstSchemaAsync(XDocument document, string schemaPath);
} 
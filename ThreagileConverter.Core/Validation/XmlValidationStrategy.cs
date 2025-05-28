using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace ThreagileConverter.Core.Validation
{
    /// <summary>
    /// Implémentation de la stratégie de validation pour les fichiers XML
    /// </summary>
    public class XmlValidationStrategy : IValidationStrategy
    {
        private readonly ILogger<XmlValidationStrategy> _logger;

        public XmlValidationStrategy(ILogger<XmlValidationStrategy> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool CanValidate(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return false;

            try
            {
                // Vérifie si le contenu commence par une déclaration XML ou un élément
                content = content.Trim();
                return content.StartsWith("<?xml") || content.StartsWith("<");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification du contenu XML");
                return false;
            }
        }

        public async Task<ValidationResult> ValidateAsync(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return ValidationResult.CreateFailure(new[] { new ValidationError("Le contenu XML ne peut pas être vide", ValidationSeverity.Error) });

            try
            {
                var errors = new List<ValidationError>();
                var warnings = new List<ValidationWarning>();

                // Parse le XML
                XDocument doc;
                try
                {
                    doc = XDocument.Parse(content);
                }
                catch (Exception ex)
                {
                    return ValidationResult.CreateFailure(new[] { new ValidationError($"Erreur de parsing XML: {ex.Message}", ValidationSeverity.Error) });
                }

                // Valide la structure de base
                if (doc.Root == null)
                {
                    errors.Add(new ValidationError("Le document XML doit avoir un élément racine", ValidationSeverity.Error));
                }
                else
                {
                    // Valide les éléments
                    ValidateElements(doc.Root, errors, warnings);
                }

                if (errors.Any())
                {
                    _logger.LogWarning("Validation XML échouée avec {Count} erreurs", errors.Count);
                    return ValidationResult.CreateFailure(errors);
                }

                if (warnings.Any())
                {
                    _logger.LogInformation("Validation XML réussie avec {Count} avertissements", warnings.Count);
                    return ValidationResult.WithWarnings(warnings);
                }

                _logger.LogInformation("Validation XML réussie");
                return ValidationResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue lors de la validation XML");
                return ValidationResult.CreateFailure(new[] { new ValidationError($"Erreur inattendue: {ex.Message}", ValidationSeverity.Error) });
            }
        }

        private void ValidateElements(XElement element, List<ValidationError> errors, List<ValidationWarning> warnings, string path = "")
        {
            var currentPath = string.IsNullOrEmpty(path) ? element.Name.LocalName : $"{path}/{element.Name.LocalName}";

            // Vérifie les attributs requis
            if (element.Name.LocalName == "mxfile" && !element.Attributes().Any(a => a.Name.LocalName == "host"))
            {
                warnings.Add(new ValidationWarning(
                    "L'attribut 'host' est recommandé pour l'élément mxfile",
                    "WARN_MISSING_HOST",
                    currentPath
                ));
            }

            // Vérifie les éléments enfants
            foreach (var child in element.Elements())
            {
                ValidateElements(child, errors, warnings, currentPath);
            }

            // Vérifie les attributs
            foreach (var attr in element.Attributes())
            {
                if (string.IsNullOrWhiteSpace(attr.Value))
                {
                    warnings.Add(new ValidationWarning(
                        $"L'attribut '{attr.Name.LocalName}' est vide",
                        "WARN_EMPTY_ATTRIBUTE",
                        $"{currentPath}/@{attr.Name.LocalName}"
                    ));
                }
            }
        }
    }
} 
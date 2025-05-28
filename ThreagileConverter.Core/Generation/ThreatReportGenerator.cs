using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ThreagileConverter.Core.Models;

namespace ThreagileConverter.Core.Generation;

/// <summary>
/// Générateur de rapport de menaces pour un modèle Threagile
/// </summary>
public class ThreatReportGenerator
{
    private readonly ILogger<ThreatReportGenerator> _logger;

    /// <summary>
    /// Crée une nouvelle instance de ThreatReportGenerator
    /// </summary>
    /// <param name="logger">Le logger</param>
    public ThreatReportGenerator(ILogger<ThreatReportGenerator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Génère un rapport de menaces pour un modèle Threagile
    /// </summary>
    /// <param name="model">Le modèle Threagile</param>
    /// <param name="outputPath">Le chemin du fichier de sortie</param>
    /// <returns>Une tâche représentant l'opération asynchrone</returns>
    public async Task GenerateReportAsync(ThreagileModel model, string outputPath)
    {
        try
        {
            _logger.LogInformation("Génération du rapport de menaces pour {Title}", model.Title);

            var threatAnalyzer = new ThreatAnalyzer();
            var analyzedModel = threatAnalyzer.AnalyzeThreats(model);

            var reportBuilder = new StringBuilder();
            
            // En-tête du rapport
            reportBuilder.AppendLine("# Rapport d'analyse de menaces");
            reportBuilder.AppendLine($"## Modèle: {model.Title}");
            reportBuilder.AppendLine($"### Date: {DateTime.Now:yyyy-MM-dd}");
            reportBuilder.AppendLine($"### Version: {model.Version}");
            reportBuilder.AppendLine($"### Auteur: {model.Author}");
            reportBuilder.AppendLine();
            
            // Résumé
            reportBuilder.AppendLine("## Résumé");
            reportBuilder.AppendLine($"- Nombre d'actifs techniques: {model.TechnicalAssets?.Count ?? 0}");
            reportBuilder.AppendLine($"- Nombre de périmètres de confiance: {model.TrustBoundaries?.Count ?? 0}");
            reportBuilder.AppendLine($"- Nombre de liens de communication: {model.CommunicationLinks?.Count ?? 0}");
            reportBuilder.AppendLine();
            
            // Analyse des menaces par actif technique
            reportBuilder.AppendLine("## Analyse des menaces par actif technique");
            
            if (model.TechnicalAssets != null)
            {
                foreach (var asset in model.TechnicalAssets)
                {
                    reportBuilder.AppendLine($"### {asset.Name} ({asset.Type})");
                    reportBuilder.AppendLine($"- **ID**: {asset.Id}");
                    reportBuilder.AppendLine($"- **Description**: {asset.Description}");
                    reportBuilder.AppendLine($"- **Confidentialité**: {asset.Properties.GetValueOrDefault("confidentiality", "medium")}");
                    reportBuilder.AppendLine($"- **Intégrité**: {asset.Properties.GetValueOrDefault("integrity", "medium")}");
                    reportBuilder.AppendLine($"- **Disponibilité**: {asset.Properties.GetValueOrDefault("availability", "medium")}");
                    
                    // Récupérer les menaces applicables pour ce type d'actif
                    var applicableThreats = ThreatCatalog.GetThreatsForAssetType(asset.Type);
                    
                    if (applicableThreats.Count > 0)
                    {
                        reportBuilder.AppendLine();
                        reportBuilder.AppendLine("#### Menaces potentielles");
                        
                        foreach (var threat in applicableThreats)
                        {
                            var confidentiality = asset.Properties.GetValueOrDefault("confidentiality", "medium");
                            var integrity = asset.Properties.GetValueOrDefault("integrity", "medium");
                            var availability = asset.Properties.GetValueOrDefault("availability", "medium");
                            
                            var risk = ThreatCatalog.EvaluateRisk(threat, confidentiality, integrity, availability);
                            
                            reportBuilder.AppendLine($"##### {threat.Name} - Risque: {risk.ToUpper()}");
                            reportBuilder.AppendLine($"- **ID**: {threat.Id}");
                            reportBuilder.AppendLine($"- **Catégorie**: {threat.Category}");
                            reportBuilder.AppendLine($"- **Description**: {threat.Description}");
                            reportBuilder.AppendLine("- **Mesures d'atténuation**:");
                            
                            foreach (var mitigation in threat.Mitigations)
                            {
                                reportBuilder.AppendLine($"  - {mitigation}");
                            }
                            
                            reportBuilder.AppendLine();
                        }
                    }
                    else
                    {
                        reportBuilder.AppendLine();
                        reportBuilder.AppendLine("#### Aucune menace spécifique identifiée pour ce type d'actif");
                    }
                    
                    reportBuilder.AppendLine();
                }
            }
            
            // Analyse des communications
            if (model.CommunicationLinks != null && model.CommunicationLinks.Count > 0)
            {
                reportBuilder.AppendLine("## Analyse des communications");
                
                foreach (var link in model.CommunicationLinks)
                {
                    var source = model.TechnicalAssets?.FirstOrDefault(a => a.Id == link.SourceId);
                    var target = model.TechnicalAssets?.FirstOrDefault(a => a.Id == link.TargetId);
                    
                    if (source != null && target != null)
                    {
                        reportBuilder.AppendLine($"### Communication: {source.Name} → {target.Name}");
                        reportBuilder.AppendLine($"- **ID**: {link.Id}");
                        reportBuilder.AppendLine($"- **Type**: {link.Type}");
                        reportBuilder.AppendLine($"- **Protocole**: {link.Protocol}");
                        reportBuilder.AppendLine($"- **Authentification**: {link.Authentication}");
                        reportBuilder.AppendLine($"- **Autorisation**: {link.Authorization}");
                        reportBuilder.AppendLine($"- **Chiffrement**: {link.Encryption}");
                        
                        // Analyse des risques pour cette communication
                        reportBuilder.AppendLine();
                        reportBuilder.AppendLine("#### Risques potentiels");
                        
                        bool risksIdentified = false;
                        
                        if (link.Authentication == "none")
                        {
                            risksIdentified = true;
                            reportBuilder.AppendLine("- **RISQUE ÉLEVÉ**: Communication sans authentification");
                            reportBuilder.AppendLine("  - *Recommandation*: Implémenter un mécanisme d'authentification approprié");
                            
                            if (source.Type == "web-application" || target.Type == "database")
                            {
                                reportBuilder.AppendLine("  - *Impact potentiel*: Accès non autorisé aux données sensibles");
                                reportBuilder.AppendLine("  - *Atténuation recommandée*: Utiliser OAuth2, JWT ou une authentification mutuelle TLS");
                            }
                        }
                        
                        if (link.Encryption == "none")
                        {
                            risksIdentified = true;
                            string riskLevel = "ÉLEVÉ";
                            
                            // Les communications avec des données sensibles sans chiffrement sont critiques
                            if (source.Type == "web-application" && target.Type == "database" || 
                                source.Properties.GetValueOrDefault("confidentiality", "medium") == "high" ||
                                target.Properties.GetValueOrDefault("confidentiality", "medium") == "high")
                            {
                                riskLevel = "CRITIQUE";
                            }
                            
                            reportBuilder.AppendLine($"- **RISQUE {riskLevel}**: Communication non chiffrée");
                            reportBuilder.AppendLine("  - *Recommandation*: Utiliser TLS pour chiffrer les communications");
                            reportBuilder.AppendLine("  - *Impact potentiel*: Interception de données sensibles (attaques de type 'man-in-the-middle')");
                            
                            if (link.Protocol == "http")
                            {
                                reportBuilder.AppendLine("  - *Atténuation spécifique*: Remplacer HTTP par HTTPS");
                            }
                        }
                        
                        if (link.Authorization == "none")
                        {
                            risksIdentified = true;
                            string riskLevel = "MOYEN";
                            
                            // Le contrôle d'accès est critique pour les services et les passerelles
                            if (target.Type == "gateway" || target.Type == "service")
                            {
                                riskLevel = "ÉLEVÉ";
                            }
                            
                            reportBuilder.AppendLine($"- **RISQUE {riskLevel}**: Communication sans autorisation");
                            reportBuilder.AppendLine("  - *Recommandation*: Implémenter un mécanisme d'autorisation basé sur les rôles");
                            reportBuilder.AppendLine("  - *Impact potentiel*: Élévation de privilèges, accès à des fonctionnalités non autorisées");
                        }
                        
                        // Analyse de la combinaison de vulnérabilités
                        if (link.Authentication == "none" && link.Encryption == "none")
                        {
                            reportBuilder.AppendLine("- **RISQUE CRITIQUE**: Communication sans authentification ET sans chiffrement");
                            reportBuilder.AppendLine("  - *Recommandation prioritaire*: Sécuriser immédiatement cette communication avec TLS et authentification");
                            reportBuilder.AppendLine("  - *Impact potentiel*: Compromission complète des données et du système");
                        }
                        
                        if (!risksIdentified)
                        {
                            reportBuilder.AppendLine("- Aucun risque majeur identifié pour cette communication");
                        }
                        
                        reportBuilder.AppendLine();
                    }
                }
            }
            
            // Recommandations générales
            reportBuilder.AppendLine("## Recommandations générales");
            reportBuilder.AppendLine("1. **Mettre en place une authentification forte** pour tous les services exposés");
            reportBuilder.AppendLine("2. **Chiffrer toutes les communications** entre les composants");
            reportBuilder.AppendLine("3. **Implémenter une journalisation centralisée** pour détecter les incidents");
            reportBuilder.AppendLine("4. **Effectuer des tests de sécurité réguliers** (SAST, DAST, tests de pénétration)");
            reportBuilder.AppendLine("5. **Appliquer le principe de moindre privilège** pour tous les accès");
            
            // Écriture du rapport dans un fichier
            await File.WriteAllTextAsync(outputPath, reportBuilder.ToString(), Encoding.UTF8);
            
            _logger.LogInformation("Rapport de menaces généré avec succès: {OutputPath}", outputPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la génération du rapport de menaces");
            throw;
        }
    }
} 
using System;
using System.Collections.Generic;

namespace ThreagileConverter.Core.Models;

/// <summary>
/// Représente une menace potentielle dans le modèle Threagile
/// </summary>
public class Threat
{
    /// <summary>
    /// Identifiant unique de la menace
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Nom de la menace
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description détaillée de la menace
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Catégorie de la menace (ex: injection, authentification, autorisation)
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Impact potentiel de la menace (faible, moyen, élevé, critique)
    /// </summary>
    public string Impact { get; set; } = "medium";

    /// <summary>
    /// Probabilité de la menace (faible, moyenne, élevée)
    /// </summary>
    public string Likelihood { get; set; } = "medium";

    /// <summary>
    /// Risque global (faible, moyen, élevé, critique)
    /// </summary>
    public string Risk { get; set; } = "medium";

    /// <summary>
    /// Recommandations pour atténuer la menace
    /// </summary>
    public List<string> Mitigations { get; set; } = new();

    /// <summary>
    /// Types d'actifs techniques concernés par cette menace
    /// </summary>
    public List<string> ApplicableAssetTypes { get; set; } = new();

    /// <summary>
    /// Propriétés supplémentaires
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = new();
}

/// <summary>
/// Catalogue de menaces pour différents types d'actifs techniques
/// </summary>
public static class ThreatCatalog
{
    /// <summary>
    /// Catalogue de menaces prédéfinies
    /// </summary>
    public static readonly List<Threat> Threats = new()
    {
        // Menaces pour les applications web
        new Threat
        {
            Id = "T001",
            Name = "Injection SQL",
            Description = "Injection de code SQL malveillant via des entrées utilisateur non validées",
            Category = "Injection",
            Impact = "high",
            Likelihood = "high",
            Risk = "high",
            ApplicableAssetTypes = new List<string> { "web-application", "web-service" },
            Mitigations = new List<string>
            {
                "Utiliser des requêtes paramétrées",
                "Valider toutes les entrées utilisateur",
                "Appliquer le principe du moindre privilège pour les comptes de base de données"
            }
        },
        new Threat
        {
            Id = "T002",
            Name = "Cross-Site Scripting (XSS)",
            Description = "Injection de scripts malveillants exécutés dans le navigateur de l'utilisateur",
            Category = "Injection",
            Impact = "medium",
            Likelihood = "high",
            Risk = "medium",
            ApplicableAssetTypes = new List<string> { "web-application" },
            Mitigations = new List<string>
            {
                "Échapper toutes les sorties",
                "Utiliser des en-têtes de sécurité comme Content-Security-Policy",
                "Valider les entrées utilisateur"
            }
        },
        new Threat
        {
            Id = "T003",
            Name = "Authentification cassée",
            Description = "Failles dans le mécanisme d'authentification permettant l'usurpation d'identité",
            Category = "Authentification",
            Impact = "high",
            Likelihood = "medium",
            Risk = "high",
            ApplicableAssetTypes = new List<string> { "web-application", "web-service", "service" },
            Mitigations = new List<string>
            {
                "Mettre en place l'authentification multi-facteurs",
                "Utiliser des mécanismes d'authentification éprouvés",
                "Limiter les tentatives de connexion échouées"
            }
        },

        // Menaces pour les bases de données
        new Threat
        {
            Id = "T004",
            Name = "Fuite de données sensibles",
            Description = "Exposition de données confidentielles stockées sans chiffrement adéquat",
            Category = "Protection des données",
            Impact = "high",
            Likelihood = "medium",
            Risk = "high",
            ApplicableAssetTypes = new List<string> { "database" },
            Mitigations = new List<string>
            {
                "Chiffrer les données sensibles au repos",
                "Mettre en place une gestion des clés de chiffrement",
                "Appliquer le principe du moindre privilège pour l'accès aux données"
            }
        },
        new Threat
        {
            Id = "T005",
            Name = "Sauvegarde insuffisante",
            Description = "Absence ou insuffisance de sauvegardes régulières et testées",
            Category = "Disponibilité",
            Impact = "high",
            Likelihood = "medium",
            Risk = "high",
            ApplicableAssetTypes = new List<string> { "database" },
            Mitigations = new List<string>
            {
                "Mettre en place des sauvegardes régulières et automatisées",
                "Tester régulièrement la restauration des sauvegardes",
                "Stocker les sauvegardes dans des emplacements sécurisés et distincts"
            }
        },

        // Menaces pour les services
        new Threat
        {
            Id = "T006",
            Name = "Déni de service (DoS)",
            Description = "Attaques visant à rendre le service indisponible",
            Category = "Disponibilité",
            Impact = "high",
            Likelihood = "medium",
            Risk = "medium",
            ApplicableAssetTypes = new List<string> { "service", "web-service", "gateway" },
            Mitigations = new List<string>
            {
                "Mettre en place des mécanismes de limitation de débit",
                "Utiliser des services de protection DDoS",
                "Concevoir l'architecture pour la résilience"
            }
        },

        // Menaces pour les passerelles
        new Threat
        {
            Id = "T007",
            Name = "Contournement d'autorisation",
            Description = "Exploitation de failles pour contourner les contrôles d'autorisation",
            Category = "Autorisation",
            Impact = "high",
            Likelihood = "medium",
            Risk = "high",
            ApplicableAssetTypes = new List<string> { "gateway", "web-service" },
            Mitigations = new List<string>
            {
                "Implémenter des contrôles d'accès basés sur les rôles",
                "Valider les autorisations à chaque niveau",
                "Utiliser le principe du moindre privilège"
            }
        },
        
        // Menaces génériques pour tous les actifs
        new Threat
        {
            Id = "T008",
            Name = "Mauvaise gestion des secrets",
            Description = "Stockage ou transmission non sécurisés des secrets (clés API, mots de passe)",
            Category = "Protection des données",
            Impact = "high",
            Likelihood = "medium",
            Risk = "high",
            ApplicableAssetTypes = new List<string> { "web-application", "web-service", "service", "database", "gateway" },
            Mitigations = new List<string>
            {
                "Utiliser un gestionnaire de secrets",
                "Ne jamais coder en dur les secrets dans le code source",
                "Rotation régulière des secrets"
            }
        },
        new Threat
        {
            Id = "T009",
            Name = "Journalisation et surveillance insuffisantes",
            Description = "Absence de journalisation adéquate pour détecter et investiguer les incidents",
            Category = "Détection",
            Impact = "medium",
            Likelihood = "high",
            Risk = "medium",
            ApplicableAssetTypes = new List<string> { "web-application", "web-service", "service", "database", "gateway" },
            Mitigations = new List<string>
            {
                "Mettre en place une journalisation centralisée",
                "Configurer des alertes pour les activités suspectes",
                "Conserver les journaux pendant une période appropriée"
            }
        }
    };

    /// <summary>
    /// Obtient les menaces applicables pour un type d'actif technique donné
    /// </summary>
    /// <param name="assetType">Type d'actif technique</param>
    /// <returns>Liste des menaces applicables</returns>
    public static List<Threat> GetThreatsForAssetType(string assetType)
    {
        return Threats.FindAll(t => t.ApplicableAssetTypes.Contains(assetType));
    }

    /// <summary>
    /// Évalue le niveau de risque en fonction des propriétés de sécurité
    /// </summary>
    /// <param name="threat">La menace à évaluer</param>
    /// <param name="confidentiality">Niveau de confidentialité requis</param>
    /// <param name="integrity">Niveau d'intégrité requis</param>
    /// <param name="availability">Niveau de disponibilité requis</param>
    /// <param name="assetType">Type d'actif technique (optionnel)</param>
    /// <returns>Niveau de risque ajusté</returns>
    public static string EvaluateRisk(Threat threat, string confidentiality, string integrity, string availability, string assetType = "")
    {
        // Convertir les niveaux en valeurs numériques
        var confidentialityValue = ConvertLevelToValue(confidentiality);
        var integrityValue = ConvertLevelToValue(integrity);
        var availabilityValue = ConvertLevelToValue(availability);
        var impactValue = ConvertLevelToValue(threat.Impact);
        var likelihoodValue = ConvertLevelToValue(threat.Likelihood);

        // Facteur de risque par défaut
        double riskFactor = 1.0;

        // Ajuster le facteur de risque en fonction du type d'actif
        switch (assetType.ToLower())
        {
            case "database":
                // Les bases de données ont un risque plus élevé pour les menaces de fuite de données
                if (threat.Category == "Protection des données")
                {
                    riskFactor = 1.5;
                }
                break;
            case "gateway":
                // Les passerelles ont un risque plus élevé pour les menaces de déni de service
                if (threat.Category == "Disponibilité")
                {
                    riskFactor = 1.5;
                }
                break;
            case "web-application":
                // Les applications web ont un risque plus élevé pour les menaces d'injection
                if (threat.Category == "Injection")
                {
                    riskFactor = 1.5;
                }
                break;
            case "service":
                // Les services d'authentification ont un risque plus élevé
                if (threat.Category == "Authentification")
                {
                    riskFactor = 1.3;
                }
                break;
        }

        // Calculer un score de risque basé sur les propriétés de sécurité
        var securityRequirement = Math.Max(Math.Max(confidentialityValue, integrityValue), availabilityValue);
        var riskScore = (impactValue * likelihoodValue * securityRequirement * riskFactor) / 4.0;

        // Convertir le score en niveau de risque
        if (riskScore >= 9) return "critical";
        if (riskScore >= 6) return "high";
        if (riskScore >= 3) return "medium";
        return "low";
    }

    private static int ConvertLevelToValue(string level)
    {
        return level.ToLower() switch
        {
            "critical" => 4,
            "very-high" => 4,
            "high" => 3,
            "medium" => 2,
            "low" => 1,
            _ => 2 // Par défaut, niveau moyen
        };
    }
}

/// <summary>
/// Analyseur de menaces pour un modèle Threagile
/// </summary>
public class ThreatAnalyzer
{
    /// <summary>
    /// Analyse les menaces potentielles pour un modèle Threagile
    /// </summary>
    /// <param name="model">Le modèle Threagile à analyser</param>
    /// <returns>Le modèle enrichi avec des informations sur les menaces</returns>
    public ThreagileModel AnalyzeThreats(ThreagileModel model)
    {
        if (model.TechnicalAssets == null)
            return model;

        foreach (var asset in model.TechnicalAssets)
        {
            // Récupérer les valeurs de confidentialité, intégrité et disponibilité
            asset.Properties.TryGetValue("confidentiality", out var confidentiality);
            asset.Properties.TryGetValue("integrity", out var integrity);
            asset.Properties.TryGetValue("availability", out var availability);

            confidentiality ??= "medium";
            integrity ??= "medium";
            availability ??= "medium";

            // Récupérer les menaces applicables pour ce type d'actif
            var applicableThreats = ThreatCatalog.GetThreatsForAssetType(asset.Type);
            
            // Évaluer le niveau de risque pour chaque menace
            var threats = new List<Dictionary<string, object>>();
            foreach (var threat in applicableThreats)
            {
                var risk = ThreatCatalog.EvaluateRisk(threat, confidentiality, integrity, availability, asset.Type);
                
                var threatInfo = new Dictionary<string, object>
                {
                    ["id"] = threat.Id,
                    ["name"] = threat.Name,
                    ["description"] = threat.Description,
                    ["category"] = threat.Category,
                    ["risk"] = risk,
                    ["mitigations"] = threat.Mitigations
                };
                
                threats.Add(threatInfo);
            }
            
            // Ajouter les menaces aux propriétés de l'actif
            asset.Properties["threats"] = $"{threats.Count} potential threats identified";
            asset.Properties["threatDetails"] = $"See threat analysis for details";
            
            // Stocker les informations détaillées sur les menaces (pourrait être utilisé pour générer un rapport séparé)
            if (!model.Properties.ContainsKey("threatAnalysis"))
            {
                model.Properties["threatAnalysis"] = "Threat analysis performed";
            }
        }

        return model;
    }
} 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ThreagileConverter.Core.Adapters;
using ThreagileConverter.Core.Logging;
using ThreagileConverter.Core.Models;

namespace ThreagileConverter.Core.Mapping;

/// <summary>
/// Implementation of the mapper between DrawIO and Threagile models
/// </summary>
public class Mapper : IMapper
{
    private readonly ILogger<Mapper> _logger;
    private readonly Dictionary<string, string> _typeMappings;
    private readonly Dictionary<string, string> _usageMappings;
    private readonly Dictionary<string, string> _confidentialityMappings;
    private readonly Dictionary<string, string> _integrityMappings;
    private readonly Dictionary<string, string> _availabilityMappings;
    private readonly Dictionary<string, string> _protocolMappings;
    private readonly Dictionary<string, string> _authenticationMappings;
    private readonly Dictionary<string, string> _trustBoundaryMappings;

    public Mapper(ILogger<Mapper> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _typeMappings = InitializeTypeMappings();
        _usageMappings = InitializeUsageMappings();
        _confidentialityMappings = InitializeConfidentialityMappings();
        _integrityMappings = InitializeIntegrityMappings();
        _availabilityMappings = InitializeAvailabilityMappings();
        _protocolMappings = InitializeProtocolMappings();
        _authenticationMappings = InitializeAuthenticationMappings();
        _trustBoundaryMappings = InitializeTrustBoundaryMappings();
    }

    /// <summary>
    /// Maps a DrawIO model to a Threagile model
    /// </summary>
    /// <param name="drawioModel">The DrawIO model to map</param>
    /// <returns>The mapped Threagile model</returns>
    public async Task<ThreagileModel> MapToThreagileAsync(DrawIOModel drawioModel)
    {
        _logger.LogInformation("Mapping DrawIO model to Threagile model");
        var model = new ThreagileModel();

        // Map metadata
        model.Title = drawioModel.Metadata.Title;
        model.Description = drawioModel.Metadata.Description;
        model.Version = drawioModel.Metadata.Version;
        model.Author = drawioModel.Metadata.Author;

        // Create default trust boundary for unassigned assets
        var defaultTrustBoundary = new TrustBoundary
        {
            Id = "default-boundary",
            Name = "Default Boundary",
            Title = "Default Trust Boundary",
            Type = "network-segment",
            Description = "Default trust boundary for unassigned assets",
            TechnicalAssets = new List<string>()
        };
        model.TrustBoundaries.Add(defaultTrustBoundary);

        // Map shapes to technical assets
        foreach (var shape in drawioModel.Shapes)
        {
            var asset = new TechnicalAsset
            {
                Id = shape.Id,
                Name = shape.Value,
                Title = shape.Value,
                Description = shape.Value,
                Type = DetermineAssetType(shape),
                Usage = DetermineAssetUsage(shape),
                Style = ConvertStyle(shape.Style),
                UsedForDataProtection = false,
                UsedForDataRetention = false,
                UsedForDataDestruction = false,
                UsedForDataArchiving = false,
                Properties = new Dictionary<string, string>(shape.Style.Properties)
            };

            // Add default confidentiality, integrity, and availability values
            asset.Properties["confidentiality"] = "medium";
            asset.Properties["integrity"] = "medium";
            asset.Properties["availability"] = "medium";

            model.TechnicalAssets.Add(asset);

            // Add to default trust boundary if no specific boundary is assigned
            defaultTrustBoundary.TechnicalAssets.Add(asset.Id);
        }

        // Map relations to communication links
        foreach (var relation in drawioModel.Relations)
        {
            var link = new CommunicationLink
            {
                Id = relation.Id,
                Name = relation.Value ?? string.Empty,
                Title = relation.Value ?? string.Empty,
                Description = relation.Value ?? string.Empty,
                SourceId = relation.SourceId,
                TargetId = relation.TargetId,
                Source = relation.SourceId,
                Target = relation.TargetId,
                Type = DetermineLinkType(relation),
                Protocol = DetermineLinkProtocol(relation),
                Authentication = DetermineLinkAuthentication(relation),
                Authorization = "none",
                Encryption = "none",
                Properties = new Dictionary<string, string>(relation.Style.Properties)
            };
            model.CommunicationLinks.Add(link);
        }

        // Create data assets for databases
        CreateDataAssetsForDatabases(model);

        return model;
    }

    /// <summary>
    /// Maps a Threagile model to a DrawIO model
    /// </summary>
    /// <param name="threagileModel">The Threagile model to map</param>
    /// <returns>The mapped DrawIO model</returns>
    public async Task<DrawIOModel> MapToDrawIOAsync(ThreagileModel threagileModel)
    {
        _logger.LogInformation("Mapping Threagile model to DrawIO model");
        var model = new DrawIOModel
        {
            Metadata = new DrawIOMetadata
            {
                Title = threagileModel.Title,
                Description = threagileModel.Description,
                Version = threagileModel.Version,
                Author = threagileModel.Author
            },
            Shapes = new List<DrawIOShape>(),
            Relations = new List<DrawIORelation>()
        };

        // Map technical assets to shapes
        foreach (var asset in threagileModel.TechnicalAssets)
        {
            var shape = new DrawIOShape
            {
                Id = asset.Id,
                Value = asset.Name,
                Style = new DrawIOStyle
                {
                    Shape = DetermineShapeType(asset.Type),
                    Properties = new Dictionary<string, string>(asset.Properties)
                }
            };
            model.Shapes.Add(shape);
        }

        // Map communication links to relations
        foreach (var link in threagileModel.CommunicationLinks)
        {
            var relation = new DrawIORelation
            {
                Id = link.Id,
                Value = link.Name,
                SourceId = link.SourceId,
                TargetId = link.TargetId,
                Style = new DrawIOStyle
                {
                    Properties = new Dictionary<string, string>(link.Properties)
                }
            };
            model.Relations.Add(relation);
        }

        return model;
    }

    /// <summary>
    /// Converts a DrawIO style to a Threagile style
    /// </summary>
    /// <param name="style">The DrawIO style to convert</param>
    /// <returns>The converted Threagile style</returns>
    public ThreagileStyle ConvertStyle(DrawIOStyle style)
    {
        return new ThreagileStyle
        {
            FillColor = style.FillColor,
            StrokeColor = style.StrokeColor,
            FontColor = style.FontColor,
            FontStyle = style.FontStyle,
            FontSize = style.FontSize,
            Shape = style.Shape,
            Properties = new Dictionary<string, string>(style.Properties)
        };
    }

    /// <summary>
    /// Validates the types in a Threagile model
    /// </summary>
    /// <param name="model">The model to validate</param>
    /// <returns>True if the validation is successful, false otherwise</returns>
    public bool ValidateTypes(ThreagileModel model)
    {
        _logger.LogInformation("Validating Threagile model types");
        bool isValid = true;

        foreach (var asset in model.TechnicalAssets)
        {
            if (!_typeMappings.ContainsValue(asset.Type))
            {
                _logger.LogWarning("Invalid asset type: {Type} for asset {Id}", asset.Type, asset.Id);
                asset.Type = "web-application"; // Default type
                isValid = false;
            }
        }

        foreach (var link in model.CommunicationLinks)
        {
            if (string.IsNullOrEmpty(link.Type))
            {
                _logger.LogWarning("Missing link type for link {Id}", link.Id);
                link.Type = "restful-api"; // Default type
                isValid = false;
            }
        }

        return isValid;
    }

    /// <summary>
    /// Processes relations in a Threagile model
    /// </summary>
    /// <param name="model">The model to process</param>
    public void ProcessRelations(ThreagileModel model)
    {
        _logger.LogInformation("Processing Threagile model relations");

        // Ensure all source and target references exist
        foreach (var link in model.CommunicationLinks)
        {
            var source = model.TechnicalAssets.FirstOrDefault(a => a.Id == link.SourceId);
            var target = model.TechnicalAssets.FirstOrDefault(a => a.Id == link.TargetId);

            if (source == null)
            {
                _logger.LogWarning("Source asset not found for link {Id}: {SourceId}", link.Id, link.SourceId);
                link.Source = string.Empty;
            }
            else
            {
                link.Source = source.Id;
            }

            if (target == null)
            {
                _logger.LogWarning("Target asset not found for link {Id}: {TargetId}", link.Id, link.TargetId);
                link.Target = string.Empty;
            }
            else
            {
                link.Target = target.Id;
            }
        }
    }

    /// <summary>
    /// Validates constraints in a Threagile model
    /// </summary>
    /// <param name="model">The model to validate</param>
    /// <returns>True if the validation is successful, false otherwise</returns>
    public bool ValidateConstraints(ThreagileModel model)
    {
        _logger.LogInformation("Validating Threagile model constraints");
        bool isValid = true;

        // Ensure all technical assets have valid types
        foreach (var asset in model.TechnicalAssets)
        {
            if (string.IsNullOrEmpty(asset.Type))
            {
                _logger.LogWarning("Missing type for asset {Id}", asset.Id);
                asset.Type = "web-application"; // Default type
                isValid = false;
            }
        }

        // Ensure all communication links have valid source and target
        foreach (var link in model.CommunicationLinks)
        {
            if (string.IsNullOrEmpty(link.Source) || string.IsNullOrEmpty(link.Target))
            {
                _logger.LogWarning("Invalid source or target for link {Id}: Source={Source}, Target={Target}", 
                    link.Id, link.Source, link.Target);
                isValid = false;
            }
        }

        return isValid;
    }

    private string DetermineAssetType(DrawIOShape shape)
    {
        // Check if the shape has a specific type property
        if (shape.Style.Properties.TryGetValue("assetType", out var explicitType))
        {
            return explicitType;
        }

        // Determine type based on shape style
        if (shape.Style.Shape == "cylinder3" || shape.Style.Shape.Contains("cylinder"))
        {
            return "database";
        }
        
        if (shape.Style.Shape == "rhombus" || shape.Value?.Contains("Gateway") == true)
        {
            return "gateway";
        }
        
        if (shape.Value?.Contains("Service") == true)
        {
            return "service";
        }

        if (shape.Value?.Contains("Database") == true || shape.Value?.Contains("DB") == true)
        {
            return "database";
        }

        if (shape.Value?.Contains("API") == true)
        {
            return "web-service";
        }

        if (shape.Value?.Contains("Web") == true || shape.Value?.Contains("Application") == true)
        {
            return "web-application";
        }

        // Default to web-application as a safe choice
        return "web-application";
    }

    private string DetermineAssetUsage(DrawIOShape shape)
    {
        // Check if the shape has a specific usage property
        if (shape.Style.Properties.TryGetValue("usage", out var explicitUsage))
        {
            return explicitUsage;
        }

        // Default to business
        return "business";
    }

    private string DetermineLinkType(DrawIORelation relation)
    {
        // Check if the relation has a specific type property
        if (relation.Style.Properties.TryGetValue("linkType", out var explicitType))
        {
            return explicitType;
        }

        // Default to restful-api
        return "restful-api";
    }

    private string DetermineLinkProtocol(DrawIORelation relation)
    {
        // Check if the relation has a specific protocol property
        if (relation.Style.Properties.TryGetValue("protocol", out var explicitProtocol))
        {
            return explicitProtocol;
        }

        // Default to https
        return "https";
    }

    private string DetermineLinkAuthentication(DrawIORelation relation)
    {
        // Check if the relation has a specific authentication property
        if (relation.Style.Properties.TryGetValue("authentication", out var explicitAuth))
        {
            return explicitAuth;
        }

        // Default to none
        return "none";
    }

    private string DetermineShapeType(string assetType)
    {
        return assetType.ToLower() switch
        {
            "web-application" => "rounded=1",
            "database" => "shape=cylinder3",
            "gateway" => "rhombus",
            "service" => "rounded=0",
            "web-service" => "rounded=1",
            _ => "rounded=1"
        };
    }

    private Dictionary<string, string> InitializeTypeMappings()
    {
        return new Dictionary<string, string>
        {
            { "rectangle", "web-application" },
            { "cylinder", "database" },
            { "rhombus", "gateway" },
            { "hexagon", "service" },
            { "cloud", "external-service" },
            { "actor", "client" }
        };
    }

    private Dictionary<string, string> InitializeUsageMappings()
    {
        return new Dictionary<string, string>
        {
            { "business", "business" },
            { "devops", "devops" },
            { "authentication", "authentication" },
            { "authorization", "authorization" },
            { "logging", "logging" },
            { "monitoring", "monitoring" }
        };
    }

    private Dictionary<string, string> InitializeConfidentialityMappings()
    {
        return new Dictionary<string, string>
        {
            { "low", "low" },
            { "medium", "medium" },
            { "high", "high" },
            { "very-high", "very-high" }
        };
    }

    private Dictionary<string, string> InitializeIntegrityMappings()
    {
        return new Dictionary<string, string>
        {
            { "low", "low" },
            { "medium", "medium" },
            { "high", "high" },
            { "very-high", "very-high" }
        };
    }

    private Dictionary<string, string> InitializeAvailabilityMappings()
    {
        return new Dictionary<string, string>
        {
            { "low", "low" },
            { "medium", "medium" },
            { "high", "high" },
            { "very-high", "very-high" }
        };
    }

    private Dictionary<string, string> InitializeProtocolMappings()
    {
        return new Dictionary<string, string>
        {
            { "http", "http" },
            { "https", "https" },
            { "tcp", "tcp" },
            { "udp", "udp" },
            { "soap", "soap" },
            { "rest", "rest" },
            { "grpc", "grpc" },
            { "graphql", "graphql" }
        };
    }

    private Dictionary<string, string> InitializeAuthenticationMappings()
    {
        return new Dictionary<string, string>
        {
            { "none", "none" },
            { "basic", "basic" },
            { "digest", "digest" },
            { "oauth2", "oauth2" },
            { "jwt", "jwt" },
            { "saml", "saml" },
            { "kerberos", "kerberos" },
            { "certificate", "certificate" }
        };
    }

    private Dictionary<string, string> InitializeTrustBoundaryMappings()
    {
        return new Dictionary<string, string>
        {
            { "network", "network-segment" },
            { "execution", "execution-environment" },
            { "cloud", "cloud-provider" },
            { "physical", "physical-location" }
        };
    }

    private void CreateDataAssetsForDatabases(ThreagileModel model)
    {
        // Create data assets for database technical assets
        foreach (var asset in model.TechnicalAssets.Where(a => a.Type == "database"))
        {
            var dataAsset = new DataAsset
            {
                Id = $"data-{asset.Id}",
                Name = $"Data in {asset.Name}",
                Description = $"Data stored in {asset.Name}",
                Type = "business-data",
                Usage = "business",
                Origin = "internal",
                Owner = "business",
                Quantity = "many",
                Confidentiality = "medium",
                Integrity = "medium",
                Availability = "medium"
            };
            
            model.DataAssets.Add(dataAsset);
        }
    }
}
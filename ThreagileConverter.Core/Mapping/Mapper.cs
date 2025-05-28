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

    public Mapper(ILogger<Mapper> logger)
    {
        _logger = logger;
        _typeMappings = InitializeTypeMappings();
        _usageMappings = InitializeUsageMappings();
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

        // Map shapes to technical assets
        foreach (var shape in drawioModel.Shapes)
        {
            var asset = new TechnicalAsset
            {
                Id = shape.Id,
                Name = shape.Value,
                Description = shape.Value,
                Type = DetermineAssetType(shape.Style),
                Properties = new Dictionary<string, string>(shape.Style.Properties)
            };
            model.TechnicalAssets.Add(asset);
        }

        // Map relations to communication links
        foreach (var relation in drawioModel.Relations)
        {
            var link = new CommunicationLink
            {
                Id = relation.Id,
                Name = relation.Value ?? string.Empty,
                Description = relation.Value ?? string.Empty,
                SourceId = relation.SourceId,
                TargetId = relation.TargetId,
                Type = DetermineLinkType(relation.Style),
                Properties = new Dictionary<string, string>(relation.Style.Properties)
            };
            model.CommunicationLinks.Add(link);
        }

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

    private string DetermineAssetType(DrawIOStyle style)
    {
        return style.Shape?.ToLower() switch
        {
            "rectangle" => "Server",
            "ellipse" => "Database",
            "diamond" => "Gateway",
            "hexagon" => "Service",
            _ => "Unknown"
        };
    }

    private string DetermineLinkType(DrawIOStyle style)
    {
        return style.Properties.GetValueOrDefault("type", "Unknown");
    }

    private string DetermineShapeType(string assetType)
    {
        return assetType.ToLower() switch
        {
            "server" => "rectangle",
            "database" => "ellipse",
            "gateway" => "diamond",
            "service" => "hexagon",
            _ => "rectangle"
        };
    }

    public bool ValidateConstraints(ThreagileModel model)
    {
        _logger.LogInformation("Validating Threagile model constraints");

        bool allValid = true;
        
        // Validate technical assets
        if (model.TechnicalAssets != null)
        {
            foreach (var asset in model.TechnicalAssets)
            {
                ValidateTechnicalAssetConstraints(asset);
                if (string.IsNullOrEmpty(asset.Id) || string.IsNullOrEmpty(asset.Title) || string.IsNullOrEmpty(asset.Type))
                {
                    allValid = false;
                }
                
                // Validate asset type
                if (!IsValidTechnicalAssetType(asset.Type))
                {
                    _logger.LogWarning("Technical asset {Id} has invalid type: {Type}", asset.Id, asset.Type);
                    allValid = false;
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
                allValid = false;
            }
        }
        
        // Validate trust boundaries
        if (model.TrustBoundaries != null)
        {
            foreach (var boundary in model.TrustBoundaries)
            {
                ValidateTrustBoundaryConstraints(boundary);
                if (string.IsNullOrEmpty(boundary.Id) || string.IsNullOrEmpty(boundary.Title) || string.IsNullOrEmpty(boundary.Type))
                {
                    allValid = false;
                }
                
                // Validate boundary type
                if (!IsValidTrustBoundaryType(boundary.Type))
                {
                    _logger.LogWarning("Trust boundary {Id} has invalid type: {Type}", boundary.Id, boundary.Type);
                    allValid = false;
                }
                
                // Validate that referenced technical assets exist
                if (boundary.TechnicalAssets != null)
                {
                    foreach (var assetId in boundary.TechnicalAssets)
                    {
                        if (!model.TechnicalAssets.Any(a => a.Id == assetId))
                        {
                            _logger.LogWarning("Trust boundary {Id} references non-existent technical asset: {AssetId}", boundary.Id, assetId);
                            allValid = false;
                        }
                    }
                }
            }
            
            // Check for duplicate IDs
            var duplicateBoundaryIds = model.TrustBoundaries
                .GroupBy(b => b.Id)
                .Where(g => g.Count() > 1 && !string.IsNullOrEmpty(g.Key))
                .Select(g => g.Key)
                .ToList();
                
            if (duplicateBoundaryIds.Any())
            {
                _logger.LogWarning("Duplicate trust boundary IDs found: {DuplicateIds}", string.Join(", ", duplicateBoundaryIds));
                allValid = false;
            }
        }
        
        // Validate communication links
        if (model.CommunicationLinks != null)
        {
            foreach (var link in model.CommunicationLinks)
            {
                ValidateCommunicationLinkConstraints(link, model);
                if (string.IsNullOrEmpty(link.Id) || string.IsNullOrEmpty(link.SourceId) || string.IsNullOrEmpty(link.TargetId))
                {
                    allValid = false;
                }
            }
            
            // Check for duplicate IDs
            var duplicateLinkIds = model.CommunicationLinks
                .GroupBy(l => l.Id)
                .Where(g => g.Count() > 1 && !string.IsNullOrEmpty(g.Key))
                .Select(g => g.Key)
                .ToList();
                
            if (duplicateLinkIds.Any())
            {
                _logger.LogWarning("Duplicate communication link IDs found: {DuplicateIds}", string.Join(", ", duplicateLinkIds));
                allValid = false;
            }
            
            // Check for duplicate source-target pairs
            var duplicateLinks = model.CommunicationLinks
                .GroupBy(l => new { l.SourceId, l.TargetId })
                .Where(g => g.Count() > 1 && !string.IsNullOrEmpty(g.Key.SourceId) && !string.IsNullOrEmpty(g.Key.TargetId))
                .Select(g => g.Key)
                .ToList();
                
            if (duplicateLinks.Any())
            {
                foreach (var dupLink in duplicateLinks)
                {
                    _logger.LogWarning("Duplicate links found between source {Source} and target {Target}", dupLink.SourceId, dupLink.TargetId);
                }
                allValid = false;
            }
        }
        
        // Validate shared runtimes
        if (model.SharedRuntimes != null)
        {
            foreach (var runtime in model.SharedRuntimes)
            {
                if (string.IsNullOrEmpty(runtime.Id))
                {
                    _logger.LogWarning("Shared runtime has empty ID");
                    allValid = false;
                }
                
                if (string.IsNullOrEmpty(runtime.Name))
                {
                    _logger.LogWarning("Shared runtime {Id} has empty name", runtime.Id);
                    allValid = false;
                }
            }
            
            // Check for duplicate IDs
            var duplicateRuntimeIds = model.SharedRuntimes
                .GroupBy(r => r.Id)
                .Where(g => g.Count() > 1 && !string.IsNullOrEmpty(g.Key))
                .Select(g => g.Key)
                .ToList();
                
            if (duplicateRuntimeIds.Any())
            {
                _logger.LogWarning("Duplicate shared runtime IDs found: {DuplicateIds}", string.Join(", ", duplicateRuntimeIds));
                allValid = false;
            }
        }
        
        // Validate data assets
        if (model.DataAssets != null)
        {
            foreach (var asset in model.DataAssets)
            {
                if (string.IsNullOrEmpty(asset.Id))
                {
                    _logger.LogWarning("Data asset has empty ID");
                    allValid = false;
                }
                
                if (string.IsNullOrEmpty(asset.Name))
                {
                    _logger.LogWarning("Data asset {Id} has empty name", asset.Id);
                    allValid = false;
                }
                
                if (string.IsNullOrEmpty(asset.Type))
                {
                    _logger.LogWarning("Data asset {Id} has empty type", asset.Id);
                    allValid = false;
                }
            }
            
            // Check for duplicate IDs
            var duplicateDataAssetIds = model.DataAssets
                .GroupBy(d => d.Id)
                .Where(g => g.Count() > 1 && !string.IsNullOrEmpty(g.Key))
                .Select(g => g.Key)
                .ToList();
                
            if (duplicateDataAssetIds.Any())
            {
                _logger.LogWarning("Duplicate data asset IDs found: {DuplicateIds}", string.Join(", ", duplicateDataAssetIds));
                allValid = false;
            }
        }
        
        return allValid;
    }

    public void ProcessRelations(ThreagileModel model)
    {
        _logger.LogInformation("Processing relations in Threagile model");

        // Process technical asset relations
        foreach (var asset in model.TechnicalAssets)
        {
            // Find all communication links where this asset is source or target
            var relatedLinks = model.CommunicationLinks
                .Where(l => l.Source == asset.Id || l.Target == asset.Id)
                .ToList();

            // Update asset properties based on relations
            foreach (var link in relatedLinks)
            {
                UpdateAssetPropertiesFromLink(asset, link);
            }
        }

        // Process trust boundary relations
        foreach (var boundary in model.TrustBoundaries)
        {
            // Validate that all referenced technical assets exist
            boundary.TechnicalAssets = boundary.TechnicalAssets
                .Where(id => model.TechnicalAssets.Any(a => a.Id == id))
                .ToList();
        }
    }
    
    private void UpdateAssetPropertiesFromLink(TechnicalAsset asset, CommunicationLink link)
    {
        if (link.Source == asset.Id)
        {
            asset.Properties["outgoingProtocol"] = link.Protocol;
            asset.Properties["outgoingAuthentication"] = link.Authentication;
            asset.Properties["outgoingAuthorization"] = link.Authorization;
            asset.Properties["outgoingEncryption"] = link.Encryption;
        }
        else if (link.Target == asset.Id)
        {
            asset.Properties["incomingProtocol"] = link.Protocol;
            asset.Properties["incomingAuthentication"] = link.Authentication;
            asset.Properties["incomingAuthorization"] = link.Authorization;
            asset.Properties["incomingEncryption"] = link.Encryption;
        }
    }

    private bool IsValidProtocol(string protocol)
    {
        var validProtocols = new[] 
        { 
            "HTTP", "HTTPS", "FTP", "FTPS", "SFTP", "SSH", "Telnet", 
            "SMTP", "SMTPS", "POP3", "IMAP", "DNS", "LDAP", "SNMP", 
            "RDP", "SMB", "NFS", "WebSocket", "gRPC", "MQTT", "AMQP", 
            "Kafka", "RabbitMQ", "JMS", "REST", "SOAP", "GraphQL", 
            "TCP", "UDP", "ICMP", "SCTP", "Unknown" 
        };
        return validProtocols.Contains(protocol, StringComparer.OrdinalIgnoreCase);
    }
    
    private bool IsValidAuthentication(string authentication)
    {
        var validAuthentications = new[] 
        { 
            "None", "Basic", "Digest", "NTLM", "Kerberos", "Certificate", 
            "JWT", "OAuth", "SAML", "OpenID Connect", "API Key", 
            "LDAP", "Windows", "Forms", "Biometric", "MFA", "2FA", 
            "SSO", "Unknown" 
        };
        return validAuthentications.Contains(authentication, StringComparer.OrdinalIgnoreCase);
    }
    
    private bool IsValidAuthorization(string authorization)
    {
        var validAuthorizations = new[] 
        { 
            "None", "Role-based", "Attribute-based", "Policy-based", 
            "Rule-based", "ACL", "RBAC", "ABAC", "MAC", "DAC", 
            "Claims-based", "OAuth Scopes", "Unknown" 
        };
        return validAuthorizations.Contains(authorization, StringComparer.OrdinalIgnoreCase);
    }
    
    private bool IsValidEncryption(string encryption)
    {
        var validEncryptions = new[] 
        { 
            "None", "TLS", "SSL", "SSH", "IPsec", "WPA", "WPA2", "WPA3", 
            "AES", "RSA", "ECC", "ChaCha20", "Blowfish", "3DES", "DES", 
            "RC4", "End-to-End", "Unknown" 
        };
        return validEncryptions.Contains(encryption, StringComparer.OrdinalIgnoreCase);
    }
    
    private bool IsValidDataAssetType(string type)
    {
        var validTypes = new[] 
        { 
            "Source Code", "Configuration", "Credentials", "PII", "Financial", 
            "Health", "Authentication Data", "Authorization Data", "Communications", 
            "Business Data", "Logs", "Operational", "Unknown" 
        };
        return validTypes.Contains(type, StringComparer.OrdinalIgnoreCase);
    }
    
    private bool IsValidConfidentialityLevel(string level)
    {
        var validLevels = new[] { "Public", "Internal", "Restricted", "Confidential", "Strictly Confidential", "Unknown" };
        return validLevels.Contains(level, StringComparer.OrdinalIgnoreCase);
    }
    
    private bool IsValidIntegrityLevel(string level)
    {
        var validLevels = new[] { "Low", "Medium", "High", "Critical", "Unknown" };
        return validLevels.Contains(level, StringComparer.OrdinalIgnoreCase);
    }
    
    private bool IsValidAvailabilityLevel(string level)
    {
        var validLevels = new[] { "Low", "Medium", "High", "Critical", "Unknown" };
        return validLevels.Contains(level, StringComparer.OrdinalIgnoreCase);
    }

    private bool IsValidTechnicalAssetType(string type)
    {
        return _typeMappings.Values.Contains(type);
    }

    private bool IsValidTrustBoundaryType(string type)
    {
        return new[] { "Network", "Execution", "Data", "Unknown" }.Contains(type);
    }

    private void ValidateTechnicalAssetConstraints(TechnicalAsset asset)
    {
        if (string.IsNullOrEmpty(asset.Id))
        {
            _logger.LogWarning("Technical asset has empty ID");
        }

        if (string.IsNullOrEmpty(asset.Title))
        {
            _logger.LogWarning("Technical asset {Id} has empty title", asset.Id);
        }

        if (string.IsNullOrEmpty(asset.Type))
        {
            _logger.LogWarning("Technical asset {Id} has empty type", asset.Id);
        }
    }

    private void ValidateTrustBoundaryConstraints(TrustBoundary boundary)
    {
        if (string.IsNullOrEmpty(boundary.Id))
        {
            _logger.LogWarning("Trust boundary has empty ID");
        }

        if (string.IsNullOrEmpty(boundary.Title))
        {
            _logger.LogWarning("Trust boundary {Id} has empty title", boundary.Id);
        }

        if (string.IsNullOrEmpty(boundary.Type))
        {
            _logger.LogWarning("Trust boundary {Id} has empty type", boundary.Id);
        }
    }

    private void ValidateCommunicationLinkConstraints(CommunicationLink link, ThreagileModel model)
    {
        if (string.IsNullOrEmpty(link.Id))
        {
            _logger.LogWarning("Communication link has empty ID");
        }

        if (string.IsNullOrEmpty(link.Source))
        {
            _logger.LogWarning("Communication link {Id} has empty source", link.Id);
        }

        if (string.IsNullOrEmpty(link.Target))
        {
            _logger.LogWarning("Communication link {Id} has empty target", link.Id);
        }

        // Validate that source and target exist
        if (!model.TechnicalAssets.Any(a => a.Id == link.Source))
        {
            _logger.LogWarning("Communication link {Id} references non-existent source {Source}", link.Id, link.Source);
        }

        if (!model.TechnicalAssets.Any(a => a.Id == link.Target))
        {
            _logger.LogWarning("Communication link {Id} references non-existent target {Target}", link.Id, link.Target);
        }
    }

    public bool ValidateTypes(ThreagileModel model)
    {
        _logger.LogInformation("Validating Threagile model types");

        bool allValid = true;
        
        // Validate technical assets
        if (model.TechnicalAssets != null)
        {
            foreach (var asset in model.TechnicalAssets)
            {
                // Check if type is empty
                if (string.IsNullOrEmpty(asset.Type))
                {
                    _logger.LogWarning("Technical asset {Id} has empty type", asset.Id);
                    allValid = false;
                }
                // Check if type is valid
                else if (!IsValidTechnicalAssetType(asset.Type))
                {
                    _logger.LogWarning("Technical asset {Id} has invalid type: {Type}", asset.Id, asset.Type);
                    allValid = false;
                }
                
                // Validate usage
                if (!string.IsNullOrEmpty(asset.Usage) && !_usageMappings.ContainsValue(asset.Usage))
                {
                    _logger.LogWarning("Technical asset {Id} has invalid usage: {Usage}", asset.Id, asset.Usage);
                    allValid = false;
                }
            }
        }
        
        // Validate trust boundaries
        if (model.TrustBoundaries != null)
        {
            foreach (var boundary in model.TrustBoundaries)
            {
                // Check if type is empty
                if (string.IsNullOrEmpty(boundary.Type))
                {
                    _logger.LogWarning("Trust boundary {Id} has empty type", boundary.Id);
                    allValid = false;
                }
                // Check if type is valid
                else if (!IsValidTrustBoundaryType(boundary.Type))
                {
                    _logger.LogWarning("Trust boundary {Id} has invalid type: {Type}", boundary.Id, boundary.Type);
                    allValid = false;
                }
            }
        }
        
        // Validate communication links
        if (model.CommunicationLinks != null)
        {
            foreach (var link in model.CommunicationLinks)
            {
                // Validate protocol
                if (!string.IsNullOrEmpty(link.Protocol) && !IsValidProtocol(link.Protocol))
                {
                    _logger.LogWarning("Communication link {Id} has invalid protocol: {Protocol}", link.Id, link.Protocol);
                    allValid = false;
                }
                
                // Validate authentication
                if (!string.IsNullOrEmpty(link.Authentication) && !IsValidAuthentication(link.Authentication))
                {
                    _logger.LogWarning("Communication link {Id} has invalid authentication: {Authentication}", link.Id, link.Authentication);
                    allValid = false;
                }
                
                // Validate authorization
                if (!string.IsNullOrEmpty(link.Authorization) && !IsValidAuthorization(link.Authorization))
                {
                    _logger.LogWarning("Communication link {Id} has invalid authorization: {Authorization}", link.Id, link.Authorization);
                    allValid = false;
                }
                
                // Validate encryption
                if (!string.IsNullOrEmpty(link.Encryption) && !IsValidEncryption(link.Encryption))
                {
                    _logger.LogWarning("Communication link {Id} has invalid encryption: {Encryption}", link.Id, link.Encryption);
                    allValid = false;
                }
            }
        }
        
        // Validate data assets
        if (model.DataAssets != null)
        {
            foreach (var asset in model.DataAssets)
            {
                // Check if type is empty
                if (string.IsNullOrEmpty(asset.Type))
                {
                    _logger.LogWarning("Data asset {Id} has empty type", asset.Id);
                    allValid = false;
                }
                // Check if type is valid
                else if (!IsValidDataAssetType(asset.Type))
                {
                    _logger.LogWarning("Data asset {Id} has invalid type: {Type}", asset.Id, asset.Type);
                    allValid = false;
                }
                
                // Validate confidentiality
                if (!string.IsNullOrEmpty(asset.Confidentiality) && !IsValidConfidentialityLevel(asset.Confidentiality))
                {
                    _logger.LogWarning("Data asset {Id} has invalid confidentiality level: {Level}", asset.Id, asset.Confidentiality);
                    allValid = false;
                }
                
                // Validate integrity
                if (!string.IsNullOrEmpty(asset.Integrity) && !IsValidIntegrityLevel(asset.Integrity))
                {
                    _logger.LogWarning("Data asset {Id} has invalid integrity level: {Level}", asset.Id, asset.Integrity);
                    allValid = false;
                }
                
                // Validate availability
                if (!string.IsNullOrEmpty(asset.Availability) && !IsValidAvailabilityLevel(asset.Availability))
                {
                    _logger.LogWarning("Data asset {Id} has invalid availability level: {Level}", asset.Id, asset.Availability);
                    allValid = false;
                }
            }
        }
        
        return allValid;
    }

    private Dictionary<string, string> InitializeTypeMappings()
    {
        return new Dictionary<string, string>
        {
            { "process", "Process" },
            { "datastore", "DataStore" },
            { "external-entity", "ExternalEntity" },
            { "trust-boundary", "TrustBoundary" },
            { "shared-runtime", "SharedRuntime" },
            { "web-application", "WebApplication" },
            { "mobile-application", "MobileApplication" },
            { "desktop-application", "DesktopApplication" },
            { "service", "Service" },
            { "database", "Database" },
            { "file-storage", "FileStorage" },
            { "message-queue", "MessageQueue" },
            { "load-balancer", "LoadBalancer" },
            { "firewall", "Firewall" },
            { "vpn", "VPN" },
            { "router", "Router" },
            { "switch", "Switch" },
            { "gateway", "Gateway" },
            { "proxy", "Proxy" },
            { "cache", "Cache" },
            { "cdn", "CDN" },
            { "dns", "DNS" },
            { "dhcp", "DHCP" },
            { "ntp", "NTP" },
            { "ldap", "LDAP" },
            { "kerberos", "Kerberos" },
            { "saml", "SAML" },
            { "oauth", "OAuth" },
            { "openid", "OpenID" },
            { "jwt", "JWT" }
        };
    }

    private Dictionary<string, string> InitializeUsageMappings()
    {
        return new Dictionary<string, string>
        {
            { "business", "Business" },
            { "devops", "DevOps" },
            { "security", "Security" },
            { "monitoring", "Monitoring" },
            { "logging", "Logging" },
            { "backup", "Backup" },
            { "recovery", "Recovery" },
            { "disaster-recovery", "DisasterRecovery" },
            { "business-continuity", "BusinessContinuity" },
            { "compliance", "Compliance" },
            { "privacy", "Privacy" },
            { "data-protection", "DataProtection" },
            { "data-retention", "DataRetention" },
            { "data-destruction", "DataDestruction" },
            { "data-archiving", "DataArchiving" }
        };
    }
} 
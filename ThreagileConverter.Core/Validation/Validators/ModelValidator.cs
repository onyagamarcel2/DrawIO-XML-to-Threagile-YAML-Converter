using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThreagileConverter.Core.Models;

namespace ThreagileConverter.Core.Validation.Validators;

public class ModelValidator : IModelValidator
{
    public async Task<ValidationResult> ValidateAsync(ThreagileModel model)
    {
        var errors = new List<ValidationError>();

        // Validate metadata
        if (string.IsNullOrEmpty(model.Title))
        {
            errors.Add(new ValidationError("Title is required", ValidationSeverity.Error));
        }

        // Validate technical assets
        if (model.TechnicalAssets == null || !model.TechnicalAssets.Any())
        {
            errors.Add(new ValidationError("At least one technical asset is required", ValidationSeverity.Error));
        }
        else
        {
            foreach (var asset in model.TechnicalAssets)
            {
                if (string.IsNullOrEmpty(asset.Id))
                {
                    errors.Add(new ValidationError($"Technical asset {asset.Name} has no ID", ValidationSeverity.Error));
                }
                if (string.IsNullOrEmpty(asset.Name))
                {
                    errors.Add(new ValidationError($"Technical asset {asset.Id} has no name", ValidationSeverity.Error));
                }
                if (string.IsNullOrEmpty(asset.Type))
                {
                    errors.Add(new ValidationError($"Technical asset {asset.Name} has no type", ValidationSeverity.Error));
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
                    errors.Add(new ValidationError($"Communication link has no ID", ValidationSeverity.Error));
                }
                if (string.IsNullOrEmpty(link.SourceId))
                {
                    errors.Add(new ValidationError($"Communication link {link.Id} has no source", ValidationSeverity.Error));
                }
                if (string.IsNullOrEmpty(link.TargetId))
                {
                    errors.Add(new ValidationError($"Communication link {link.Id} has no target", ValidationSeverity.Error));
                }
                if (string.IsNullOrEmpty(link.Protocol))
                {
                    errors.Add(new ValidationError($"Communication link {link.Id} has no protocol", ValidationSeverity.Warning));
                }
            }
        }

        // Validate trust boundaries
        if (model.TrustBoundaries != null)
        {
            foreach (var boundary in model.TrustBoundaries)
            {
                if (string.IsNullOrEmpty(boundary.Id))
                {
                    errors.Add(new ValidationError($"Trust boundary has no ID", ValidationSeverity.Error));
                }
                if (string.IsNullOrEmpty(boundary.Name))
                {
                    errors.Add(new ValidationError($"Trust boundary {boundary.Id} has no name", ValidationSeverity.Error));
                }
                if (boundary.TechnicalAssets == null || !boundary.TechnicalAssets.Any())
                {
                    errors.Add(new ValidationError($"Trust boundary {boundary.Name} has no technical assets", ValidationSeverity.Warning));
                }
            }
        }

        if (errors.Any())
            return ValidationResult.CreateFailure(errors);
        else
            return ValidationResult.Success();
    }
} 
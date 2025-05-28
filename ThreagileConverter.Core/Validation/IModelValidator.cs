using System.Threading.Tasks;
using ThreagileConverter.Core.Models;

namespace ThreagileConverter.Core.Validation;

public interface IModelValidator
{
    /// <summary>
    /// Validates a Threagile model
    /// </summary>
    /// <param name="model">The model to validate</param>
    /// <returns>A validation result containing any errors or warnings</returns>
    Task<ValidationResult> ValidateAsync(ThreagileModel model);
} 
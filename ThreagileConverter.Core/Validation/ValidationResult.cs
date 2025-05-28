using System;
using System.Collections.Generic;
using System.Linq;

namespace ThreagileConverter.Core.Validation
{
    /// <summary>
    /// Represents the result of a validation operation
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets whether the validation was successful
        /// </summary>
        public bool IsValid => !Errors.Any();

        /// <summary>
        /// Gets the list of validation errors
        /// </summary>
        public IReadOnlyList<ValidationError> Errors { get; }

        /// <summary>
        /// Gets the list of validation warnings
        /// </summary>
        public IReadOnlyList<ValidationWarning> Warnings { get; }

        private ValidationResult(IReadOnlyList<ValidationError> errors, IReadOnlyList<ValidationWarning> warnings)
        {
            Errors = errors ?? Array.Empty<ValidationError>();
            Warnings = warnings ?? Array.Empty<ValidationWarning>();
        }

        /// <summary>
        /// Creates a successful validation result
        /// </summary>
        public static ValidationResult Success() => new(Array.Empty<ValidationError>(), Array.Empty<ValidationWarning>());

        /// <summary>
        /// Creates a validation result with warnings
        /// </summary>
        public static ValidationResult WithWarnings(IEnumerable<ValidationWarning> warnings) =>
            new(Array.Empty<ValidationError>(), warnings.ToList());

        /// <summary>
        /// Creates a failed validation result
        /// </summary>
        public static ValidationResult CreateFailure(IEnumerable<ValidationError> errors) =>
            new(errors.ToList(), Array.Empty<ValidationWarning>());

        /// <summary>
        /// Creates a failed validation result with warnings
        /// </summary>
        public static ValidationResult CreateFailureWithWarnings(IEnumerable<ValidationError> errors, IEnumerable<ValidationWarning> warnings) =>
            new(errors.ToList(), warnings.ToList());
    }

    /// <summary>
    /// Represents a validation warning
    /// </summary>
    public class ValidationWarning
    {
        /// <summary>
        /// Gets the warning message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the warning code
        /// </summary>
        public string? Code { get; }

        /// <summary>
        /// Gets the path of the element concerned
        /// </summary>
        public string? Path { get; }

        /// <summary>
        /// Creates a new instance of ValidationWarning
        /// </summary>
        /// <param name="message">Warning message</param>
        /// <param name="code">Warning code</param>
        /// <param name="path">Path of the element concerned</param>
        public ValidationWarning(string message, string? code = null, string? path = null)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Code = code;
            Path = path;
        }
    }
} 
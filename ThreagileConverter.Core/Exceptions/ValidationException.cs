using System;
using System.Collections.Generic;

namespace ThreagileConverter.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a validation error occurs
    /// </summary>
    public class ValidationException : ThreagileConverterException
    {
        /// <summary>
        /// Gets the validation error type
        /// </summary>
        public ValidationErrorType ErrorType { get; }

        /// <summary>
        /// Gets the validation errors
        /// </summary>
        public IReadOnlyList<ValidationError> ValidationErrors { get; }

        /// <summary>
        /// Creates a new instance of ValidationException
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="errorType">The validation error type</param>
        public ValidationException(string message, ValidationErrorType errorType)
            : base(message, $"VALIDATION_{errorType}")
        {
            ErrorType = errorType;
            ValidationErrors = new List<ValidationError>();
        }

        /// <summary>
        /// Creates a new instance of ValidationException
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="errorType">The validation error type</param>
        /// <param name="validationErrors">The validation errors</param>
        public ValidationException(string message, ValidationErrorType errorType, IEnumerable<ValidationError> validationErrors)
            : base(message, $"VALIDATION_{errorType}")
        {
            ErrorType = errorType;
            ValidationErrors = new List<ValidationError>(validationErrors);
        }

        /// <summary>
        /// Creates a new instance of ValidationException
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="errorType">The validation error type</param>
        /// <param name="innerException">The inner exception</param>
        public ValidationException(string message, ValidationErrorType errorType, Exception innerException)
            : base(message, $"VALIDATION_{errorType}", innerException)
        {
            ErrorType = errorType;
            ValidationErrors = new List<ValidationError>();
        }

        /// <summary>
        /// Creates a new instance of ValidationException
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="errorType">The validation error type</param>
        /// <param name="validationErrors">The validation errors</param>
        /// <param name="innerException">The inner exception</param>
        public ValidationException(string message, ValidationErrorType errorType, IEnumerable<ValidationError> validationErrors, Exception innerException)
            : base(message, $"VALIDATION_{errorType}", innerException)
        {
            ErrorType = errorType;
            ValidationErrors = new List<ValidationError>(validationErrors);
        }
    }
} 
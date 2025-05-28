using System;

namespace ThreagileConverter.Core.Exceptions
{
    /// <summary>
    /// Represents a validation error
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Gets the error message
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the property name that caused the error
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the error code
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// Gets the severity level
        /// </summary>
        public ErrorSeverity Severity { get; }

        /// <summary>
        /// Creates a new instance of ValidationError
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="propertyName">The property name</param>
        public ValidationError(string message, string propertyName)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            PropertyName = propertyName;
            ErrorCode = "VALIDATION_ERROR";
            Severity = ErrorSeverity.Error;
        }

        /// <summary>
        /// Creates a new instance of ValidationError
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="propertyName">The property name</param>
        /// <param name="errorCode">The error code</param>
        public ValidationError(string message, string propertyName, string errorCode)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            PropertyName = propertyName;
            ErrorCode = errorCode ?? "VALIDATION_ERROR";
            Severity = ErrorSeverity.Error;
        }

        /// <summary>
        /// Creates a new instance of ValidationError
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="propertyName">The property name</param>
        /// <param name="errorCode">The error code</param>
        /// <param name="severity">The severity level</param>
        public ValidationError(string message, string propertyName, string errorCode, ErrorSeverity severity)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            PropertyName = propertyName;
            ErrorCode = errorCode ?? "VALIDATION_ERROR";
            Severity = severity;
        }

        /// <summary>
        /// Returns a string representation of the validation error
        /// </summary>
        /// <returns>A string representation of the validation error</returns>
        public override string ToString()
        {
            return $"[{Severity}] {ErrorCode}: {PropertyName} - {Message}";
        }
    }
} 
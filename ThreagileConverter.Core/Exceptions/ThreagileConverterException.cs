using System;

namespace ThreagileConverter.Core.Exceptions
{
    /// <summary>
    /// Base exception class for all exceptions in the ThreagileConverter project
    /// </summary>
    public class ThreagileConverterException : Exception
    {
        /// <summary>
        /// Gets the error code associated with this exception
        /// </summary>
        public string ErrorCode { get; }

        /// <summary>
        /// Gets the severity level of this exception
        /// </summary>
        public ErrorSeverity Severity { get; }

        /// <summary>
        /// Creates a new instance of ThreagileConverterException
        /// </summary>
        /// <param name="message">The error message</param>
        public ThreagileConverterException(string message) 
            : base(message)
        {
            ErrorCode = "GENERAL_ERROR";
            Severity = ErrorSeverity.Error;
        }

        /// <summary>
        /// Creates a new instance of ThreagileConverterException
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="errorCode">The error code</param>
        public ThreagileConverterException(string message, string errorCode) 
            : base(message)
        {
            ErrorCode = errorCode;
            Severity = ErrorSeverity.Error;
        }

        /// <summary>
        /// Creates a new instance of ThreagileConverterException
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="errorCode">The error code</param>
        /// <param name="severity">The severity level</param>
        public ThreagileConverterException(string message, string errorCode, ErrorSeverity severity) 
            : base(message)
        {
            ErrorCode = errorCode;
            Severity = severity;
        }

        /// <summary>
        /// Creates a new instance of ThreagileConverterException
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public ThreagileConverterException(string message, Exception innerException) 
            : base(message, innerException)
        {
            ErrorCode = "GENERAL_ERROR";
            Severity = ErrorSeverity.Error;
        }

        /// <summary>
        /// Creates a new instance of ThreagileConverterException
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="errorCode">The error code</param>
        /// <param name="innerException">The inner exception</param>
        public ThreagileConverterException(string message, string errorCode, Exception innerException) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            Severity = ErrorSeverity.Error;
        }

        /// <summary>
        /// Creates a new instance of ThreagileConverterException
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="errorCode">The error code</param>
        /// <param name="severity">The severity level</param>
        /// <param name="innerException">The inner exception</param>
        public ThreagileConverterException(string message, string errorCode, ErrorSeverity severity, Exception innerException) 
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            Severity = severity;
        }
    }
} 
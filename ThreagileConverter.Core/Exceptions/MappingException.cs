using System;

namespace ThreagileConverter.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a mapping error occurs
    /// </summary>
    public class MappingException : ThreagileConverterException
    {
        /// <summary>
        /// Gets the mapping error type
        /// </summary>
        public MappingErrorType ErrorType { get; }

        /// <summary>
        /// Gets the source object ID that caused the mapping error
        /// </summary>
        public string SourceId { get; }

        /// <summary>
        /// Creates a new instance of MappingException
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="errorType">The mapping error type</param>
        public MappingException(string message, MappingErrorType errorType)
            : base(message, $"MAPPING_{errorType}")
        {
            ErrorType = errorType;
        }

        /// <summary>
        /// Creates a new instance of MappingException
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="errorType">The mapping error type</param>
        /// <param name="sourceId">The source object ID</param>
        public MappingException(string message, MappingErrorType errorType, string sourceId)
            : base(message, $"MAPPING_{errorType}")
        {
            ErrorType = errorType;
            SourceId = sourceId;
        }

        /// <summary>
        /// Creates a new instance of MappingException
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="errorType">The mapping error type</param>
        /// <param name="sourceId">The source object ID</param>
        /// <param name="innerException">The inner exception</param>
        public MappingException(string message, MappingErrorType errorType, string sourceId, Exception innerException)
            : base(message, $"MAPPING_{errorType}", innerException)
        {
            ErrorType = errorType;
            SourceId = sourceId;
        }

        /// <summary>
        /// Creates a new instance of MappingException
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="errorType">The mapping error type</param>
        /// <param name="innerException">The inner exception</param>
        public MappingException(string message, MappingErrorType errorType, Exception innerException)
            : base(message, $"MAPPING_{errorType}", innerException)
        {
            ErrorType = errorType;
        }
    }
} 
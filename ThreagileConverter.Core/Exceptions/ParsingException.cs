using System;

namespace ThreagileConverter.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a parsing error occurs
    /// </summary>
    public class ParsingException : ThreagileConverterException
    {
        /// <summary>
        /// Gets the source file path that caused the parsing error
        /// </summary>
        public string? FilePath { get; }

        /// <summary>
        /// Gets the parsing error type
        /// </summary>
        public ParsingErrorType ErrorType { get; }

        /// <summary>
        /// Creates a new instance of ParsingException
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="errorType">The parsing error type</param>
        public ParsingException(string message, ParsingErrorType errorType)
            : base(message, $"PARSING_{errorType}")
        {
            ErrorType = errorType;
        }

        /// <summary>
        /// Creates a new instance of ParsingException
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="errorType">The parsing error type</param>
        /// <param name="filePath">The source file path</param>
        public ParsingException(string message, ParsingErrorType errorType, string filePath)
            : base(message, $"PARSING_{errorType}")
        {
            ErrorType = errorType;
            FilePath = filePath;
        }

        /// <summary>
        /// Creates a new instance of ParsingException
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="errorType">The parsing error type</param>
        /// <param name="filePath">The source file path</param>
        /// <param name="innerException">The inner exception</param>
        public ParsingException(string message, ParsingErrorType errorType, string filePath, Exception innerException)
            : base(message, $"PARSING_{errorType}", innerException)
        {
            ErrorType = errorType;
            FilePath = filePath;
        }

        /// <summary>
        /// Creates a new instance of ParsingException
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="errorType">The parsing error type</param>
        /// <param name="innerException">The inner exception</param>
        public ParsingException(string message, ParsingErrorType errorType, Exception innerException)
            : base(message, $"PARSING_{errorType}", innerException)
        {
            ErrorType = errorType;
        }
    }
} 
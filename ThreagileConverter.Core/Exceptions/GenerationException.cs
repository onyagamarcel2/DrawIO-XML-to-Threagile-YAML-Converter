using System;

namespace ThreagileConverter.Core.Exceptions
{
    /// <summary>
    /// Exception thrown when a generation error occurs
    /// </summary>
    public class GenerationException : ThreagileConverterException
    {
        /// <summary>
        /// Gets the output file path that caused the generation error
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Gets the generation error type
        /// </summary>
        public GenerationErrorType ErrorType { get; }

        /// <summary>
        /// Creates a new instance of GenerationException
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="errorType">The generation error type</param>
        public GenerationException(string message, GenerationErrorType errorType)
            : base(message, $"GENERATION_{errorType}")
        {
            ErrorType = errorType;
        }

        /// <summary>
        /// Creates a new instance of GenerationException
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="errorType">The generation error type</param>
        /// <param name="filePath">The output file path</param>
        public GenerationException(string message, GenerationErrorType errorType, string filePath)
            : base(message, $"GENERATION_{errorType}")
        {
            ErrorType = errorType;
            FilePath = filePath;
        }

        /// <summary>
        /// Creates a new instance of GenerationException
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="errorType">The generation error type</param>
        /// <param name="filePath">The output file path</param>
        /// <param name="innerException">The inner exception</param>
        public GenerationException(string message, GenerationErrorType errorType, string filePath, Exception innerException)
            : base(message, $"GENERATION_{errorType}", innerException)
        {
            ErrorType = errorType;
            FilePath = filePath;
        }

        /// <summary>
        /// Creates a new instance of GenerationException
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="errorType">The generation error type</param>
        /// <param name="innerException">The inner exception</param>
        public GenerationException(string message, GenerationErrorType errorType, Exception innerException)
            : base(message, $"GENERATION_{errorType}", innerException)
        {
            ErrorType = errorType;
        }
    }
} 
namespace ThreagileConverter.Core.Exceptions
{
    /// <summary>
    /// Represents the type of parsing error
    /// </summary>
    public enum ParsingErrorType
    {
        /// <summary>
        /// Unknown parsing error
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// File not found
        /// </summary>
        FileNotFound = 1,

        /// <summary>
        /// File access denied
        /// </summary>
        AccessDenied = 2,

        /// <summary>
        /// Empty file
        /// </summary>
        EmptyFile = 3,

        /// <summary>
        /// Invalid XML format
        /// </summary>
        InvalidXml = 4,

        /// <summary>
        /// Invalid XML schema
        /// </summary>
        InvalidSchema = 5,

        /// <summary>
        /// Missing required elements
        /// </summary>
        MissingElements = 6,

        /// <summary>
        /// Invalid DrawIO format
        /// </summary>
        InvalidDrawIOFormat = 7,

        /// <summary>
        /// File too large
        /// </summary>
        FileTooLarge = 8,

        /// <summary>
        /// Invalid encoding
        /// </summary>
        InvalidEncoding = 9,

        /// <summary>
        /// Unsupported file format
        /// </summary>
        UnsupportedFormat = 10
    }
} 
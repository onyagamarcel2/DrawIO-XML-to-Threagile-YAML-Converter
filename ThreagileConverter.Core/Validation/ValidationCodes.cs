namespace ThreagileConverter.Core.Validation;

/// <summary>
/// Constants for validation error and warning codes
/// </summary>
public static class ValidationCodes
{
    /// <summary>
    /// Warning codes
    /// </summary>
    public static class Warnings
    {
        /// <summary>
        /// Missing host attribute in mxfile element
        /// </summary>
        public const string MissingHost = "WARN_MISSING_HOST";

        /// <summary>
        /// Missing ID attribute
        /// </summary>
        public const string MissingId = "WARN_MISSING_ID";

        /// <summary>
        /// Missing value attribute
        /// </summary>
        public const string MissingValue = "WARN_MISSING_VALUE";

        /// <summary>
        /// Missing style attribute
        /// </summary>
        public const string MissingStyle = "WARN_MISSING_STYLE";

        /// <summary>
        /// Missing position attributes in geometry
        /// </summary>
        public const string MissingPosition = "WARN_MISSING_POSITION";

        /// <summary>
        /// Missing size attributes in geometry
        /// </summary>
        public const string MissingSize = "WARN_MISSING_SIZE";

        /// <summary>
        /// No cells found in diagram
        /// </summary>
        public const string NoCells = "WARN_NO_CELLS";
    }

    /// <summary>
    /// Error codes
    /// </summary>
    public static class Errors
    {
        /// <summary>
        /// Invalid XML document structure
        /// </summary>
        public const string InvalidStructure = "ERR_INVALID_STRUCTURE";

        /// <summary>
        /// Missing required element
        /// </summary>
        public const string MissingElement = "ERR_MISSING_ELEMENT";

        /// <summary>
        /// Missing required attribute
        /// </summary>
        public const string MissingAttribute = "ERR_MISSING_ATTRIBUTE";

        /// <summary>
        /// Invalid attribute value
        /// </summary>
        public const string InvalidAttributeValue = "ERR_INVALID_ATTRIBUTE_VALUE";

        /// <summary>
        /// Schema validation error
        /// </summary>
        public const string SchemaValidation = "ERR_SCHEMA_VALIDATION";

        /// <summary>
        /// Reference to non-existent element
        /// </summary>
        public const string InvalidReference = "ERR_INVALID_REFERENCE";
    }
} 
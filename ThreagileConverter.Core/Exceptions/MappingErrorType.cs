namespace ThreagileConverter.Core.Exceptions
{
    /// <summary>
    /// Represents the type of mapping error
    /// </summary>
    public enum MappingErrorType
    {
        /// <summary>
        /// Unknown mapping error
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Missing source field
        /// </summary>
        MissingSourceField = 1,

        /// <summary>
        /// Invalid source value
        /// </summary>
        InvalidSourceValue = 2,

        /// <summary>
        /// Incompatible types
        /// </summary>
        IncompatibleTypes = 3,

        /// <summary>
        /// Missing mapping rule
        /// </summary>
        MissingMappingRule = 4,

        /// <summary>
        /// Ambiguous mapping
        /// </summary>
        AmbiguousMapping = 5,

        /// <summary>
        /// Circular reference
        /// </summary>
        CircularReference = 6,

        /// <summary>
        /// Invalid target field
        /// </summary>
        InvalidTargetField = 7,

        /// <summary>
        /// Type conversion error
        /// </summary>
        TypeConversionError = 8,

        /// <summary>
        /// Unsupported mapping
        /// </summary>
        UnsupportedMapping = 9,

        /// <summary>
        /// Invalid relationship
        /// </summary>
        InvalidRelationship = 10
    }
} 
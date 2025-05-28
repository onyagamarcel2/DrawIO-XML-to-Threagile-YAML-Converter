namespace ThreagileConverter.Core.Exceptions
{
    /// <summary>
    /// Represents the type of validation error
    /// </summary>
    public enum ValidationErrorType
    {
        /// <summary>
        /// Unknown validation error
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Missing required field
        /// </summary>
        MissingRequiredField = 1,

        /// <summary>
        /// Invalid field value
        /// </summary>
        InvalidFieldValue = 2,

        /// <summary>
        /// Duplicate identifier
        /// </summary>
        DuplicateId = 3,

        /// <summary>
        /// Invalid reference
        /// </summary>
        InvalidReference = 4,

        /// <summary>
        /// Circular reference
        /// </summary>
        CircularReference = 5,

        /// <summary>
        /// Invalid type
        /// </summary>
        InvalidType = 6,

        /// <summary>
        /// Invalid format
        /// </summary>
        InvalidFormat = 7,

        /// <summary>
        /// Constraint violation
        /// </summary>
        ConstraintViolation = 8,

        /// <summary>
        /// Schema validation error
        /// </summary>
        SchemaValidation = 9,

        /// <summary>
        /// Inconsistent model
        /// </summary>
        InconsistentModel = 10
    }
} 
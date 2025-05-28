namespace ThreagileConverter.Core.Exceptions
{
    /// <summary>
    /// Represents the type of generation error
    /// </summary>
    public enum GenerationErrorType
    {
        /// <summary>
        /// Unknown generation error
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// File access denied
        /// </summary>
        AccessDenied = 1,

        /// <summary>
        /// Invalid output path
        /// </summary>
        InvalidPath = 2,

        /// <summary>
        /// Disk full
        /// </summary>
        DiskFull = 3,

        /// <summary>
        /// Invalid model
        /// </summary>
        InvalidModel = 4,

        /// <summary>
        /// Missing required data
        /// </summary>
        MissingData = 5,

        /// <summary>
        /// Invalid YAML format
        /// </summary>
        InvalidYamlFormat = 6,

        /// <summary>
        /// Serialization error
        /// </summary>
        SerializationError = 7,

        /// <summary>
        /// Circular references detected
        /// </summary>
        CircularReferences = 8,

        /// <summary>
        /// Invalid schema
        /// </summary>
        InvalidSchema = 9,

        /// <summary>
        /// File already exists
        /// </summary>
        FileAlreadyExists = 10
    }
} 
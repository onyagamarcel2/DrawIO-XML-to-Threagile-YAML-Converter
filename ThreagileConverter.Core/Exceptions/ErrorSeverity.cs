namespace ThreagileConverter.Core.Exceptions
{
    /// <summary>
    /// Represents the severity level of an error
    /// </summary>
    public enum ErrorSeverity
    {
        /// <summary>
        /// Information only, not an error
        /// </summary>
        Info = 0,

        /// <summary>
        /// Warning that doesn't prevent operation but should be addressed
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Error that prevents normal operation
        /// </summary>
        Error = 2,

        /// <summary>
        /// Critical error that requires immediate attention
        /// </summary>
        Critical = 3
    }
} 
using System;
using System.Runtime.Serialization;

namespace ThreagileConverter.Core.Parsing;

[Serializable]
public class XmlParserException : Exception
{
    public string FilePath { get; }
    public XmlParserErrorType ErrorType { get; }

    public XmlParserException(string message, string filePath, XmlParserErrorType errorType)
        : base(message)
    {
        FilePath = filePath;
        ErrorType = errorType;
    }

    public XmlParserException(string message, string filePath, XmlParserErrorType errorType, Exception innerException)
        : base(message, innerException)
    {
        FilePath = filePath;
        ErrorType = errorType;
    }

    protected XmlParserException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        FilePath = info.GetString(nameof(FilePath));
        ErrorType = (XmlParserErrorType)info.GetInt32(nameof(ErrorType));
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(FilePath), FilePath);
        info.AddValue(nameof(ErrorType), (int)ErrorType);
    }
}

public enum XmlParserErrorType
{
    FileNotFound,
    FileAccessDenied,
    InvalidXml,
    ValidationError,
    NamespaceError,
    ExternalReferenceError,
    Unknown
} 
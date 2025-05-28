using System;

namespace ThreagileConverter.Core.Validation;

/// <summary>
/// Représente une erreur de validation
/// </summary>
public class ValidationError
{
    /// <summary>
    /// Message d'erreur
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Niveau de sévérité de l'erreur
    /// </summary>
    public ValidationSeverity Severity { get; }

    /// <summary>
    /// Identifiant de l'élément concerné
    /// </summary>
    public string ElementId { get; }

    /// <summary>
    /// Détails supplémentaires de l'erreur
    /// </summary>
    public string Details { get; }

    /// <summary>
    /// Crée une nouvelle instance de ValidationError
    /// </summary>
    /// <param name="message">Message d'erreur</param>
    /// <param name="severity">Niveau de sévérité</param>
    /// <param name="elementId">Identifiant de l'élément concerné</param>
    /// <param name="details">Détails supplémentaires de l'erreur</param>
    public ValidationError(string message, ValidationSeverity severity, string elementId = null, string details = "")
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Severity = severity;
        ElementId = elementId;
        Details = details ?? string.Empty;
    }
}

/// <summary>
/// Niveau de sévérité d'une erreur de validation
/// </summary>
public enum ValidationSeverity
{
    /// <summary>
    /// Erreur critique
    /// </summary>
    Error,

    /// <summary>
    /// Avertissement
    /// </summary>
    Warning,

    /// <summary>
    /// Information
    /// </summary>
    Info
} 
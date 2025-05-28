using System.Xml.Linq;

namespace ThreagileConverter.Core.Parsing;

/// <summary>
/// Interface pour les parseurs XML
/// </summary>
public interface IParser
{
    /// <summary>
    /// Parse un fichier XML
    /// </summary>
    /// <param name="filePath">Chemin du fichier XML</param>
    /// <returns>Document XML parsé</returns>
    Task<XDocument> ParseXmlAsync(string filePath);

    /// <summary>
    /// Parse un fichier XML en streaming
    /// </summary>
    /// <param name="filePath">Chemin du fichier XML</param>
    /// <param name="chunkSize">Taille des chunks pour le streaming</param>
    /// <returns>Document XML parsé</returns>
    Task<XDocument> ParseXmlStreamingAsync(string filePath, int chunkSize = 8192);

    /// <summary>
    /// Traite un fichier XML en streaming
    /// </summary>
    /// <param name="filePath">Chemin du fichier XML</param>
    /// <param name="processElement">Fonction de traitement pour chaque élément</param>
    /// <param name="elementName">Nom de l'élément à traiter (optionnel)</param>
    /// <returns>Task</returns>
    Task ProcessXmlStreamingAsync(string filePath, Func<XElement, Task> processElement, string? elementName = null);
} 
using System.Threading.Tasks;
using ThreagileConverter.Core.Models;

namespace ThreagileConverter.Core.Mapping;

/// <summary>
/// Interface for mapping between DrawIO and Threagile models
/// </summary>
public interface IMapper
{
    /// <summary>
    /// Maps a DrawIO model to a Threagile model
    /// </summary>
    /// <param name="drawioModel">The DrawIO model to map</param>
    /// <returns>The mapped Threagile model</returns>
    Task<ThreagileModel> MapToThreagileAsync(DrawIOModel drawioModel);

    /// <summary>
    /// Maps a Threagile model to a DrawIO model
    /// </summary>
    /// <param name="threagileModel">The Threagile model to map</param>
    /// <returns>The mapped DrawIO model</returns>
    Task<DrawIOModel> MapToDrawIOAsync(ThreagileModel threagileModel);

    /// <summary>
    /// Converts a DrawIO style to a Threagile style
    /// </summary>
    /// <param name="style">The DrawIO style to convert</param>
    /// <returns>The converted Threagile style</returns>
    ThreagileStyle ConvertStyle(DrawIOStyle style);

    /// <summary>
    /// Valide les types de données du modèle Threagile
    /// </summary>
    /// <param name="model">Le modèle Threagile à valider</param>
    /// <returns>True si la validation est réussie, false sinon</returns>
    bool ValidateTypes(ThreagileModel model);

    /// <summary>
    /// Gère les relations entre les éléments du modèle Threagile
    /// </summary>
    /// <param name="model">Le modèle Threagile à traiter</param>
    void ProcessRelations(ThreagileModel model);

    /// <summary>
    /// Valide les contraintes du modèle Threagile
    /// </summary>
    /// <param name="model">Le modèle Threagile à valider</param>
    /// <returns>True si la validation est réussie, false sinon</returns>
    bool ValidateConstraints(ThreagileModel model);
} 
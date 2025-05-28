using System;
using System.Threading.Tasks;

namespace ThreagileConverter.Core.Validation
{
    /// <summary>
    /// Interface définissant une stratégie de validation
    /// </summary>
    public interface IValidationStrategy
    {
        /// <summary>
        /// Valide le contenu selon la stratégie spécifique
        /// </summary>
        /// <param name="content">Contenu à valider</param>
        /// <returns>Résultat de la validation</returns>
        Task<ValidationResult> ValidateAsync(string content);

        /// <summary>
        /// Vérifie si la stratégie peut être appliquée au contenu
        /// </summary>
        /// <param name="content">Contenu à vérifier</param>
        /// <returns>True si la stratégie peut être appliquée, False sinon</returns>
        bool CanValidate(string content);
    }
} 
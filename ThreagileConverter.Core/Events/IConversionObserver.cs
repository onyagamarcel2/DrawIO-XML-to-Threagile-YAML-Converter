using System;
using System.Threading.Tasks;

namespace ThreagileConverter.Core.Events
{
    /// <summary>
    /// Interface définissant un observateur de conversion
    /// </summary>
    public interface IConversionObserver
    {
        /// <summary>
        /// Appelé lorsque la conversion commence
        /// </summary>
        /// <param name="sourcePath">Chemin du fichier source</param>
        /// <param name="targetPath">Chemin du fichier cible</param>
        Task OnConversionStartedAsync(string sourcePath, string targetPath);

        /// <summary>
        /// Appelé lorsque la conversion progresse
        /// </summary>
        /// <param name="progress">Progression (0-100)</param>
        /// <param name="message">Message de progression</param>
        Task OnConversionProgressAsync(int progress, string message);

        /// <summary>
        /// Appelé lorsque la conversion est terminée avec succès
        /// </summary>
        /// <param name="result">Résultat de la conversion</param>
        Task OnConversionCompletedAsync(ConversionResult result);

        /// <summary>
        /// Appelé lorsqu'une erreur se produit pendant la conversion
        /// </summary>
        /// <param name="error">Erreur survenue</param>
        Task OnConversionErrorAsync(Exception error);
    }
} 
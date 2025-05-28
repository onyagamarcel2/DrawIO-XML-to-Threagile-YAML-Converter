using System.Threading.Tasks;
using ThreagileConverter.Core.Events;

namespace ThreagileConverter.Core.Commands
{
    /// <summary>
    /// Interface définissant une commande de conversion
    /// </summary>
    public interface IConversionCommand
    {
        /// <summary>
        /// Exécute la commande de conversion
        /// </summary>
        /// <param name="observer">Observateur pour suivre la progression</param>
        /// <returns>Résultat de la conversion</returns>
        Task<ConversionResult> ExecuteAsync(IConversionObserver observer);

        /// <summary>
        /// Annule l'exécution de la commande
        /// </summary>
        Task CancelAsync();
    }
} 
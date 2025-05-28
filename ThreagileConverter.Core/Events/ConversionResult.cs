using System;
using System.Collections.Generic;

namespace ThreagileConverter.Core.Events
{
    /// <summary>
    /// Représente le résultat d'une conversion
    /// </summary>
    public class ConversionResult
    {
        /// <summary>
        /// Indique si la conversion a réussi
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Chemin du fichier source
        /// </summary>
        public string SourcePath { get; }

        /// <summary>
        /// Chemin du fichier cible
        /// </summary>
        public string TargetPath { get; }

        /// <summary>
        /// Durée de la conversion
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Nombre d'éléments convertis
        /// </summary>
        public int ConvertedElements { get; }

        /// <summary>
        /// Liste des avertissements générés pendant la conversion
        /// </summary>
        public IReadOnlyList<string> Warnings { get; }

        /// <summary>
        /// Crée une nouvelle instance de ConversionResult
        /// </summary>
        public ConversionResult(
            bool success,
            string sourcePath,
            string targetPath,
            TimeSpan duration,
            int convertedElements,
            IReadOnlyList<string> warnings)
        {
            Success = success;
            SourcePath = sourcePath ?? throw new ArgumentNullException(nameof(sourcePath));
            TargetPath = targetPath ?? throw new ArgumentNullException(nameof(targetPath));
            Duration = duration;
            ConvertedElements = convertedElements;
            Warnings = warnings ?? throw new ArgumentNullException(nameof(warnings));
        }

        /// <summary>
        /// Crée un résultat de conversion réussi
        /// </summary>
        public static ConversionResult CreateSuccess(
            string sourcePath,
            string targetPath,
            TimeSpan duration,
            int convertedElements,
            IReadOnlyList<string> warnings = null)
        {
            return new ConversionResult(
                true,
                sourcePath,
                targetPath,
                duration,
                convertedElements,
                warnings ?? Array.Empty<string>());
        }

        /// <summary>
        /// Crée un résultat de conversion échoué
        /// </summary>
        public static ConversionResult CreateFailure(
            string sourcePath,
            string targetPath,
            TimeSpan duration,
            IReadOnlyList<string> warnings = null)
        {
            return new ConversionResult(
                false,
                sourcePath,
                targetPath,
                duration,
                0,
                warnings ?? Array.Empty<string>());
        }
    }
} 
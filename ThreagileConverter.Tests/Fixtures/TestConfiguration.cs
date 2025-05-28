using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace ThreagileConverter.Tests.Fixtures
{
    /// <summary>
    /// Classe de configuration commune pour les tests.
    /// Fournit des méthodes utilitaires et des configurations partagées entre les tests.
    /// </summary>
    public static class TestConfiguration
    {
        /// <summary>
        /// Crée un logger de test configuré.
        /// </summary>
        /// <typeparam name="T">Le type pour lequel créer le logger.</typeparam>
        /// <returns>Une instance de ILogger configurée pour les tests.</returns>
        public static ILogger<T> CreateTestLogger<T>()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddConsole()
                    .SetMinimumLevel(LogLevel.Debug);
            });
            return loggerFactory.CreateLogger<T>();
        }

        /// <summary>
        /// Crée un répertoire temporaire pour les tests.
        /// </summary>
        /// <param name="subDirectory">Sous-répertoire optionnel à créer.</param>
        /// <returns>Le chemin du répertoire temporaire créé.</returns>
        public static string CreateTempDirectory(string subDirectory = null)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "ThreagileConverterTests", Guid.NewGuid().ToString());
            if (!string.IsNullOrEmpty(subDirectory))
            {
                tempPath = Path.Combine(tempPath, subDirectory);
            }
            Directory.CreateDirectory(tempPath);
            return tempPath;
        }

        /// <summary>
        /// Nettoie un répertoire temporaire.
        /// </summary>
        /// <param name="directoryPath">Le chemin du répertoire à nettoyer.</param>
        public static void CleanupTempDirectory(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }

        /// <summary>
        /// Crée un fichier temporaire avec le contenu spécifié.
        /// </summary>
        /// <param name="content">Le contenu du fichier.</param>
        /// <param name="extension">L'extension du fichier (sans le point).</param>
        /// <returns>Le chemin du fichier temporaire créé.</returns>
        public static string CreateTempFile(string content, string extension = "tmp")
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"ThreagileConverterTests_{Guid.NewGuid()}.{extension}");
            File.WriteAllText(filePath, content);
            return filePath;
        }

        /// <summary>
        /// Nettoie un fichier temporaire.
        /// </summary>
        /// <param name="filePath">Le chemin du fichier à nettoyer.</param>
        public static void CleanupTempFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
} 
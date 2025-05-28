using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ThreagileConverter.Core.Events;
using ThreagileConverter.Core.Factories;
using ThreagileConverter.Core.Validation;
using System.Xml.Linq;

namespace ThreagileConverter.Core.Commands
{
    /// <summary>
    /// Implémentation de la commande de conversion XML vers YAML
    /// </summary>
    public class XmlToYamlConversionCommand : IConversionCommand
    {
        private readonly string _sourcePath;
        private readonly string _targetPath;
        private readonly IParserFactory _parserFactory;
        private readonly ILogger<XmlToYamlConversionCommand> _logger;
        private CancellationTokenSource _cancellationTokenSource;

        public XmlToYamlConversionCommand(
            string sourcePath,
            string targetPath,
            IParserFactory parserFactory,
            ILogger<XmlToYamlConversionCommand> logger)
        {
            _sourcePath = sourcePath ?? throw new ArgumentNullException(nameof(sourcePath));
            _targetPath = targetPath ?? throw new ArgumentNullException(nameof(targetPath));
            _parserFactory = parserFactory ?? throw new ArgumentNullException(nameof(parserFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task<ConversionResult> ExecuteAsync(IConversionObserver observer)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            _cancellationTokenSource = new CancellationTokenSource();
            var stopwatch = Stopwatch.StartNew();
            var warnings = new List<string>();

            try
            {
                await observer.OnConversionStartedAsync(_sourcePath, _targetPath);

                // Crée les composants nécessaires
                var xmlParser = _parserFactory.CreateParser("xml");
                var xmlValidator = _parserFactory.CreateValidator("xml");
                var yamlGenerator = _parserFactory.CreateGenerator("yaml");

                // Lit le fichier source
                await observer.OnConversionProgressAsync(10, "Lecture du fichier source...");
                var xmlContent = await System.IO.File.ReadAllTextAsync(_sourcePath);

                // Parse le XML
                await observer.OnConversionProgressAsync(20, "Parsing du fichier XML...");
                var xmlModel = await xmlParser.ParseXmlAsync(_sourcePath);

                // Valide le XML
                await observer.OnConversionProgressAsync(40, "Validation du fichier XML...");
                var validationResult = await xmlValidator.ValidateAsync(xmlModel);
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Errors)
                    {
                        warnings.Add($"Erreur de validation: {error.Message}");
                    }
                    return ConversionResult.CreateFailure(_sourcePath, _targetPath, stopwatch.Elapsed, warnings);
                }

                // Génère le YAML
                await observer.OnConversionProgressAsync(70, "Génération du fichier YAML...");
                await yamlGenerator.GenerateAsync(xmlModel, _targetPath);

                // Écrit le fichier cible (déjà fait par GenerateAsync)
                await observer.OnConversionProgressAsync(90, "Écriture du fichier cible...");

                stopwatch.Stop();
                var result = ConversionResult.CreateSuccess(
                    _sourcePath,
                    _targetPath,
                    stopwatch.Elapsed,
                    xmlModel.Elements().Count(),
                    warnings);

                await observer.OnConversionCompletedAsync(result);
                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Conversion annulée");
                return ConversionResult.CreateFailure(_sourcePath, _targetPath, stopwatch.Elapsed, warnings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la conversion");
                await observer.OnConversionErrorAsync(ex);
                return ConversionResult.CreateFailure(_sourcePath, _targetPath, stopwatch.Elapsed, warnings);
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        public Task CancelAsync()
        {
            _cancellationTokenSource?.Cancel();
            return Task.CompletedTask;
        }
    }
} 
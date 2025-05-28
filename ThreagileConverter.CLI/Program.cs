using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using ThreagileConverter.Core.Configuration;
using ThreagileConverter.Core.Generation;
using ThreagileConverter.Core.Logging;
using ThreagileConverter.Core.Mapping;
using ThreagileConverter.Core.Parsers;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Création du host générique .NET
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                // Ajout de la configuration personnalisée
                services.AddAppConfiguration(context.Configuration);

                // Ajout du logging avec la configuration simple
                services.AddLogging(builder =>
                {
                    builder.ConfigureLogging();
                });

                // Enregistrement des services
                services.AddTransient<IDrawIOParser, DrawIOParser>();
                services.AddTransient<IMapper, Mapper>();
                services.AddTransient<IYamlGenerator, YamlGenerator>();
            })
            .Build();

        // Configuration de la ligne de commande
        var rootCommand = new RootCommand("Convertisseur XML (DrawIO) → YAML (Threagile)");
        
        var inputOption = new Option<string>(
            aliases: new[] { "--input", "-i" },
            description: "Chemin du fichier XML DrawIO à convertir");
        
        var outputOption = new Option<string>(
            aliases: new[] { "--output", "-o" },
            description: "Chemin du fichier YAML de sortie");
        
        var validateInputOption = new Option<bool>(
            aliases: new[] { "--validate-input" },
            description: "Valider le fichier XML en entrée",
            getDefaultValue: () => true);
        
        var validateOutputOption = new Option<bool>(
            aliases: new[] { "--validate-output" },
            description: "Valider le fichier YAML généré",
            getDefaultValue: () => true);
        
        rootCommand.AddOption(inputOption);
        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(validateInputOption);
        rootCommand.AddOption(validateOutputOption);
        
        rootCommand.SetHandler(async (string input, string output, bool validateInput, bool validateOutput) =>
        {
            await ConvertAsync(host, input, output, validateInput, validateOutput);
        }, inputOption, outputOption, validateInputOption, validateOutputOption);
        
        return await rootCommand.InvokeAsync(args);
    }

    private static async Task ConvertAsync(IHost host, string inputPath, string outputPath, bool validateInput, bool validateOutput)
    {
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        var config = host.Services.GetRequiredService<AppConfiguration>();
        
        // Utiliser les valeurs de la configuration si les paramètres ne sont pas fournis
        inputPath = string.IsNullOrEmpty(inputPath) ? config.InputPath : inputPath;
        outputPath = string.IsNullOrEmpty(outputPath) ? config.OutputPath : outputPath;
        
        // Vérifier que les chemins sont valides
        if (string.IsNullOrEmpty(inputPath))
        {
            logger.LogError("Chemin du fichier d'entrée non spécifié");
            return;
        }
        
        if (string.IsNullOrEmpty(outputPath))
        {
            logger.LogError("Chemin du fichier de sortie non spécifié");
            return;
        }
        
        // Vérifier que le fichier d'entrée existe
        if (!File.Exists(inputPath))
        {
            logger.LogError("Le fichier d'entrée n'existe pas: {InputPath}", inputPath);
            return;
        }
        
        try
        {
            // Lire le fichier XML
            logger.LogInformation("Lecture du fichier XML: {InputPath}", inputPath);
            var xmlContent = await File.ReadAllTextAsync(inputPath);
            
            // Parser le fichier XML
            logger.LogInformation("Parsing du fichier XML");
            var parser = host.Services.GetRequiredService<IDrawIOParser>();
            var drawioModel = await parser.ParseAsync(xmlContent);
            
            // Convertir en modèle Threagile
            logger.LogInformation("Conversion en modèle Threagile");
            var mapper = host.Services.GetRequiredService<IMapper>();
            var threagileModel = await mapper.MapToThreagileAsync(drawioModel);
            
            // Générer le fichier YAML
            logger.LogInformation("Génération du fichier YAML: {OutputPath}", outputPath);
            var generator = host.Services.GetRequiredService<IYamlGenerator>();
            generator.GenerateYamlToFile(threagileModel, outputPath);
            
            logger.LogInformation("Conversion terminée avec succès");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la conversion");
        }
    }
}

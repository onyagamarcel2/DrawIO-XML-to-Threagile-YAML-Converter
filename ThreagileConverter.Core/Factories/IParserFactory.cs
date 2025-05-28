using System;
using ThreagileConverter.Core.Parsing;
using ThreagileConverter.Core.Validation;
using ThreagileConverter.Core.Generation;

namespace ThreagileConverter.Core.Factories
{
    /// <summary>
    /// Interface définissant la factory pour la création des différents types de parsers
    /// </summary>
    public interface IParserFactory
    {
        /// <summary>
        /// Crée un parser en fonction du type spécifié
        /// </summary>
        /// <param name="type">Type de parser à créer</param>
        /// <returns>Instance du parser créé</returns>
        IParser CreateParser(string type);

        /// <summary>
        /// Crée un validateur en fonction du type spécifié
        /// </summary>
        /// <param name="type">Type de validateur à créer</param>
        /// <returns>Instance du validateur créé</returns>
        IValidator CreateValidator(string type);

        /// <summary>
        /// Crée un générateur en fonction du type spécifié
        /// </summary>
        /// <param name="type">Type de générateur à créer</param>
        /// <returns>Instance du générateur créé</returns>
        IGenerator CreateGenerator(string type);
    }
} 
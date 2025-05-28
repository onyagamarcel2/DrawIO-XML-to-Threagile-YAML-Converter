# Tests du Projet ThreagileConverter

Ce dossier contient tous les tests du projet ThreagileConverter. L'organisation suit les meilleures pratiques de test en .NET.

## Structure des Tests

```
ThreagileConverter.Tests/
├── Unit/                    # Tests unitaires
│   ├── Commands/           # Tests des commandes
│   ├── Events/             # Tests des événements
│   ├── Factories/          # Tests des factories
│   ├── Parsing/            # Tests des parsers
│   ├── Repositories/       # Tests des repositories
│   └── Validation/         # Tests des validateurs
├── Integration/            # Tests d'intégration
│   ├── Commands/          # Tests d'intégration des commandes
│   ├── Events/            # Tests d'intégration des événements
│   └── ...
├── TestData/              # Données de test
│   ├── Xml/              # Fichiers XML de test
│   └── Yaml/             # Fichiers YAML de test
└── Fixtures/             # Classes de configuration communes
```

## Conventions de Nommage

- Les classes de test : `[ClasseTestée]Tests`
- Les méthodes de test : `[Méthode]_[Scénario]_[RésultatAttendu]`
- Les régions de test : 
  - `Tests de [Fonctionnalité] Réussi`
  - `Tests de Gestion des Erreurs`
  - `Tests de Performance`
  - `Tests de Logging`

## Organisation des Tests

Chaque classe de test suit le pattern AAA (Arrange-Act-Assert) et est organisée en régions logiques :

```csharp
public class ExampleTests
{
    #region Tests de [Fonctionnalité] Réussi
    [Fact]
    public void Method_Scenario_ExpectedResult()
    {
        // Arrange
        // Act
        // Assert
    }
    #endregion

    #region Tests de Gestion des Erreurs
    // Tests des cas d'erreur
    #endregion

    #region Tests de Performance
    // Tests de performance
    #endregion

    #region Tests de Logging
    // Tests des logs
    #endregion
}
```

## Bonnes Pratiques

1. **Isolation** : Chaque test doit être indépendant des autres
2. **Nettoyage** : Utiliser `IDisposable` pour nettoyer les ressources
3. **Mocks** : Utiliser des mocks pour les dépendances externes
4. **Documentation** : Documenter les scénarios de test avec des commentaires XML
5. **Cohérence** : Maintenir une structure cohérente dans tous les tests

## Exécution des Tests

Pour exécuter les tests :

```bash
# Exécuter tous les tests
dotnet test

# Exécuter les tests unitaires uniquement
dotnet test --filter "Category=Unit"

# Exécuter les tests d'intégration uniquement
dotnet test --filter "Category=Integration"
``` 
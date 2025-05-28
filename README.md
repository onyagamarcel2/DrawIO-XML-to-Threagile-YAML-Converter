# ThreagileConverter

Convertisseur XML (DrawIO) → YAML (Threagile)

## Description
Ce projet permet de convertir des fichiers XML issus de diagrammes DrawIO en fichiers YAML compatibles avec Threagile.

## Fonctionnalités principales
- Parsing de fichiers XML DrawIO
- Génération de fichiers YAML Threagile
- Extraction et validation des métadonnées
- Logging configurable (console, extensible)
- Configuration centralisée via `appsettings.json`

## Prérequis
- .NET 8 SDK

## Installation
```bash
dotnet restore
dotnet build
```

## Utilisation
```bash
dotnet run --project ThreagileConverter.CLI -- --input <chemin-fichier-xml> --output <chemin-fichier-yaml>
```

Options disponibles:
- `--input`, `-i` : Chemin du fichier XML DrawIO à convertir
- `--output`, `-o` : Chemin du fichier YAML de sortie
- `--validate-input` : Valider le fichier XML en entrée (par défaut: true)
- `--validate-output` : Valider le fichier YAML généré (par défaut: true)

## Configuration
La configuration de l'application se fait via le fichier `appsettings.json` situé dans le dossier `ThreagileConverter.CLI`.

Exemple de contenu par défaut :
```json
{
  "InputPath": "",
  "OutputPath": "",
  "ValidateInput": true,
  "ValidateOutput": true,
  "ExtractMetadata": true,
  "MinimumLogLevel": "Information",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  }
}
```

### Options disponibles
- **InputPath** : Chemin du fichier XML DrawIO à convertir (laisser vide pour fournir en argument CLI).
- **OutputPath** : Chemin du fichier YAML de sortie (laisser vide pour fournir en argument CLI).
- **ValidateInput** : Active la validation du fichier XML en entrée (par défaut : true).
- **ValidateOutput** : Active la validation du fichier YAML généré (par défaut : true).
- **ExtractMetadata** : Active l'extraction des métadonnées du diagramme (par défaut : true).
- **MinimumLogLevel** : Niveau minimal des logs (Trace, Debug, Information, Warning, Error, Critical).
- **Logging** : Configuration avancée des logs (voir documentation Microsoft.Extensions.Logging).

Vous pouvez modifier ces options pour adapter le comportement de l'application à vos besoins.

## Analyse des Formats

### Format XML DrawIO
Le format XML généré par DrawIO contient généralement les éléments suivants :
- **Diagramme** : Élément racine contenant les informations générales du diagramme.
- **Éléments graphiques** : Représentés par des balises comme `<mxCell>`, `<object>`, etc., avec des attributs décrivant leur position, style, et contenu.
- **Métadonnées** : Informations supplémentaires (titre, auteur, date, etc.) stockées dans des attributs ou des nœuds dédiés.
- **Styles** : Définis par des attributs ou des nœuds spécifiques pour personnaliser l'apparence des éléments.

#### Cas particuliers et pièges
- **Namespaces** : Le XML DrawIO peut utiliser des namespaces (ex: `xmlns:mx="..."`). Il faut les gérer correctement lors du parsing.
- **Références externes** : Certains éléments peuvent référencer des ressources externes (images, liens, etc.). Il faut vérifier leur validité.
- **Styles complexes** : Les styles peuvent être imbriqués ou hérités. Il faut s'assurer de les extraire correctement.
- **Éléments vides ou malformés** : Certains éléments peuvent être vides ou malformés. Il faut gérer ces cas pour éviter des erreurs.

### Format YAML Threagile
Le format YAML attendu par Threagile est structuré pour représenter un modèle de menace. Il inclut généralement :
- **Entités** : Représentant les composants du système (applications, serveurs, bases de données, etc.).
- **Relations** : Définissant les interactions entre les entités.
- **Propriétés** : Attributs spécifiques à chaque entité ou relation (nom, description, niveau de confiance, etc.).
- **Métadonnées** : Informations générales sur le modèle (version, auteur, date, etc.).

#### Cas particuliers et pièges
- **Types complexes** : Certaines propriétés peuvent être des objets ou des listes. Il faut s'assurer de les sérialiser correctement en YAML.
- **Références circulaires** : Les relations peuvent créer des références circulaires. Il faut les gérer pour éviter des boucles infinies.
- **Validation du schéma** : Le YAML généré doit respecter le schéma Threagile. Il faut valider la structure et les types.
- **Préservation des commentaires** : Les commentaires dans le XML doivent être préservés dans le YAML si possible.

### Mapping entre XML DrawIO et YAML Threagile
Le mapping consiste à :
- Extraire les éléments graphiques du XML DrawIO et les convertir en entités Threagile.
- Transformer les connexions entre éléments en relations Threagile.
- Préserver les métadonnées et les styles dans la mesure du possible.
- Valider la cohérence et la complétude du modèle généré.

#### Besoins de validation
- **Cohérence des entités** : Vérifier que chaque entité a un identifiant unique et des propriétés valides.
- **Cohérence des relations** : Vérifier que chaque relation référence des entités existantes.
- **Complétude du modèle** : Vérifier que toutes les entités et relations nécessaires sont présentes.
- **Validation des types** : Vérifier que les types de données sont corrects (ex: nombres, booléens, chaînes).

Cette analyse est en cours et sera enrichie au fur et à mesure du développement.

## Cas d'Utilisation Principaux

1. **Conversion d'un diagramme DrawIO en YAML Threagile**
   - L'utilisateur fournit un fichier XML DrawIO.
   - L'application convertit le diagramme en YAML Threagile.
   - L'utilisateur récupère le fichier YAML généré.

2. **Validation du fichier XML en entrée**
   - L'utilisateur active l'option `ValidateInput`.
   - L'application vérifie la structure et la validité du fichier XML.
   - Des erreurs sont signalées si le fichier est invalide.

3. **Validation du fichier YAML généré**
   - L'utilisateur active l'option `ValidateOutput`.
   - L'application vérifie que le YAML généré respecte le schéma Threagile.
   - Des erreurs sont signalées si le YAML est invalide.

4. **Extraction des métadonnées**
   - L'utilisateur active l'option `ExtractMetadata`.
   - L'application extrait les métadonnées du diagramme (titre, auteur, date, etc.).
   - Les métadonnées sont incluses dans le fichier YAML généré.

5. **Gestion des erreurs**
   - L'application gère les erreurs de parsing, de validation, et de génération.
   - Des messages d'erreur clairs sont fournis à l'utilisateur.

## Scénarios de Test

1. **Tests de parsing XML**
   - Tester le parsing d'un fichier XML DrawIO valide.
   - Tester le parsing d'un fichier XML invalide ou malformé.
   - Tester le parsing d'un fichier XML avec des namespaces.

2. **Tests de validation**
   - Tester la validation d'un fichier XML en entrée.
   - Tester la validation d'un fichier YAML généré.
   - Tester la validation des métadonnées.

3. **Tests de génération YAML**
   - Tester la génération d'un fichier YAML à partir d'un diagramme simple.
   - Tester la génération d'un fichier YAML à partir d'un diagramme complexe.
   - Tester la préservation des métadonnées et des styles.

4. **Tests d'intégration**
   - Tester le flux complet de conversion (XML → YAML).
   - Tester la gestion des erreurs et des cas particuliers.

5. **Tests de performance**
   - Tester la performance du parsing et de la génération avec des fichiers volumineux.

Ces scénarios de test seront implémentés dans les projets de tests unitaires et d'intégration.

## Avancement
Ce fichier sera enrichi au fur et à mesure du développement (voir checklist du projet).

## Architecture et Modules

### Vue d'ensemble
Le projet est structuré en plusieurs modules qui travaillent ensemble pour assurer la conversion des diagrammes DrawIO en format Threagile. L'architecture suit les principes SOLID et utilise plusieurs design patterns pour assurer la maintenabilité et l'extensibilité du code.

### Modules Principaux

#### 1. Module XML Parser (`ThreagileConverter.Core/Parsing`)
Responsable du parsing et de la validation des fichiers XML DrawIO.

**Fonctionnalités principales :**
- Parsing de base des fichiers XML
- Support des namespaces
- Validation XSD
- Gestion des références externes
- Support du streaming pour les gros fichiers
- Gestion des erreurs avec messages détaillés

**Classes principales :**
- `XmlParser` : Classe principale pour le parsing XML
- `XmlParserException` : Gestion des erreurs spécifiques au parsing
- `ValidationResult` : Résultat de la validation XSD
- `ExternalReference` : Gestion des références externes

#### 2. Module DrawIO Adapter (`ThreagileConverter.Core/Adapters`)
Adapte les données DrawIO au format interne de l'application.

**Fonctionnalités principales :**
- Extraction des styles
- Extraction des métadonnées
- Conversion des formes
- Gestion des couches
- Gestion des groupes

#### 3. Module YAML Generator (`ThreagileConverter.Core/Generation`)
Génère les fichiers YAML au format Threagile.

**Fonctionnalités principales :**
- Génération de base des fichiers YAML
- Validation du schéma
- Gestion des références
- Support des types complexes
- Préservation des commentaires

#### 4. Module de Mapping (`ThreagileConverter.Core/Mapping`)
Gère la conversion entre les formats DrawIO et Threagile.

**Fonctionnalités principales :**
- Mapping XML → YAML
- Validation des types
- Gestion des relations
- Support des métadonnées
- Conversion des styles

### Design Patterns Utilisés

1. **Repository Pattern**
   - `XmlRepository` : Gestion de l'accès aux fichiers XML
   - Tests unitaires associés

2. **Factory Pattern**
   - `ParserFactory` : Création des parsers appropriés
   - Tests unitaires associés

3. **Strategy Pattern**
   - `XmlValidationStrategy` : Stratégies de validation XML
   - Tests unitaires associés

4. **Observer Pattern**
   - `ConversionObserver` : Suivi de la progression de la conversion
   - Tests unitaires associés

5. **Command Pattern**
   - `XmlToYamlConversionCommand` : Encapsulation de la logique de conversion
   - Tests unitaires associés

### Tests

#### Tests Unitaires
- Tests du XML Parser
- Tests du DrawIO Adapter
- Tests du YAML Generator
- Tests du Mapping
- Tests des Design Patterns

#### Tests d'Intégration
- Tests du flux complet de conversion
- Tests de performance
- Tests de charge
- Tests de sécurité

### Logging et Monitoring
- Logging configurable (console, fichiers)
- Niveaux de log ajustables
- Messages d'erreur détaillés
- Suivi de la progression

### Gestion des Erreurs
- Exceptions personnalisées
- Messages d'erreur clairs
- Logging des erreurs
- Récupération gracieuse

### Configuration
- Configuration via `appsettings.json`
- Options extensibles
- Validation de la configuration
- Documentation des options

Cette architecture modulaire permet une maintenance facile et l'ajout de nouvelles fonctionnalités. Chaque module est testé indépendamment et l'intégration est vérifiée par des tests d'intégration. 
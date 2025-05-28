# Documentation Technique - ThreagileConverter

## Architecture Globale

```mermaid
graph TB
    subgraph CLI[ThreagileConverter.CLI]
        CLI_Entry[Point d'entrée]
        CLI_Config[Configuration]
        CLI_Logging[Logging]
    end

    subgraph Core[ThreagileConverter.Core]
        XML_Parser[Parser XML]
        DrawIO_Adapter[Adaptateur DrawIO]
        YAML_Generator[Générateur YAML]
        Mapper[Mapper XML→YAML]
        Validation[Validation]
        Metadata[Extraction Métadonnées]
    end

    subgraph Tests[Tests]
        Unit_Tests[Tests Unitaires]
        Integration_Tests[Tests d'Intégration]
    end

    CLI_Entry --> CLI_Config
    CLI_Entry --> CLI_Logging
    CLI_Entry --> Core
    Core --> Tests

    %% Flux de données
    XML_Parser --> DrawIO_Adapter
    DrawIO_Adapter --> Mapper
    Mapper --> YAML_Generator
    Validation --> XML_Parser
    Validation --> YAML_Generator
    Metadata --> DrawIO_Adapter
    Metadata --> YAML_Generator
```

*Commentaire : Ce diagramme montre l'architecture globale du projet avec ses composants principaux et leurs interactions. Les flèches indiquent les dépendances et le flux de données.*

## Flux de Données

```mermaid
sequenceDiagram
    participant CLI as CLI
    participant Config as Configuration
    participant Parser as XML Parser
    participant Adapter as DrawIO Adapter
    participant Mapper as Mapper
    participant Generator as YAML Generator
    participant Validator as Validator
    participant Metadata as Metadata Extractor

    CLI->>Config: Charger configuration
    CLI->>Parser: Lire fichier XML
    Parser->>Validator: Valider XML
    Parser->>Adapter: Convertir en modèle DrawIO
    Adapter->>Metadata: Extraire métadonnées
    Adapter->>Mapper: Transformer en modèle Threagile
    Mapper->>Generator: Générer YAML
    Generator->>Validator: Valider YAML
    Generator->>CLI: Retourner résultat
```

*Commentaire : Ce diagramme de séquence illustre le flux de données à travers les différents composants du système, de la lecture du fichier XML jusqu'à la génération du YAML.*

## Structure des Modules

### Module XML Parser

```mermaid
classDiagram
    class IXmlParser {
        <<interface>>
        +ParseXml(string path) XDocument
        +ValidateXml(XDocument doc) bool
        +HandleNamespaces(XDocument doc) void
        +ExtractElements(XElement root) List~Element~
    }
    class XmlParser {
        -ILogger _logger
        -IValidator _validator
        +ParseXml(string path) XDocument
        +ValidateXml(XDocument doc) bool
        +HandleNamespaces(XDocument doc) void
        +ExtractElements(XElement root) List~Element~
        -ValidateStructure(XDocument doc) bool
        -ValidateContent(XElement element) bool
    }
    class XmlValidationException {
        +string Message
        +string Details
        +ValidationErrorType ErrorType
        +string ElementPath
    }

    IXmlParser <|.. XmlParser
    XmlParser ..> XmlValidationException
    XmlParser ..> IValidator
```

*Commentaire : Ce diagramme de classe montre la structure du module XML Parser avec son interface, son implémentation et les exceptions associées. Ajout des dépendances manquantes et des méthodes de validation.*

### Module DrawIO Adapter

```mermaid
classDiagram
    class IDrawIOAdapter {
        <<interface>>
        +ConvertToModel(XDocument doc) DrawIOModel
        +ExtractStyles(XElement element) Style
        +ExtractMetadata(XDocument doc) Metadata
        +ProcessShapes(List~Element~ elements) void
        +ProcessConnections(List~Element~ elements) void
    }
    class DrawIOAdapter {
        -ILogger _logger
        -IMetadataExtractor _metadataExtractor
        +ConvertToModel(XDocument doc) DrawIOModel
        +ExtractStyles(XElement element) Style
        +ExtractMetadata(XDocument doc) Metadata
        +ProcessShapes(List~Element~ elements) void
        +ProcessConnections(List~Element~ elements) void
        -ValidateShape(Element shape) bool
        -ValidateConnection(Element connection) bool
    }
    class DrawIOModel {
        +List~Shape~ Shapes
        +List~Connection~ Connections
        +Metadata Metadata
        +Dictionary~string,Style~ Styles
        +Dictionary~string,string~ Properties
    }

    IDrawIOAdapter <|.. DrawIOAdapter
    DrawIOAdapter ..> DrawIOModel
    DrawIOAdapter ..> IMetadataExtractor
```

*Commentaire : Ce diagramme de classe illustre la structure du module DrawIO Adapter qui convertit le XML DrawIO en un modèle interne. Ajout des dépendances et méthodes de validation manquantes.*

### Module YAML Generator

```mermaid
classDiagram
    class IYamlGenerator {
        <<interface>>
        +GenerateYaml(ThreagileModel model) string
        +ValidateYaml(string yaml) bool
        +PreserveComments(string yaml) string
        +SerializeEntity(Entity entity) string
        +SerializeRelation(Relation relation) string
    }
    class YamlGenerator {
        -ILogger _logger
        -IValidator _validator
        +GenerateYaml(ThreagileModel model) string
        +ValidateYaml(string yaml) bool
        +PreserveComments(string yaml) string
        +SerializeEntity(Entity entity) string
        +SerializeRelation(Relation relation) string
        -ValidateEntity(Entity entity) bool
        -ValidateRelation(Relation relation) bool
    }
    class ThreagileModel {
        +List~Entity~ Entities
        +List~Relation~ Relations
        +Metadata Metadata
        +Dictionary~string,object~ Properties
        +Dictionary~string,string~ Comments
    }

    IYamlGenerator <|.. YamlGenerator
    YamlGenerator ..> ThreagileModel
    YamlGenerator ..> IValidator
```

*Commentaire : Ce diagramme de classe montre la structure du module YAML Generator qui convertit le modèle Threagile en YAML. Ajout des dépendances et méthodes de validation manquantes.*

## Patterns de Conception

### Pattern Repository

```mermaid
classDiagram
    class IRepository~T~ {
        <<interface>>
        +GetById(string id) T
        +GetAll() IEnumerable~T~
        +Add(T entity) void
        +Update(T entity) void
        +Delete(string id) void
        +Exists(string id) bool
        +Count() int
    }
    class XmlRepository {
        -string _filePath
        -ILogger _logger
        +GetById(string id) XElement
        +GetAll() IEnumerable~XElement~
        +Add(XElement entity) void
        +Update(XElement entity) void
        +Delete(string id) void
        +Exists(string id) bool
        +Count() int
        -ValidateEntity(XElement entity) bool
    }
    class YamlRepository {
        -string _filePath
        -ILogger _logger
        +GetById(string id) YNode
        +GetAll() IEnumerable~YNode~
        +Add(YNode entity) void
        +Update(YNode entity) void
        +Delete(string id) void
        +Exists(string id) bool
        +Count() int
        -ValidateEntity(YNode entity) bool
    }

    IRepository <|.. XmlRepository
    IRepository <|.. YamlRepository
```

*Commentaire : Ce diagramme illustre l'utilisation du pattern Repository pour l'accès aux données XML et YAML. Ajout des méthodes manquantes et des dépendances.*

### Pattern Factory

```mermaid
classDiagram
    class IParserFactory {
        <<interface>>
        +CreateParser(string type) IParser
        +CreateValidator(string type) IValidator
        +CreateGenerator(string type) IGenerator
    }
    class ParserFactory {
        -ILogger _logger
        +CreateParser(string type) IParser
        +CreateValidator(string type) IValidator
        +CreateGenerator(string type) IGenerator
        -CreateXmlParser() IParser
        -CreateYamlParser() IParser
        -CreateXmlValidator() IValidator
        -CreateYamlValidator() IValidator
    }
    class IParser {
        <<interface>>
        +Parse(string input) object
        +Validate(object input) bool
    }

    IParserFactory <|.. ParserFactory
    ParserFactory ..> IParser
    ParserFactory ..> IValidator
    ParserFactory ..> IGenerator
```

*Commentaire : Ce diagramme montre l'utilisation du pattern Factory pour la création des différents types de parsers. Ajout des méthodes manquantes et des dépendances.*

## Gestion des Erreurs

```mermaid
graph TD
    A[Erreur] --> B{Type d'Erreur}
    B -->|Validation| C[ValidationException]
    B -->|Parsing| D[ParsingException]
    B -->|Mapping| E[MappingException]
    B -->|Génération| F[GenerationException]
    B -->|Configuration| G[ConfigurationException]
    
    C --> H[Logger]
    D --> H
    E --> H
    F --> H
    G --> H
    
    H --> I[Console]
    H --> J[Fichier]
    H --> K[Autres cibles]
    
    L[ErrorHandler] --> B
    L --> H
```

*Commentaire : Ce diagramme illustre la hiérarchie des exceptions et leur gestion dans le système. Ajout des types d'erreurs manquants et du gestionnaire d'erreurs.*

## Configuration

```mermaid
graph LR
    A[appsettings.json] --> B[IConfiguration]
    C[Variables d'environnement] --> B
    D[Arguments CLI] --> B
    
    B --> E[AppConfiguration]
    E --> F[Services]
    
    F --> G[Logging]
    F --> H[Validation]
    F --> I[Parsing]
    F --> J[Generation]
    F --> K[ErrorHandling]
    
    L[ConfigurationValidator] --> E
```

*Commentaire : Ce diagramme montre comment la configuration est chargée et distribuée aux différents services. Ajout du validateur de configuration.*

## Logging

```mermaid
graph TD
    A[Logger] --> B{Level}
    B -->|Trace| C[Console]
    B -->|Debug| C
    B -->|Info| C
    B -->|Warning| C
    B -->|Error| C
    B -->|Critical| C
    
    C --> D[Format]
    D --> E[Timestamp]
    D --> F[Level]
    D --> G[Category]
    D --> H[Message]
    D --> I[Exception]
    D --> J[Context]
    
    K[LogManager] --> A
    K --> C
```

*Commentaire : Ce diagramme illustre la structure du système de logging avec ses différents niveaux et composants. Ajout des champs de log manquants et du gestionnaire de logs.*

## Validation

```mermaid
graph TD
    A[Validation] --> B{Type}
    B -->|XML| C[Schema Validation]
    B -->|YAML| D[Structure Validation]
    B -->|Data| E[Content Validation]
    B -->|Configuration| F[Config Validation]
    
    C --> G[Results]
    D --> G
    E --> G
    F --> G
    
    G --> H[Success]
    G --> I[Errors]
    G --> J[Warnings]
    G --> K[Details]
    
    L[ValidatorFactory] --> B
```

*Commentaire : Ce diagramme montre les différents types de validation et leurs résultats. Ajout des types de validation manquants et de la factory de validation.*

## Métadonnées

```mermaid
graph TD
    A[Métadonnées] --> B[Diagramme]
    A --> C[Entités]
    A --> D[Relations]
    A --> E[Styles]
    
    B --> F[Titre]
    B --> G[Auteur]
    B --> H[Date]
    B --> I[Version]
    
    C --> J[Type]
    C --> K[Propriétés]
    C --> L[Styles]
    
    D --> M[Source]
    D --> N[Cible]
    D --> O[Type]
    D --> P[Propriétés]
    
    E --> Q[Nom]
    E --> R[Valeurs]
    E --> S[Héritage]
```

*Commentaire : Ce diagramme illustre la structure des métadonnées extraites du diagramme. Ajout des types de métadonnées manquants et de leurs propriétés.*

## Flux de Conversion

```mermaid
stateDiagram-v2
    [*] --> Chargement
    Chargement --> ValidationXML
    ValidationXML --> Extraction
    Extraction --> Mapping
    Mapping --> GénérationYAML
    GénérationYAML --> ValidationYAML
    ValidationYAML --> [*]
    
    state ValidationXML {
        [*] --> VérificationStructure
        VérificationStructure --> VérificationTypes
        VérificationTypes --> VérificationContenu
        VérificationContenu --> [*]
    }
    
    state Extraction {
        [*] --> Éléments
        Éléments --> Connexions
        Connexions --> Styles
        Styles --> Métadonnées
        Métadonnées --> [*]
    }
    
    state Mapping {
        [*] --> Entités
        Entités --> Relations
        Relations --> Propriétés
        Propriétés --> Styles
        Styles --> [*]
    }
    
    state ValidationYAML {
        [*] --> Structure
        Structure --> Types
        Types --> Contenu
        Contenu --> [*]
    }
```

*Commentaire : Ce diagramme d'état montre le flux complet de conversion d'un diagramme DrawIO en YAML Threagile. Ajout des états manquants et des transitions.*

## Tests

```mermaid
graph TD
    A[Tests] --> B[Unitaires]
    A --> C[Intégration]
    A --> D[Performance]
    A --> E[Validation]
    
    B --> F[Parser]
    B --> G[Adapter]
    B --> H[Generator]
    B --> I[Validator]
    
    C --> J[Flux Complet]
    C --> K[Erreurs]
    C --> L[Configuration]
    
    D --> M[Gros Fichiers]
    D --> N[Concurrence]
    D --> O[Memory]
    
    E --> P[XML]
    E --> Q[YAML]
    E --> R[Data]
```

*Commentaire : Ce diagramme illustre la structure des tests avec leurs différents types et composants testés. Ajout des types de tests manquants et des composants à tester.* 
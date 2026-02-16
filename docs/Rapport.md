# ANALYSE TECHNIQUE DU PROJET EASYSAVE - RAPPORT COMPLET



## 1. CARTOGRAPHIE DU SYSTÈME

### 1.1 Structure Globale

Le projet suit une organisation en couches inspirée de la Clean Architecture :

```
EasySave/
├── Bootstrap/              # Point d'entrée et composition manuelle
├── Domain/Models/          # Entités métier (BackupJob, BackupJobState, etc.)
├── Application/            # Services applicatifs et interfaces
│   ├── Abstractions/      # 8 interfaces de services
│   └── Services/          # 8 implémentations concrètes
├── Infrastructure/         # Implémentations techniques
│   ├── Configuration/     # Configuration globale (Singleton)
│   ├── Persistence/       # JobRepository, StateFileService
│   ├── IO/               # JsonFile, PathService
│   └── Lang/             # Gestion i18n
└── Presentation/          # Interface utilisateur
    ├── Cli/              # Mode ligne de commande
    ├── Ui/               # Interface interactive (menu console)
    └── Resources/        # Ressources i18n (.resx)

EasyLog/                   # Projet séparé : système de logging
```

### 1.2 Flux de Données Principal

**Démarrage de l'application** :
1. `Program.cs` → charge `ApplicationConfiguration.Instance` (singleton global)
2. `Bootstrapper.BuildServices()` → composition manuelle de tous les services
3. `UserInterface.Initialize()` → initialisation des vues UI avec services injectés
4. `UserInterface.ShowMenu()` → boucle interactive principale

**Exécution d'un backup** :
1. Utilisateur sélectionne un job via `JobLaunchView`
2. `BackupService.RunJob(job)` orchestre l'ensemble :
   - Validation des chemins (`IPathService`)
   - Sélection des fichiers (`IBackupFileSelector`)
   - Création de la structure de répertoires (`IBackupDirectoryPreparer`)
   - Copie fichier par fichier (`IFileCopier`)
   - Mise à jour de l'état en temps réel (`IStateService`)
   - Logging de chaque opération (`ConfigurableLogWriter<LogEntry>`)
   - **Affichage de progression dans la console** (`ProgressWidget` - instancié dans le service!)

**Persistence** :
- **Jobs** : Fichier JSON (`jobs.json`) via `JobRepository`
- **État d'exécution** : Fichier JSON (`state.json`) via `StateFileService`
- **Logs** : Fichiers quotidiens JSON ou XML via `EasyLog`

### 1.3 Composants Critiques

#### BackupService (Application/Services/BackupService.cs)
**Rôle** : Orchestrateur principal de l'exécution des backups.  
**Responsabilités** : Validation, orchestration, logging, gestion d'état, affichage UI (!).  
**Problème** : Fait TOUT. God Object avec 221 lignes, 6 dépendances + violations de layering.

#### ApplicationConfiguration (Infrastructure/Configuration/ApplicationConfiguration.cs)
**Rôle** : Configuration globale de l'application.  
**Responsabilités** : Chargement/sauvegarde appsettings.json, singleton global.  
**Problème** : Singleton mutable accessible partout via `Instance`, modifie son propre fichier de config en runtime.

#### JobRepository (Infrastructure/Persistence/JobRepository.cs)
**Rôle** : Persistence des jobs de backup.  
**Responsabilités** : CRUD des jobs, assignation d'ID, limite à 5 jobs.  
**Problème** : API bizarre (passer la liste en paramètre), mélange logique métier et persistence.

#### UserInterface (Presentation/Ui/UserInterface.cs)
**Rôle** : Point d'entrée de l'UI interactive.  
**Responsabilités** : Initialisation des vues, affichage du menu principal.  
**Problème** : Singleton statique, Service Locator, instancie tout en dur.

---

## 2. PROBLÈMES IDENTIFIÉS

### 2.1 VIOLATIONS ARCHITECTURALES CRITIQUES

#### **CRITIQUE #1 : Inversion de dépendance Application → Presentation**

**Localisation** : `BackupService.cs` (Application/Services), lignes 3-4, 150

**Code incriminé** :
```csharp
using EasySave.Presentation.Ui;
using EasySave.Presentation.Ui.Console;

// ...ligne 150 dans RunJob():
var progressWidget = new ProgressWidget(new SystemConsole());
```

**Explication** : La couche Application dépend de la couche Presentation, ce qui **inverse complètement** le principe de Dependency Inversion (SOLID). Dans une Clean Architecture, l'Application ne doit JAMAIS connaître l'existence de la Presentation. Les flèches de dépendance doivent pointer vers l'intérieur (Domain), pas vers l'extérieur.

**Impact** :
- Impossible de tester `BackupService` sans UI
- Impossible de remplacer l'UI (ex: GUI, web) sans modifier Application
- Couplage bétonné entre logique métier et affichage
- Violation de l'Open/Closed Principle

**Gravité** : **CRITIQUE** - Bloque toute évolution de l'architecture

---

#### **CRITIQUE #2 : Service Locator Pattern - Singleton Global Mutable**

**Localisation** : `ApplicationConfiguration.cs` (Infrastructure/Configuration), lignes 11-34  
Utilisé dans : `ConfigurableLogWriter<T>`, `LogTypeView`, `LanguageView`, `LoggerService`, `LanguageService`

**Code incriminé** :
```csharp
public class ApplicationConfiguration
{
    private static ApplicationConfiguration? _instance;
    
    public static ApplicationConfiguration Instance
    {
        get
        {
            if (_instance == null)
                throw new InvalidOperationException("ApplicationConfiguration has not been loaded.");
            return _instance;
        }
    }
    
    // Properties avec side-effects :
    public string LogPath
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                Save(nameof(LogPath), value); // Modifie le fichier!
            }
        }
    } = "./log";
}
```

**Utilisation typique** :
```csharp
// Dans ConfigurableLogWriter<T>.cs
private ILogger<T> DetermineLogger()
{
    var cfg = ApplicationConfiguration.Instance; // Service Locator
    return cfg.LogType.ToLower() switch { /* ... */ };
}
```

**Explication** : Le Service Locator est un anti-pattern reconnu qui crée un **couplage global invisible**. Chaque classe qui utilise `ApplicationConfiguration.Instance` a une dépendance cachée non déclarée dans son constructeur. De plus, les setters modifient le fichier `appsettings.json` en runtime, ce qui est une **pratique non standard** (les fichiers de config doivent être read-only en production).

**Impact** :
- Dépendances cachées → code non testable
- Ordre d'initialisation fragile (exception si `Load()` non appelé avant)
- Impossible de mocker pour les tests
- Modification de fichiers de config en prod = risque de corruption/conflit
- État global partagé = cauchemar pour threading futur

**Gravité** : **CRITIQUE** - Mine la testabilité de toute la codebase

---

#### **CRITIQUE #3 : Repository Pattern Mal Implémenté**

**Localisation** : `IJobRepository.cs` (Application/Abstractions) + `JobRepository.cs` (Infrastructure/Persistence)

**Code incriminé** :
```csharp
public interface IJobRepository
{
    List<BackupJob> Load();
    void Save(List<BackupJob> jobs);
    (bool ok, string error) AddJob(List<BackupJob> jobs, BackupJob job);
    bool RemoveJob(List<BackupJob> jobs, string idOrName);
}
```

**Utilisation typique** :
```csharp
var jobs = _repository.Load();  // Charge depuis le disque
var (ok, error) = _repository.AddJob(jobs, newJob);  // Passe la liste + l'item
// Le repository modifie la liste ET persiste!
```

**Explication** : Cette API viole le principe "Tell, Don't Ask". Le repository ne devrait **pas** demander au client de lui passer la collection à modifier. Cela crée une confusion sur qui est responsable de l'état : l'appelant doit gérer une liste qu'il a chargée, puis la passer au repository qui la modifie et persiste. C'est un **leaky abstraction** qui expose les détails d'implémentation (fichier JSON = tout en mémoire).

**Design attendu** :
```csharp
public interface IJobRepository
{
    IEnumerable<BackupJob> GetAll();
    BackupJob GetById(int id);
    void Add(BackupJob job);
    void Remove(int id);
    // State géré en interne, pas exposé à l'appelant
}
```

**Impact** :
- API confuse et error-prone
- Responsabilités floues (qui gère la liste?)
- Impossible de changer l'implémentation (ex: base de données) sans casser l'API
- Force l'appelant à charger toute la collection à chaque fois

**Gravité** : **IMPORTANTE** - Complique significativement l'évolution

---

### 2.2 VIOLATIONS DES PRINCIPES SOLID

#### **VIOLATION SRP #1 : BackupService - God Method**

**Localisation** : `BackupService.cs`, méthode `RunJob()` lignes 65-220 (156 lignes)

**Responsabilités multiples** :
1. Validation des chemins source/target
2. Logging des erreurs de validation
3. Préparation de la structure de répertoires
4. Sélection des fichiers à copier
5. Calcul de la taille totale
6. Gestion de l'état d'exécution (8 updates de `jobState`)
7. Boucle de copie fichier par fichier
8. Gestion des erreurs de copie
9. Logging de chaque transfert
10. **Affichage de la barre de progression UI** (ligne 150, 197)
11. Finalisation et log de complétion

**Code symptomatique** :
```csharp
public void RunJob(BackupJob job)
{
    // 1. Validation
    var sourceOk = _paths.TryNormalizeExistingDirectory(...);
    if (!sourceOk) { /* log + update state */ }
    
    // 2-3. Preparation
    _directoryPreparer.EnsureTargetDirectories(...);
    var filesToCopy = _fileSelector.GetFilesToCopy(...);
    
    // 4. Update state
    jobState.State = JobRunState.Active;
    _state.Update(jobState);
    
    // 5. UI (!!)
    var progressWidget = new ProgressWidget(new SystemConsole());
    
    // 6. Boucle principale
    foreach (var sourceFile in filesToCopy)
    {
        // Update state again
        jobState.CurrentAction = "file_transfer";
        _state.Update(jobState);
        
        // Copy + log + update progress + update state
        // ...encore 40 lignes...
    }
    
    // 7. Cleanup + final state update
    jobState.State = hadError ? JobRunState.Failed : JobRunState.Completed;
    _state.Update(jobState);
}
```

**Duplication** : 4 appels quasi-identiques à `_logger.Log(new LogEntry { ... })` avec variations mineures.

**Impact** :
- Méthode impossible à tester unitairement (trop de dépendances)
- Impossible à réutiliser partiellement
- Modification risquée (beaucoup de responsabilités = beaucoup de raisons de changer)
- Difficile à comprendre (charge cognitive élevée)

**Gravité** : **IMPORTANTE** - Entrave maintenance et tests

---

#### **VIOLATION DIP #2 : Dépendances concrètes dans les constructeurs**

**Localisation** : Multiples fichiers

**Exemples** :
```csharp
// BackupService.cs - ligne 22
private readonly ConfigurableLogWriter<LogEntry> _logger;  // Classe concrète!

public BackupService(
    ConfigurableLogWriter<LogEntry> logger,  // Devrait être ILogger<LogEntry>
    // ...
)

// LogTypeView.cs - ligne 22
public LogTypeView(IConsole console)
{
    _loggerService = new LoggerService();  // new dans constructeur!
}

// MainMenuController.cs - ligne 49
new Option(..., new LanguageView(_console).Show),  // new dans initialisation
```

**Explication** : Le Dependency Inversion Principle stipule que les dépendances doivent pointer vers des **abstractions** (interfaces), pas des implémentations concrètes. Injecter `ConfigurableLogWriter<T>` au lieu d'une interface `ILogger<T>` crée un couplage fort. Instancier des dépendances avec `new` dans un constructeur ou une méthode viole complètement le principe d'inversion de contrôle.

**Impact** :
- Tests impossibles (pas de mock)
- Changement d'implémentation = modification de nombreux fichiers
- Couplage en cascade

**Gravité** : **IMPORTANTE** - Complique tests et modifications

---

### 2.3 OVER-ENGINEERING ET COMPLEXITÉ INUTILE

#### **OVER-ENGINEERING #1 : Abstraction Excessive Sans Justification**

**Localisation** : `Application/Abstractions/` - 8 interfaces pour des cas simples

**Exemples** :
```csharp
// IBackupDirectoryPreparer.cs
public interface IBackupDirectoryPreparer
{
    void EnsureTargetDirectories(BackupJob job, string sourceDir, string targetDir);
    void EnsureTargetDirectoryForFile(BackupJob job, string sourceFile, string targetFile);
}

// IFileCopier.cs
public interface IFileCopier
{
    long Copy(string sourceFile, string targetFile);
}
```

**Explication** : Ces interfaces n'ont **qu'une seule implémentation** et n'en auront probablement jamais d'autre (comment implémenter différemment la copie de fichiers?). C'est une application dogmatique du principe "Program to an interface, not an implementation" sans analyse du besoin réel. Cela crée une **indirection inutile** qui complique la navigation dans le code sans apporter de flexibilité.

**Justification manquante** : Pourquoi aurait-on besoin de plusieurs implémentations de `IFileCopier`? Quand est-ce que copier un fichier pourrait varier?

**Impact** :
- Navigation dans le code compliquée (clic sur interface → "Find implementations")
- Fichiers supplémentaires à maintenir
- Complexité conceptuelle accrue pour les nouveaux développeurs

**Gravité** : **MINEURE** - N'empêche pas le fonctionnement mais ralentit la compréhension

---

#### **OVER-ENGINEERING #2 : ConfigurableLogWriter - Complexité Pour Rien**

**Localisation** : `ConfigurableLogWriter<T>.cs` (Application/Services)

**Code** :
```csharp
public class ConfigurableLogWriter<T>
{
    private readonly object _sync = new();
    private ILogger<T> _logger;

    public ConfigurableLogWriter(string logDirectory)
    {
        _logger = DetermineLogger(logDirectory);
    }

    public void Log(T entry)
    {
        lock (_sync)  // Thread-safety... mais EasySave est single-threaded!
        {
            _logger.Log(entry);
        }
    }

    private ILogger<T> DetermineLogger(string logDirectory)
    {
        var cfg = ApplicationConfiguration.Instance;  // Service Locator
        return cfg.LogType.ToLower() switch
        {
            "json" => new JsonLogger<T>(logDirectory),
            "xml" => new XmlLogger<T>(logDirectory),
            _ => new JsonLogger<T>(logDirectory)
        };
    }
}
```

**Problèmes** :
1. **Lock inutile** : Le code commente "v1.0/v1.1 execute jobs sequentially in a single thread" mais ajoute du locking
2. **Service Locator** : Dépendance cachée à `ApplicationConfiguration.Instance`
3. **Pas d'interface** : Classe concrète injectée dans `BackupService`
4. **Réassignation impossible** : Comment changer de logger à runtime? Le field `_logger` n'est jamais réassigné

**Alternative simple** :
```csharp
// Injecter directement ILogger<T> choisi au démarrage
public BackupService(ILogger<LogEntry> logger, ...)
```

**Gravité** : **MINEURE** - Fonctionne mais ajoute complexité inutile

---

### 2.4 PROBLÈMES DE CODE ET BUGS POTENTIELS

#### **BUG #1 : Exception Avalée Sans Logging**

**Localisation** : `BackupService.cs`, ligne 172-176

**Code** :
```csharp
try
{
    var fi = new FileInfo(sourceFile);
    fileSize = fi.Length;
    elapsedMs = _fileCopier.Copy(sourceFile, targetFile);
}
catch (Exception)
{
    hadError = true;
    elapsedMs = -1;
}
```

**Explication** : Le catch attrape **toutes** les exceptions (`Exception`) sans logger le message d'erreur. Impossible de débugger un problème en production : pourquoi le fichier n'a pas été copié? IOException? UnauthorizedAccessException? OutOfMemoryException? Aucune information.

**Impact** : Diagnostic impossible en cas de problème

**Gravité** : **IMPORTANTE** - Complique significativement le debugging

---

#### **BUG #2 : Thread Safety Absente Dans AbstractLogger<T>**

**Localisation** : `EasyLog/AbstractLogger.cs`, ligne 26

**Code** :
```csharp
protected void WriteLogFile(T log)
{
    // ...
    File.AppendAllText(logFilePath, Serialize(log) + Environment.NewLine);
}
```

**Explication** : `File.AppendAllText()` n'est **pas thread-safe**. Si deux threads (ou processus) écrivent simultanément dans le même fichier de log, corruption possible. Bien que l'application soit actuellement single-threaded, le commentaire dans `StateFileService` mentionne "we do not need locking primitives" mais cela pourrait changer en v2.0.

**Impact** : Corruption de logs si évolution vers multi-threading

**Gravité** : **MINEURE** actuellement, **IMPORTANTE** si évolution prévue

---

#### **CODE SMELL #1 : Modèles Anémiques**

**Localisation** : Tous les modèles `Domain/Models/`

**Exemple** :
```csharp
public sealed class BackupJob
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SourceDirectory { get; set; } = string.Empty;
    public string TargetDirectory { get; set; } = string.Empty;
    public BackupType Type { get; set; }
}
```

**Explication** : Les entités Domain n'ont **aucune logique métier**, que des getters/setters publics. Tous les champs sont mutables. C'est un **Anemic Domain Model** anti-pattern : les objets ne font rien, toute la logique est dans les services. De plus, aucune validation : on peut créer un `BackupJob` avec `Name = ""` et `SourceDirectory = null`.

**Design attendu** :
```csharp
public sealed class BackupJob
{
    public int Id { get; private set; }
    public string Name { get; }
    public DirectoryPath SourceDirectory { get; }
    public DirectoryPath TargetDirectory { get; }
    public BackupType Type { get; }
    
    public BackupJob(string name, DirectoryPath source, DirectoryPath target, BackupType type)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        Name = name;
        SourceDirectory = source ?? throw new ArgumentNullException(nameof(source));
        TargetDirectory = target ?? throw new ArgumentNullException(nameof(target));
        Type = type;
    }
    
    public void AssignId(int id) { Id = id; }
}
```

**Impact** : Aucune garantie d'intégrité des données, validation dispersée dans les services

**Gravité** : **IMPORTANTE** - Complique validation et garanties métier

---

#### **CODE SMELL #2 : Duplication de Validation**

**Localisation** : `BackupService.cs`, `CommandJobRunner.cs`, `JobLaunchView.cs`

**Code dupliqué** :
```csharp
// Dans BackupService.cs
if (!_paths.TryNormalizeExistingDirectory(job.SourceDirectory, out _))
{
    jobState.State = JobRunState.Failed;
    jobState.CurrentAction = "source_missing";
    _state.Update(jobState);
    _logger.Log(/* ... */);
    return;
}

// Dans CommandJobRunner.cs (quasi identique)
if (!_paths.TryNormalizeExistingDirectory(job.SourceDirectory, out _))
{
    message = string.Format(UserInterface.Terminal_log_JobSourceNotFound, job.Id);
    return false;
}

// Dans JobLaunchView.cs (encore!)
if (!_paths.TryNormalizeExistingDirectory(j.SourceDirectory, out _))
{
    _console.WriteLine($"[{j.Id}] {Resources.UserInterface.Path_SourceNotFound}");
    continue;
}
```

**Explication** : La validation des chemins est répétée dans 3 endroits différents avec 3 comportements légèrement différents. C'est une violation du DRY (Don't Repeat Yourself). Si la logique de validation change, il faut modifier 3 fichiers.

**Impact** : Risque d'incohérence, maintenabilité réduite

**Gravité** : **MINEURE** - Amélioration souhaitable

---

### 2.5 MANQUE DE COHÉSION ET COUPLAGE EXCESSIF

#### **COUPLAGE #1 : Infrastructure Connaît Domain**

**Localisation** : `JobRepository.cs`, `StateFileService.cs`

**Code** :
```csharp
// JobRepository.cs dépend de Domain.Models
using EasySave.Domain.Models;

public sealed class JobRepository : IJobRepository
{
    public (bool ok, string error) AddJob(List<BackupJob> jobs, BackupJob job)
    {
        // Logique métier : limite 5 jobs, assignation d'ID
        if (jobs.Count >= MaxJobs)
            return (false, "Error.MaxJobs");
        
        var id = GetNextFreeId(jobs);
        job.Id = id;  // Modifie l'entité Domain!
        jobs.Add(job);
        Save(jobs);
    }
}
```

**Explication** : Le repository (Infrastructure) contient de la **logique métier** (limite de 5 jobs, assignation d'ID). Il modifie directement les entités Domain. Dans une Clean Architecture, Infrastructure ne devrait être qu'un détail d'implémentation technique (lecture/écriture disque), pas contenir de règles métier.

**Design attendu** : La règle "max 5 jobs" devrait être dans Domain ou Application, pas Infrastructure.

**Impact** : Mélange des responsabilités, difficulté à changer la persistence

**Gravité** : **IMPORTANTE** - Complexifie l'architecture

---

#### **COUPLAGE #2 : Presentation Appelle Directement Repository**

**Localisation** : Toutes les vues UI (JobCreationView, JobRemovalView, JobListView, etc.)

**Code** :
```csharp
public sealed class JobCreationView
{
    private readonly IJobRepository _repository;
    
    public void Show()
    {
        var jobs = _repository.Load();  // Presentation → Infrastructure !
        // ...
        var (ok, error) = _repository.AddJob(jobs, newJob);
    }
}
```

**Explication** : La couche Presentation accède directement au Repository (Infrastructure), en court-circuitant la couche Application. Cela viole la séparation des couches : Presentation devrait passer par des **cas d'usage** (Application) qui orchestrent les opérations.

**Design attendu** :
```
Presentation → Application (Use Cases) → Domain ← Infrastructure
```

**Impact** : Logique métier dispersée, difficile à réutiliser (ex: API web)

**Gravité** : **IMPORTANTE** - Viole l'architecture en couches

---

### 2.6 PROBLÈMES SPÉCIFIQUES À L'IA

#### **SYMPTÔME IA #1 : Documentation Excessive Mais Inutile**

**Localisation** : Partout

**Exemple** :
```csharp
/// <summary>
///     Builds the backup orchestrator.
/// </summary>
/// <param name="logger">Log writer.</param>
/// <param name="state">State management service.</param>
/// <param name="paths">Path service.</param>
/// <param name="fileSelector">File selector.</param>
/// <param name="directoryPreparer">Target directory preparer.</param>
/// <param name="fileCopier">File copier.</param>
public BackupService(
    ConfigurableLogWriter<LogEntry> logger,
    IStateService state,
    // ...
)
```

**Explication** : Les commentaires XML sont **verbeux** et ne disent rien que le code ne dit déjà. "Log writer" pour un paramètre nommé `logger` n'apporte aucune valeur. C'est typique de l'IA qui génère du XML Doc systématiquement sans réflexion sur l'utilité.

---

#### **SYMPTÔME IA #2 : Sur-utilisation d'Interfaces Sans Raison**

Déjà couvert en 2.3 (Over-engineering #1).

---

#### **SYMPTÔME IA #3 : Patterns Appliqués Dogmatiquement**

**Observation** : Le code tente d'implémenter Repository Pattern, Service Layer, Dependency Injection, mais **incorrectement** :
- Repository qui demande la liste en paramètre
- DI manuelle au lieu d'un conteneur IoC (alors que les packages MS sont là!)
- Interfaces partout même pour des classes qui n'en ont pas besoin

**Explication** : L'IA connaît les noms des patterns mais pas leur **intention** ni leur contexte d'application approprié.

---

## 3. SYNTHÈSE ET RECOMMANDATIONS

### 3.1 Résumé des Problèmes Majeurs (Par Ordre de Priorité)

| # | Problème | Gravité | Impact | Effort Fix |
|---|----------|---------|--------|------------|
| 1 | Service Locator (Singleton global) | **CRITIQUE** | Testabilité nulle, couplage global | Élevé |
| 2 | Application dépend de Presentation | **CRITIQUE** | Évolutivité bloquée | Moyen |
| 3 | Repository mal implémenté | **IMPORTANTE** | API confuse, rigidité | Moyen |
| 4 | BackupService God Method | **IMPORTANTE** | Maintenance difficile | Moyen |
| 5 | Modèles anémiques sans validation | **IMPORTANTE** | Intégrité données | Faible |
| 6 | Exceptions avalées sans log | **IMPORTANTE** | Debugging impossible | Faible |
| 7 | Couplage Infrastructure/Domain | **IMPORTANTE** | Architecture compromise | Moyen |
| 8 | Dépendances concrètes (pas d'IoC) | **IMPORTANTE** | Tests compliqués | Élevé |
| 9 | Configuration mutable runtime | MINEURE | Risque corruption | Faible |
| 10 | Over-engineering (interfaces inutiles) | MINEURE | Compréhension ralentie | Faible |

### 3.2 Impact Global sur la Qualité de la Codebase

**Testabilité** : **2/10**  
- Impossible de tester unitairement la plupart des classes (Singleton global, dépendances concrètes)
- Pas de tests existants détectés dans le projet

**Maintenabilité** : **3/10**  
- God Methods difficiles à modifier
- Duplication de code
- Couplage fort = changements en cascade

**Évolutivité** : **2/10**  
- Impossible d'ajouter une GUI sans refactoriser (couplage Presentation)
- Impossible de paralléliser (Singleton mutable, absence de thread-safety)
- Impossible de distribuer (tout est en mémoire)

**Lisibilité** : **5/10**  
- Documentation excessive mais peu utile
- Navigation compliquée (interfaces inutiles)
- Fichiers longs (BackupService 221 lignes)

**Robustesse** : **4/10**  
- Exceptions avalées
- Absence de validation Domain
- Configuration mutable = risque corruption

### 3.3 Axes de Refactorisation Prioritaires

#### **PRIORITÉ 1 : Éliminer le Service Locator**

**Objectif** : Remplacer `ApplicationConfiguration.Instance` par injection de dépendances.

**Actions** :
1. Créer une interface `IApplicationConfiguration` en lecture seule
2. Utiliser le conteneur IoC `Microsoft.Extensions.DependencyInjection` (packages déjà présents!)
3. Enregistrer tous les services dans `Bootstrapper` via le conteneur
4. Injecter `IApplicationConfiguration` dans les classes qui en ont besoin

**Bénéfice** : Testabilité restaurée, dépendances explicites.

---

#### **PRIORITÉ 2 : Inverser la Dépendance Presentation**

**Objectif** : Supprimer les `using EasySave.Presentation` de `BackupService`.

**Actions** :
1. Créer une interface `IProgressReporter` dans Application :
   ```csharp
   public interface IProgressReporter
   {
       void ReportProgress(double percentage);
   }
   ```
2. Injecter `IProgressReporter` dans `BackupService`
3. Implémenter dans Presentation : `ConsoleProgressReporter : IProgressReporter`

**Bénéfice** : Respect de la Clean Architecture, réutilisabilité du service.

---

#### **PRIORITÉ 3 : Refactoriser le Repository**

**Objectif** : API cohérente et responsabilités claires.

**Actions** :
1. Modifier l'interface :
   ```csharp
   public interface IJobRepository
   {
       IEnumerable<BackupJob> GetAll();
       void Add(BackupJob job);  // Gère l'ID en interne
       void Remove(int id);
   }
   ```
2. Déplacer la logique "max 5 jobs" dans un service Application ou Domain
3. Le repository ne fait que persist/load, pas de logique métier

**Bénéfice** : API claire, séparation des responsabilités.

---

#### **PRIORITÉ 4 : Découper BackupService**

**Objectif** : Méthode `RunJob` < 50 lignes.

**Actions** :
1. Extraire méthode `ValidateJob(BackupJob) : ValidationResult`
2. Extraire méthode `PrepareBackup(BackupJob, sourceDir, targetDir) : BackupPlan`
3. Extraire méthode `ExecuteBackup(BackupPlan) : BackupResult`
4. Extraire classe `BackupProgressTracker` pour gérer les updates d'état

**Bénéfice** : Code testable, compréhensible, maintenable.

---

#### **PRIORITÉ 5 : Ajouter Validation Domain**

**Objectif** : Entités immutables avec validation.

**Actions** :
1. Rendre les setters `private` ou supprimer
2. Valider dans les constructeurs
3. Utiliser Value Objects pour `SourceDirectory` / `TargetDirectory` (ex: `DirectoryPath`)

**Bénéfice** : Intégrité des données garantie par le Domain.


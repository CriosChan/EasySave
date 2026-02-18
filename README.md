# EasySave
![Version 1.0](https://img.shields.io/badge/Version-1.0-green) ![Project framework .NET](https://img.shields.io/badge/Project%20framework-.NET-purple)

[English version](#english)

EasySave est un outil de sauvegarde automatise.

## Installation
Telechargez le fichier zip disponible dans la partie Releases.

## Utilisation
```bash
EasySave.exe 1-3
EasySave.exe 1;3
```
Sans argument, EasySave demarre une interface graphique Avalonia basee sur MVVM.

## Architecture
Le code est organise autour d'une architecture MVVM:
- `Views`: interface graphique Avalonia.
- `ViewModels`: commandes et etat de l'UI.
- `Core`: modeles metier, contrats et validation.
- `Services`: orchestration des cas d'usage.
- `Data`: configuration, persistence et ecriture des logs.
- `Platform`: services systeme (IO, localisation).
- `Cli`: mode ligne de commande (compatible syntaxe existante).

## Diagramme UML
Le diagramme complet est disponible ici: `docs/uml/EasySave-full.puml`

Pour regenerer le diagramme:
```bash
dotnet run --project tools/UmlGenerator/UmlGenerator.csproj -c Debug
```

## Contribution
Voir le fichier [CONTRIBUTING.md](CONTRIBUTING.md).

## License
Tous droits reserves par l'entreprise ProSoft[^1].

# ENGLISH

EasySave is a backup automation tool.

## Installation
Download the zip file available in the Releases category.

## Usage

```bash
EasySave.exe 1-3
EasySave.exe 1;3
```
Without arguments, EasySave starts an Avalonia GUI using MVVM.

## Architecture
The codebase now follows an MVVM architecture:
- `Views`: Avalonia UI.
- `ViewModels`: UI state and commands.
- `Core`: domain models, contracts, and validation.
- `Services`: use-case orchestration.
- `Data`: configuration, persistence, and logging.
- `Platform`: system adapters (IO, localization).
- `Cli`: command-line mode (same syntax as before).

## UML Diagram
Full diagram: `docs/uml/EasySave-full.puml`

To regenerate:
```bash
dotnet run --project tools/UmlGenerator/UmlGenerator.csproj -c Debug
```

## Contribution
See [CONTRIBUTING.md](CONTRIBUTING.md).

## License
All rights reserved by the Prosoft[^1] company.

[^1]: Prosoft est une entreprise FICTIVE. / Prosoft is a FICTIVE company.

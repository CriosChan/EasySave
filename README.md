# EasySave
![Version 1.0](https://img.shields.io/badge/Version-1.0-green) ![Project framework .NET](https://img.shields.io/badge/Project%20framework-.NET-purple)

[English version](#ENGLISH)

EasySave est un outil de sauvegarde automatise.

## Installation
Telechargez le fichier zip disponible dans la partie Releases.

## Utilisation
```bash
EasySave.exe 1-3
EasySave.exe 1;3
```
EasySave propose aussi une interface console si aucun argument n'est donne.

## Architecture
Le code est organise en couches:
- Bootstrap: point d'entree et composition des dependances.
- Presentation: CLI et UI console.
- Application: orchestration des cas d'usage et services metier.
- Domain: modeles et enums.
- Infrastructure: configuration, IO, persistence, logging.

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
EasySave also have a Textual User Interface if no arguments are provided.

## Architecture
The codebase is layered:
- Bootstrap: entry point and dependency composition.
- Presentation: CLI and console UI.
- Application: use case orchestration and services.
- Domain: models and enums.
- Infrastructure: configuration, IO, persistence, logging.

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

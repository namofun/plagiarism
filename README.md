# Plagiarism Detection Module

Build Status: ![](https://www.travis-ci.org/namofun/plagiarism.svg?branch=master)

Core algorithms and ideas from [JPlag](https://github.com/jplag/jplag).

All rewritten in C# and ANTLR4.

### Backend

Role based design. Storage is for EFCore version and Rest is for Remote API server.

- Plag.Backend.Abstraction
- Plag.Backend.Generation
- Plag.Backend.Roles.Rest
- Plag.Backend.Roles.Storage

### Frontend

ANTLR language parser and algorithms.

- Plag.Frontend.Algorithm
- Plag.Frontend.Common
- Plag.Frontend.Cpp
- Plag.Frontend.Csharp
- Plag.Frontend.Java
- Plag.Frontend.Python

### Module

Used in [Project Substrate](https://github.com/namofun/uikit).

- SatelliteSite.PlagModule

### Usage

If you are interested in the usage, please refer to `src/Host`.
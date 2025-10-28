# Archivarius

> When JSON is not enough.

A modular serialization library for .NET. The repository contains the core serialization engine and optional packages for JSON integration, storage backends, and developer-facing extensions.

This solution is organized as a set of reusable class libraries (netstandard2.0) with an accompanying test project (net8.0, NUnit).


## Contents
- Overview
- Stack and supported frameworks
- Requirements
- Getting started (build, test, pack)
- Scripts and common commands
- Environment variables
- Project structure
- Entry points and packages
- Tests
- License


## Overview
Archivarius aims to provide a flexible serialization API with multiple components:
- Core serialization primitives and type serializers (Archivarius)
- Optional JSON backend built on top of System.Text.Json (Archivarius.Json)
- Storage abstractions and backends, including in-memory and compressed variants (Archivarius.Storage)
- Convenience extensions for the core API (Archivarius.Extensions)
- Internal utilities reused across packages (Archivarius.Tools)

For usage examples, see the tests under Archivarius.Tests.


## Stack and supported frameworks
- Language: C#
- Runtime: .NET (cross‑platform)
- Package manager and tooling: dotnet CLI / NuGet
- Test framework: NUnit
- Target frameworks:
  - Libraries: netstandard2.0 (usable from .NET Framework 4.6.1+, .NET Core 2.0+, .NET 5/6/7/8+)
  - Tests: net8.0


## Requirements
- .NET SDK 8.0 or newer (recommended) — required to build the entire solution including tests.
  - The libraries themselves target netstandard2.0 and can be consumed by older runtimes, but building tests requires the .NET 8 SDK.
- OS: Windows, macOS, or Linux


## Getting started
Clone the repository and restore/build:

- Restore packages
  dotnet restore Archivarius.sln

- Build all projects
  dotnet build Archivarius.sln -c Debug

- Run tests
  dotnet test Archivarius.sln -c Debug

- Pack individual libraries (NuGet packages)
  dotnet pack Archivarius/Archivarius.csproj -c Release -o ./nupkgs
  dotnet pack Archivarius.Json/Archivarius.Json.csproj -c Release -o ./nupkgs
  dotnet pack Archivarius.Storage/Archivarius.Storage.csproj -c Release -o ./nupkgs
  dotnet pack Archivarius.Extensions/Archivarius.Extensions.csproj -c Release -o ./nupkgs
  dotnet pack Archivarius.Tools/Archivarius.Tools.csproj -c Release -o ./nupkgs

Note: The solution contains no executable application; it is a set of libraries. Use the tests and your own applications to exercise the APIs.


## Scripts and common commands
There are no custom shell scripts in this repo; use standard dotnet CLI commands.

Common commands:
- Build: dotnet build Archivarius.sln -c Release
- Test with coverage (uses coverlet.collector):
  dotnet test Archivarius.sln -c Release --collect:"XPlat Code Coverage"
- Pack (NuGet): see the pack commands above for each project


## Environment variables
None required for build or tests.

- TODO: Document any runtime configuration or environment variables once public APIs requiring them are finalized.


## Project structure
Top‑level directories and purpose:
- Archivarius — Core serialization library
- Archivarius.Json — JSON backend (System.Text.Json)
- Archivarius.Storage — Storage abstractions/backends (e.g., in‑memory, compressed)
- Archivarius.Extensions — Optional extensions for the core library
- Archivarius.Tools — Internal utilities shared by other projects
- Archivarius.Tests — NUnit test project

Solution file:
- Archivarius.sln


## Entry points and packages
This repository provides reusable class libraries (no console or web entry points). Key packages/namespaces:
- Archivarius (core)
- Archivarius.Json
  - Package reference: System.Text.Json
- Archivarius.Storage
  - Package references: Ionic.Zlib.Core, Microsoft.Bcl.AsyncInterfaces
- Archivarius.Extensions
- Archivarius.Tools

Refer to the test project for examples of how to construct serializers and use storage backends.


## Tests
- Framework: NUnit (Microsoft.NET.Test.Sdk, NUnit3TestAdapter, NUnit.Analyzers)
- Target framework: net8.0
- Run all tests:
  dotnet test Archivarius.sln -c Debug
- Run a specific test class or filter (examples):
  dotnet test Archivarius.Tests/Archivarius.Tests.csproj --filter TestCategory=Unit
  dotnet test Archivarius.Tests/Archivarius.Tests.csproj --filter FullyQualifiedName~SomeNamespace.SomeTestClass


## License
This project is licensed under the GNU Lesser General Public License v3.0 (LGPL‑3.0). See the LICENSE file in the repository root for details.

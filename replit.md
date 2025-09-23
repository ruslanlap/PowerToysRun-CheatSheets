# Overview

This repository contains a collection of projects for creating PowerToys Run plugins, specifically featuring a CheatSheets plugin and dotnet templates for generating new plugins. The main focus is on the PowerToys Run plugin ecosystem, which extends Microsoft PowerToys' launcher functionality with custom search capabilities.

The primary components include:
- A CheatSheets plugin that provides quick access to programming cheat sheets from remote sources
- Dotnet templates (`Community.PowerToys.Run.Plugin.Templates`) for scaffolding new PowerToys Run plugins
- Build configurations for both x64 and ARM64 Windows architectures
- Publishing pipeline for distribution

## Recent Changes

**September 22, 2025** - Enhanced CheatSheets plugin with improved content filtering:
- Implemented robust HTML error page detection to prevent display of HTML tags in search results
- Added command syntax cleaning to convert confusing patterns ({{[-A|--all]}} → [-A|--all]) while preserving placeholder values ({{branch}})
- Improved content quality and readability of displayed commands

# User Preferences

Preferred communication style: Simple, everyday language.

# System Architecture

## Plugin Architecture
The repository follows the PowerToys Run plugin architecture pattern, where plugins implement the `IPlugin` interface and integrate with the PowerToys launcher system. Plugins are distributed as compiled .NET assemblies with accompanying metadata files.

### Core Plugin Structure
- **Plugin Entry Point**: Each plugin has a `Main.cs` file implementing the PowerToys plugin interfaces
- **Plugin Metadata**: `plugin.json` files define plugin properties like action keywords, display names, and executable information
- **Multi-Architecture Support**: Built for both x64 and ARM64 Windows platforms using .NET 9.0 with Windows-specific targeting

### Service Layer Design
The CheatSheets plugin uses a service-oriented architecture:
- **CacheService**: Wraps `System.Runtime.Caching.MemoryCache` for HTTP response caching with configurable expiration
- **CheatSheetService**: Core business logic for querying remote cheat sheet providers and normalizing results
- **Dependency Injection**: Services are instantiated and managed through constructor injection patterns

### Template System
The project includes a comprehensive template system for generating new PowerToys Run plugins:
- **Solution Templates**: `ptrun-sln` for creating complete plugin solutions
- **Project Templates**: `ptrun-proj` for individual plugin projects  
- **Script Templates**: `ptrun-scripts` for build and deployment automation

## Build System

### Multi-Target Compilation
The build system is configured to compile for multiple Windows architectures:
- **x64 Windows**: Primary target for most Windows systems
- **ARM64 Windows**: Support for ARM-based Windows devices
- **Framework Targeting**: Uses `net9.0-windows10.0.22621.0` for Windows-specific APIs

### Publishing Pipeline
- **Self-Contained Deployment**: Plugins are published with all dependencies included
- **Architecture-Specific Outputs**: Separate build outputs for x64 and ARM64 in dedicated folders
- **Asset Management**: Images and configuration files are automatically copied during build

# External Dependencies

## PowerToys Integration
- **Community.PowerToys.Run.Plugin.Dependencies (v0.93.0)**: Core PowerToys plugin framework providing base interfaces and common functionality
- **Wox Framework**: Legacy plugin system components (Wox.Plugin.dll, Wox.Infrastructure.dll) maintained for compatibility

## .NET Runtime Dependencies
- **System.Text.Json (v9.0.9)**: JSON serialization for API responses and configuration
- **System.Runtime.Caching**: Memory caching implementation for HTTP response optimization
- **Microsoft.Windows.SDK.NET.Ref (v10.0.22621.57)**: Windows SDK for platform-specific functionality

## Development Tools
- **NuGet Package Management**: Template distribution through NuGet packages
- **MSBuild Integration**: Custom build targets and properties for plugin compilation
- **GitHub Actions**: Automated builds and security scanning (referenced in README badges)

## Remote Data Sources
The CheatSheets plugin integrates with external cheat sheet providers (specific endpoints determined at runtime based on plugin configuration), implementing HTTP client patterns with caching and retry logic for reliability.
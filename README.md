# PowerToys Run: CheatSheets Plugin

<div align="center">
  <img src="assets/logo.png" alt="CheatSheets Icon" width="128" height="128">
  <h3>📚 Find cheat sheets and command examples instantly always at your fingertips with PowerToys Run plugin </h3>
  
  <!-- Badges -->
  <a href="https://github.com/ruslanlap/PowerToysRun-CheatSheets/actions/workflows/build-and-release.yml">
    <img src="https://github.com/ruslanlap/PowerToysRun-CheatSheets/actions/workflows/build-and-release.yml/badge.svg" alt="Build Status">
  </a>
  <a href="https://github.com/ruslanlap/PowerToysRun-CheatSheets/releases/latest">
    <img src="https://img.shields.io/github/v/release/ruslanlap/PowerToysRun-CheatSheets?label=latest" alt="Latest Release">
  </a>
  <img src="https://img.shields.io/maintenance/yes/2025" alt="Maintenance">
  <img src="https://img.shields.io/badge/C%23-.NET-512BD4" alt="C# .NET">
  <img src="https://img.shields.io/badge/version-v1.0.0-brightgreen" alt="Version">
  <img src="https://img.shields.io/badge/PRs-welcome-brightgreen.svg" alt="PRs Welcome">
  <a href="https://github.com/ruslanlap/PowerToysRun-CheatSheets/stargazers">
    <img src="https://img.shields.io/github/stars/ruslanlap/PowerToysRun-CheatSheets" alt="GitHub stars">
  </a>
  <a href="https://github.com/ruslanlap/PowerToysRun-CheatSheets/issues">
    <img src="https://img.shields.io/github/issues/ruslanlap/PowerToysRun-CheatSheets" alt="GitHub issues">
  </a>
  <a href="https://github.com/ruslanlap/PowerToysRun-CheatSheets/releases/latest">
    <img src="https://img.shields.io/github/downloads/ruslanlap/PowerToysRun-CheatSheets/total" alt="GitHub all releases">
  </a>
  <img src="https://img.shields.io/badge/Made%20with-❤️-red" alt="Made with Love">
  <img src="https://img.shields.io/badge/Awesome-Yes-orange" alt="Awesome">
  <a href="https://github.com/ruslanlap/PowerToysRun-CheatSheets/releases/latest">
    <img src="https://img.shields.io/github/v/release/ruslanlap/PowerToysRun-CheatSheets?style=for-the-badge" alt="Latest Release">
  </a>
  <img src="https://img.shields.io/badge/PowerToys-Compatible-blue" alt="PowerToys Compatible">
  <img src="https://img.shields.io/badge/platform-Windows-lightgrey" alt="Platform">
  <a href="https://opensource.org/licenses/MIT">
    <img src="https://img.shields.io/badge/License-MIT-yellow.svg" alt="License">
  </a>
  <a href="https://github.com/hlaueriksson/awesome-powertoys-run-plugins">
    <img src="https://awesome.re/mentioned-badge.svg" alt="Mentioned in Awesome PowerToys Run Plugins">
  </a>
</div>

<div align="center">
  <a href="https://github.com/ruslanlap/PowerToysRun-CheatSheets/releases/download/v1.0.0/CheatSheets-1.0.0-x64.zip">
    <img src="https://img.shields.io/badge/⬇️_DOWNLOAD-x64-blue?style=for-the-badge&logo=github" alt="Download x64">
  </a>
  <a href="https://github.com/ruslanlap/PowerToysRun-CheatSheets/releases/download/v1.0.0/CheatSheets-1.0.0-ARM64.zip">
    <img src="https://img.shields.io/badge/⬇️_DOWNLOAD-ARM64-blue?style=for-the-badge&logo=github" alt="Download ARM64">
  </a>
</div>

## 📊 Download Statistics

<div align="center">
  <img src="https://img.shields.io/github/downloads/ruslanlap/PowerToysRun-CheatSheets/total?style=for-the-badge&label=Total%20Downloads" height="42" alt="Total Downloads">
  <img src="https://img.shields.io/github/downloads/ruslanlap/PowerToysRun-CheatSheets/latest/total?style=for-the-badge&label=Latest%20Release" height="42"  alt="Latest Release Downloads">
</div>
  <div align="center">
    <img src="assets/demo3.png" alt="Demo: Settings" width="800">
    <p><em>CheatSheets Plugin Settings Demo</em></p>
  </div>

## 📋 Quick Links

- [Download latest release](https://github.com/ruslanlap/PowerToysRun-CheatSheets/releases/latest)
- [Installation guide](#-installation)
- [Usage tips](#-usage)
- [Release notes](#-whats-new-in-v100)
- [Report an issue](https://github.com/ruslanlap/PowerToysRun-CheatSheets/issues)

## 🧭 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Demo](#-demo)
- [Quick Start](#-quick-start)
- [Installation](#-installation)
- [Usage](#-usage)
- [Configuration Tips](#-configuration-tips)
- [Updating](#-updating)
- [What's New in v1.0.0](#-whats-new-in-v100)
- [Building from Source](#-building-from-source)
- [Development & Quality](#-development--quality)
- [Project Structure](#-project-structure)
- [FAQ](#-faq)
- [Troubleshooting](#-troubleshooting)
- [Security & Privacy](#-security--privacy)
- [Tech Stack](#-tech-stack)
- [Contributing](#-contributing)
- [Screenshots](#-screenshots)
- [Support the Project](#-support-the-project)
- [License](#-license)
- [Acknowledgements](#-acknowledgements)



## 📝 Overview

**CheatSheets** keeps command references at your fingertips directly inside PowerToys Run. Search across [tldr pages](https://tldr.sh/), [cheat.sh](https://cheat.sh/), [devhints.io](https://devhints.io/), and offline documentation with fuzzy matching, smart caching, and zero browser context switching. Launch PowerToys Run with `Alt+Space`, type `cs` (or your custom keyword), and jump straight to the command you need.

- **Plugin ID:** `41BF0604C51A4974A0BAA108826D0A94`
- **Action Keyword:** `cs` or change to `cheatsheet`
- **Platform:** Windows 10/11 (x64, ARM64)
- **Tech:** C#/.NET, WPF, PowerToys Run API

## ✨ Features
- 🔍 **Instant Search** - Find commands and cheat sheets with fuzzy matching
- 📚 **Multiple Sources** - Integrates with tldr, cheat.sh, and offline cheat sheets
- ⭐ **Favorites System** - Save and quickly access your most-used commands
- 📂 **Categories** - Browse commands by tool/language (git, docker, python, etc.)
- 📊 **Usage History** - Tracks popular commands for quick access
- 💾 **Smart Caching** - Fast offline access with configurable cache duration
- 🎨 **Modern UI** - Beautiful WPF interface with theme adaptation
- 🔧 **Offline Mode** - Works without internet connection using cached data
- ⚡ **Enhanced Performance** - Optimized search with background caching
- 🛠️ **Developer Hints** - Toggle advanced features and debug information

## 🎬 Demo
<div align="center">
  <img src="assets/demo-cheatsheets.gif" alt="CheatSheets Plugin Demo" width="800">
  <p><em>CheatSheets Plugin Demo GIF</em></p>
</div>

## 🚀 Quick Start

1. Download the latest `CheatSheets-<version>-<arch>.zip` from the [releases](https://github.com/ruslanlap/PowerToysRun-CheatSheets/releases/latest) page.
2. Extract the archive into `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\CheatSheets` (create the folder if it does not exist).
3. Restart PowerToys, open PowerToys Run with `Alt+Space`, and type `cs docker` (or any query) to verify search results appear.

## ⚡ Installation

### Prerequisites
- Windows 10/11
- PowerToys installed and running

### Steps
1. Download the ZIP that matches your CPU architecture:
   - [CheatSheets-1.0.0-x64.zip](https://github.com/ruslanlap/PowerToysRun-CheatSheets/releases/download/v1.0.0/CheatSheets-1.0.0-x64.zip)
   - [CheatSheets-1.0.0-ARM64.zip](https://github.com/ruslanlap/PowerToysRun-CheatSheets/releases/download/v1.0.0/CheatSheets-1.0.0-ARM64.zip)
2. Extract the ZIP contents into:
   ```
   %LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\CheatSheets
   ```
   Ensure the folder contains files such as `Community.PowerToys.Run.Plugin.CheatSheets.dll` and `plugin.json`.
3. Restart PowerToys completely:
   - Right-click PowerToys in the system tray → **Exit**
   - Launch PowerToys again from the Start menu
4. Confirm installation:
   - Open PowerToys Run with `Alt+Space`
   - Type `cs docker` or any term to see cheat sheet results
   - Use the context menu to add a result to favorites as an extra sanity check

## 🚀 Usage     
- Open PowerToys Run (`Alt+Space`)
- Type `cs` followed by your search term (e.g., `cs git commit`)
- Use special commands:
  - `cs:popular` - View trending/popular commands
  - `cs:fav` - Access your favorite commands
  - `cs:git` - Browse git-related commands
  - `cs:docker` - Browse docker commands
  - `cs:python` - Browse python commands
- Press `Enter` to copy command to clipboard
- Right-click results for context menu options (favorites, etc.)
- Configure settings in PowerToys settings

## ⚙️ Configuration Tips

- Adjust the **Action Keyword** (`cs` by default) in PowerToys → `PowerToys Run` → `Plugins` → `CheatSheets` to suit your workflow.
- Tweak cache duration, preferred data sources, and offline sync from the plugin settings page for faster, offline-first lookups.
- Manage favorites and history directly within the plugin settings if you want to clear or export your curated list.

## 🔄 Updating

1. Download the newest release ZIP for your architecture.
2. Close PowerToys.
3. Delete the old `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\CheatSheets` folder to avoid stale files.
4. Extract the updated ZIP to the same location and restart PowerToys.
5. Verify the plugin version in PowerToys settings matches the release notes.

## 📢 What's New in v1.0.0

- **🔍 Fuzzy Search** - Improved search with fuzzy matching for better results
- **⭐ Favorites System** - Save and organize your most-used commands
- **📂 Categories** - Browse commands by tool and programming language
- **💾 Smart Caching** - Background caching with configurable duration
- **📱 Offline Mode** - Full functionality without internet connection
- **📊 Usage Tracking** - Learn from your habits with usage history
- **🎨 Modern UI** - Enhanced interface with theme adaptation

## 🛠️ Building from Source

### Prerequisites
- .NET 9.0 SDK and Windows 10/11
- Visual Studio 2022 (or newer) with `.NET desktop development` workload
- PowerToys installed locally for manual testing

### Visual Studio workflow
1. Clone the repository and open `Templates.sln`.
2. Set the startup project to `Community.PowerToys.Run.Plugin.CheatSheets`.
3. Choose the desired architecture (**x64** or **ARM64**) and build in **Release** mode.
4. Published binaries are placed under `CheatSheets/Community.PowerToys.Run.Plugin.CheatSheets/bin/<ARCH>/Release/net9.0-windows10.0.22621.0/<runtime>/publish`.

### Command-line workflow

Use the helper script for reproducible builds and packaging:

```
./build-and-zip.sh
```

Resulting ZIPs (`CheatSheets-<version>-x64.zip`, `CheatSheets-<version>-arm64.zip`) are created in the repository root.

## 🧪 Development & Quality

- Run unit tests with `dotnet test CheatSheets/Community.PowerToys.Run.Plugin.CheatSheets.UnitTests/` before submitting changes.
- Execute `./build-and-zip.sh` to verify multi-architecture packages and SQLite dependencies.
- Use `./ptrun-lint.sh` to catch common packaging issues flagged by `ptrun-lint` before publishing.
- Keep screenshots and GIFs in `assets/` up to date so documentation reflects the current UI.

## 📊 Project Structure
```
PowerToysRun-CheatSheets/
├── .github/                                    # GitHub Actions workflows
├── assets/                                     # Demo assets, screenshots, and logo
│   ├── logo.png                               # Plugin logo
│   ├── demo1.png                              # Screenshot 1
│   ├── demo2.png                              # Screenshot 2
│   └── demo-cheatsheets.mp4                   # Video demo
├── CheatSheets/                               # Plugin source code
│   ├── Community.PowerToys.Run.Plugin.CheatSheets/  # Main plugin
│   └── Community.PowerToys.Run.Plugin.CheatSheets.UnitTests/  # Unit tests
├── Templates.sln                              # Solution file for templates
├── build-and-zip.sh                           # Build script
├── ptrun-lint.sh                              # Linting script
├── CLAUDE.md                                  # Development guide
├── LICENSE                                    # MIT License
└── README.md                                  # This file
```

## ❓ FAQ
<details>
<summary><b>How do I add a command to favorites?</b></summary>
<p>Right-click on any search result and select "Add to Favorites" from the context menu.</p>
</details>
<details>
<summary><b>Where are my favorites stored?</b></summary>
<p>Favorites are stored locally in your PowerToys settings directory.</p>
</details>
<details>
<summary><b>How do I clear the cache?</b></summary>
<p>Go to PowerToys settings, find the CheatSheets plugin section, and use the cache management options.</p>
</details>
<details>
<summary><b>Does it work offline?</b></summary>
<p>Yes! The plugin caches cheat sheets and works fully offline after the initial setup.</p>
</details>
<details>
<summary><b>How do I browse by category?</b></summary>
<p>Use commands like 'cs:git', 'cs:docker', 'cs:python' to browse specific tool categories.</p>
</details>

## 🛠️ Troubleshooting

- **Plugin does not appear in PowerToys Run**  
  Confirm the folder path matches `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\CheatSheets` and that `plugin.json` exists inside.
- **Search results not showing**  
  Check internet access for first-time sync, then use the settings page to refresh cache if needed.
- **Icons or assets look outdated**  
  Remove the old plugin folder before copying the new version to avoid stale images.
- **Favorites not saving**  
  Ensure PowerToys runs with enough permissions to write to its settings directory (no read-only drives or roaming profile restrictions).
- **Unexpected errors in logs**  
  Capture `PowerToys Run` logs, open an [issue](https://github.com/ruslanlap/PowerToysRun-CheatSheets/issues), and include reproduction steps plus the output of `./build-and-zip.sh` if relevant.

## 🔒 Security & Privacy

- The plugin caches cheat sheet data locally for offline access
- No personal data is collected or transmitted
- Favorites and usage history stored locally only
- Integrates with public cheat sheet services (tldr, cheat.sh)
- No third-party tracking or analytics

## 🧑‍💻 Tech Stack

- C# / .NET 9.0
- WPF (UI)
- PowerToys Run API
- GitHub Actions (CI/CD)
- JSON for settings storage
- Fuzzy search algorithms

## 🤝 Contributing
Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md) before submitting a pull request.

### Contributors
- [ruslanlap](https://github.com/ruslanlap) - Project creator and maintainer

## 📸 Screenshots
<div align="center">
  <img src="assets/demo1.png" width="800" height="400" alt="Demo: CheatSheets Plugin Interface">
  <img src="assets/demo2.png" width="800" height="400" alt="Demo: Search Results">
</div>

## ☕ Support the Project
Enjoying CheatSheets? ☕ Buy me a coffee to support development:

[![Buy me a coffee](https://img.shields.io/badge/Buy%20me%20a%20coffee-☕️-FFDD00?style=for-the-badge&logo=buy-me-a-coffee)](https://ruslanlap.github.io/ruslanlap_buymeacoffe/)

## 📄 License
MIT License. See [LICENSE](LICENSE).

## 🙏 Acknowledgements
- [Microsoft PowerToys](https://github.com/microsoft/PowerToys) team
- [tldr](https://tldr.sh/) project
- [cheat.sh](https://cheat.sh/) service
- [devhints.io](https://devhints.io/)

<div align="center">
  <img src="assets/demo4.png" width="800" height="400" alt="Demo">
</div>


---

<div align="center">
  <sub>Made with ❤️ by <a href="https://github.com/ruslanlap">ruslanlap</a></sub>
</div>
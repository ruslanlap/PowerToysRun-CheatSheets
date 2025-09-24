# Contributing

## �� Code of Conduct
This project follows the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). By contributing, you agree to a welcoming community. Report issues to maintainers via GitHub.

## 🧰 Prerequisites
- .NET 9.0 SDK
- Windows 10/11
- Visual Studio 2022 or dotnet CLI
- PowerToys (latest)
- Git

## 🚀 Getting Started
1. Fork and clone the repo.
2. Run `dotnet restore` or open `Templates.sln`.
3. Test locally in PowerToys Run.

## 🧪 Development
- Branch from `main`.
- Focus on one change per PR.
- Follow C# style; use `dotnet format`.
- Update assets for UI changes.

## ✅ Testing
- Run unit tests: `dotnet test CheatSheets/Community.PowerToys.Run.Plugin.CheatSheets.UnitTests/`
- Manual test in PowerToys Run.

## 📤 How to Create a PR (Easy Steps)
1. **Create branch**: `git checkout -b feature/my-change`
2. **Make changes** and test them.
3. **Commit**: `git add . && git commit -m "Describe change"`
4. **Push**: `git push origin feature/my-change`
5. **Open PR**: Go to GitHub, compare & pull request from your branch to `main`.
   - Reference issues (e.g., `Fixes #123`).
   - Describe changes with screenshots if UI-related.
   - Ensure tests pass and attach build logs.
6. **Review**: Respond to feedback and update as needed.

## 🐞 Issues & Requests
- Search existing issues first.
- Provide steps to reproduce, expected vs. actual, Windows version, plugin version.
- Attach logs/screenshots.

## 🙌 Thanks
We appreciate your help! Happy contributing.

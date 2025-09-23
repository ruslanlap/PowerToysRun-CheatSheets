#!/bin/bash
set -e

# Встановлення .NET 9.0
if command -v dotnet &> /dev/null; then
    echo ".NET вже встановлено (версія: $(dotnet --version))"
else
    echo "Встановлення .NET 9.0..."
    ./dotnet-install.sh --channel 9.0
fi

# Налаштування змінних оточення для вашої локальної інсталяції .NET
echo "Налаштування змінних оточення..."
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$PATH:$HOME/.dotnet:$HOME/.dotnet/tools"

# Додаткові налаштування для середовища CI (наприклад, GitHub Actions runner)
if [[ -d "/home/runner/.dotnet" ]]; then
    export PATH="$PATH:/home/runner/.dotnet"
fi

# Перевірка поточної версії .NET
echo "Поточна версія .NET:"
dotnet --version

# Встановлення PowerToys Run Plugin Lint
if dotnet tool list -g | grep -q "Community.PowerToys.Run.Plugin.Lint"; then
    echo "Community.PowerToys.Run.Plugin.Lint вже встановлено"
else
    echo "Встановлення Community.PowerToys.Run.Plugin.Lint..."
    dotnet tool install -g Community.PowerToys.Run.Plugin.Lint
fi

# Ще раз вказуємо хосту .NET на ваш користувацький шлях
export DOTNET_ROOT="$HOME/.dotnet"
# Переконуємося, що папка інструментів у PATH
export PATH="$PATH:$HOME/.dotnet/tools"

# Запуск ptrun-lint
echo "Запуск ptrun-lint для PowerToysRun-Hotkeys..."
ptrun-lint https://github.com/ruslanlap/PowerToysRun-Hotkeys

echo "Готово!"

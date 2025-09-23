#!/bin/bash
set -euo pipefail

# ── Settings ────────────────────────────────────────────────────────────────────
PROJECT_PATH="CheatSheets/Community.PowerToys.Run.Plugin.CheatSheets/Community.PowerToys.Run.Plugin.CheatSheets.csproj"
PLUGIN_NAME="CheatSheets"
TFM="net9.0-windows10.0.22621.0"   # має відповідати <TargetFramework> у .csproj

# Отримати версію з plugin.json
VERSION=$(grep '"Version"' CheatSheets/Community.PowerToys.Run.Plugin.CheatSheets/plugin.json | sed 's/.*"Version": "\([^"]*\)".*/\1/')

# Залежності, які дає сам PowerToys і які можна викидати з пакету плагіна
# НЕ видаляємо SQLite-залежності!
DEPENDENCIES_TO_EXCLUDE="PowerToys.Common.UI.* PowerToys.ManagedCommon.* PowerToys.Settings.UI.Lib.* Wox.Infrastructure.* Wox.Plugin.*"

echo "📋 Plugin: $PLUGIN_NAME"
echo "📋 Version: $VERSION"

# ── Clean ───────────────────────────────────────────────────────────────────────
rm -rf ./CheatSheets/Publish
rm -rf ./CheatSheets/Community.PowerToys.Run.Plugin.CheatSheets/obj
rm -rf ./CheatSheets/Community.PowerToys.Run.Plugin.CheatSheets/bin
rm -f  ./CheatSheets-*.zip

# ── Build (копіюємо всі транзитивні DLL у publish/) ────────────────────────────
echo "🛠️  Building for x64..."
dotnet publish "$PROJECT_PATH" -c Release -r win-x64 --self-contained false \
    -p:CopyLocalLockFileAssemblies=true \
    -p:CopyLocalRuntimeTargetAssets=true \
    -p:Platform=x64

echo "🛠️  Building for ARM64..."
dotnet publish "$PROJECT_PATH" -c Release -r win-arm64 --self-contained false \
    -p:CopyLocalLockFileAssemblies=true \
    -p:CopyLocalRuntimeTargetAssets=true \
    -p:Platform=ARM64

# ── Paths ──────────────────────────────────────────────────────────────────────
PUBLISH_X64="./CheatSheets/Community.PowerToys.Run.Plugin.CheatSheets/bin/x64/Release/${TFM}/win-x64/publish"
PUBLISH_ARM64="./CheatSheets/Community.PowerToys.Run.Plugin.CheatSheets/bin/ARM64/Release/${TFM}/win-arm64/publish"

DEST_X64="./CheatSheets/Publish/x64"
DEST_ARM64="./CheatSheets/Publish/arm64"

# Get absolute paths for ZIP files to avoid path issues
SCRIPT_DIR="$(pwd)"
ZIP_X64="${SCRIPT_DIR}/${PLUGIN_NAME}-${VERSION}-x64.zip"
ZIP_ARM64="${SCRIPT_DIR}/${PLUGIN_NAME}-${VERSION}-arm64.zip"

# ── Package function ───────────────────────────────────────────────────────────
package_arch () {
  local SRC_DIR="$1"
  local DEST_DIR="$2"
  local ZIP_PATH="$3"
  local RUNTIME_HINT="$4"   # win-x64 | win-arm64

  echo "📦 Packaging ${RUNTIME_HINT}..."
  rm -rf "$DEST_DIR"
  mkdir -p "$DEST_DIR"

  # Копіюємо все з publish
  cp -r "$SRC_DIR"/* "$DEST_DIR/"

  # Видаляємо зайві залежності, які надає PowerToys (НЕ чіпаємо SQLite!)
  for dep in $DEPENDENCIES_TO_EXCLUDE; do
      find "$DEST_DIR" -name "$dep" -delete 2>/dev/null || true
  done

  # Переконуємося, що SQLite native бібліотеки на місці
  echo "🔍 Checking SQLite dependencies..."

  # Перевіряємо основні SQLite DLL
  SQLITE_CORE_FOUND=false
  SQLITE_NATIVE_FOUND=false

  if [ -f "$DEST_DIR/Microsoft.Data.Sqlite.dll" ] || [ -f "$DEST_DIR/Microsoft.Data.Sqlite.Core.dll" ]; then
    SQLITE_CORE_FOUND=true
    echo "✅ SQLite managed libraries found"
  fi

  if [ -f "$DEST_DIR/runtimes/${RUNTIME_HINT}/native/e_sqlite3.dll" ] || [ -f "$DEST_DIR/e_sqlite3.dll" ]; then
    SQLITE_NATIVE_FOUND=true
    echo "✅ SQLite native library found"
  fi

  # Додаткова перевірка в можливих місцях
  if [ ! "$SQLITE_NATIVE_FOUND" = true ]; then
    # Шукаємо native бібліотеки в різних місцях
    find "$DEST_DIR" -name "e_sqlite3.dll" -o -name "*sqlite*.dll" | while read -r file; do
      echo "📍 Found SQLite file: $file"
    done

    # Якщо не знайдено, спробуємо скопіювати з NuGet cache
    if [ -d "$HOME/.nuget/packages/sqlitepclraw.lib.e_sqlite3" ]; then
      echo "🔧 Attempting to copy SQLite native from NuGet cache..."
      NUGET_NATIVE_PATH="$HOME/.nuget/packages/sqlitepclraw.lib.e_sqlite3/*/runtimes/${RUNTIME_HINT}/native/e_sqlite3.dll"
      for file in $NUGET_NATIVE_PATH; do
        if [ -f "$file" ]; then
          mkdir -p "$DEST_DIR/runtimes/${RUNTIME_HINT}/native"
          cp "$file" "$DEST_DIR/runtimes/${RUNTIME_HINT}/native/"
          cp "$file" "$DEST_DIR/"
          echo "✅ Copied SQLite native from: $file"
          SQLITE_NATIVE_FOUND=true
          break
        fi
      done
    fi
  fi

  # Попередження якщо критичні компоненти не знайдено
  if [ ! "$SQLITE_CORE_FOUND" = true ]; then
    echo "⚠️  WARNING: SQLite managed libraries not found in ${DEST_DIR}"
    echo "    The plugin may fail with 'Could not load Microsoft.Data.Sqlite' error"
  fi

  if [ ! "$SQLITE_NATIVE_FOUND" = true ]; then
    echo "⚠️  WARNING: SQLite native library (e_sqlite3.dll) not found in ${DEST_DIR}"
    echo "    The plugin may fail with SQLite initialization errors"
  fi

  # Виводимо структуру файлів для діагностики
  echo "📁 Package contents:"
  find "$DEST_DIR" -name "*.dll" | head -20

  # Створюємо ZIP - change to destination directory to avoid path issues
  echo "🗜️  Creating ZIP archive..."
  (cd "$DEST_DIR" && zip -r "$ZIP_PATH" .)

  # Verify ZIP was created
  if [ -f "$ZIP_PATH" ]; then
    echo "✅ Created $(basename "$ZIP_PATH")"

    # Додаткова інформація про розмір пакету
    ZIP_SIZE=$(du -h "$ZIP_PATH" | cut -f1)
    echo "📦 Package size: $ZIP_SIZE"
  else
    echo "❌ Failed to create $(basename "$ZIP_PATH")"
    return 1
  fi
}

# ── Package x64 & arm64 ────────────────────────────────────────────────────────
package_arch "$PUBLISH_X64"  "$DEST_X64"  "$ZIP_X64"  "win-x64"
package_arch "$PUBLISH_ARM64" "$DEST_ARM64" "$ZIP_ARM64" "win-arm64"

# ── Post-build validation ──────────────────────────────────────────────────────
echo ""
echo "🔍 Post-build validation:"

validate_package() {
  local ZIP_PATH="$1"
  local RUNTIME="$2"

  echo "Validating $(basename "$ZIP_PATH")..."

  # Check if ZIP file exists first
  if [ ! -f "$ZIP_PATH" ]; then
    echo "❌ ZIP file not found: $ZIP_PATH"
    return 1
  fi

  # Створюємо тимчасову директорію для перевірки
  TEMP_DIR=$(mktemp -d)
  unzip -q "$ZIP_PATH" -d "$TEMP_DIR"

  # Перевіряємо критичні файли
  VALIDATION_PASSED=true

  if [ ! -f "$TEMP_DIR/Community.PowerToys.Run.Plugin.CheatSheets.dll" ]; then
    echo "❌ Main plugin DLL missing"
    VALIDATION_PASSED=false
  fi

  if [ ! -f "$TEMP_DIR/plugin.json" ]; then
    echo "❌ plugin.json missing"
    VALIDATION_PASSED=false
  fi

  # Перевіряємо SQLite
  SQLITE_FILES_FOUND=0
  if [ -f "$TEMP_DIR/Microsoft.Data.Sqlite.dll" ] || [ -f "$TEMP_DIR/Microsoft.Data.Sqlite.Core.dll" ]; then
    ((SQLITE_FILES_FOUND++))
  fi

  if [ -f "$TEMP_DIR/e_sqlite3.dll" ] || [ -f "$TEMP_DIR/runtimes/$RUNTIME/native/e_sqlite3.dll" ]; then
    ((SQLITE_FILES_FOUND++))
  fi

  if [ $SQLITE_FILES_FOUND -lt 2 ]; then
    echo "⚠️  SQLite components may be incomplete ($SQLITE_FILES_FOUND/2 found)"
  else
    echo "✅ SQLite components present"
  fi

  # Очищаємо тимчасову директорію
  rm -rf "$TEMP_DIR"

  if [ "$VALIDATION_PASSED" = true ]; then
    echo "✅ $(basename "$ZIP_PATH") validation passed"
  else
    echo "❌ $(basename "$ZIP_PATH") validation failed"
  fi
}

validate_package "$ZIP_X64" "win-x64"
validate_package "$ZIP_ARM64" "win-arm64"

# ── Summary ────────────────────────────────────────────────────────────────────
echo ""
echo "🎉 Build completed successfully!"
echo "✅ Created packages:"
if [ -f "$ZIP_X64" ]; then
  echo " - $(basename "$ZIP_X64")"
else
  echo " - ❌ $(basename "$ZIP_X64") (FAILED)"
fi

if [ -f "$ZIP_ARM64" ]; then
  echo " - $(basename "$ZIP_ARM64")"
else
  echo " - ❌ $(basename "$ZIP_ARM64") (FAILED)"
fi

# Виводимо checksums для верифікації
if command -v sha256sum >/dev/null 2>&1; then
  echo ""
  echo "🔐 SHA256 Checksums:"
  [ -f "$ZIP_X64" ] && echo "x64:   $(sha256sum "$ZIP_X64"  | cut -d' ' -f1)"
  [ -f "$ZIP_ARM64" ] && echo "arm64: $(sha256sum "$ZIP_ARM64" | cut -d' ' -f1)"
fi

echo ""
echo "📋 Installation instructions:"
echo "1. Close PowerToys"
echo "2. Extract the ZIP to: %LOCALAPPDATA%\\Microsoft\\PowerToys\\PowerToys Run\\Plugins\\CheatSheets"
echo "3. Start PowerToys"
echo "4. Test with: '++ test' to verify SQLite connectivity"
# pack-release.ps1 — Builds the mod and creates a ready-to-distribute ZIP
# containing both the mod DLL and the required BepInEx framework files.
#
# Usage:  .\pack-release.ps1
# Output: bin\Release\MineMogulMultiplayer-vX.Y.Z.zip
#
# The ZIP has this structure (user extracts into game root):
#   MineMogul/
#     winhttp.dll
#     doorstop_config.ini
#     .doorstop_version
#     BepInEx/
#       core/          ← all framework DLLs
#       config/        ← default BepInEx.cfg
#       plugins/
#         MineMogulMultiplayer/
#           MineMogulMultiplayer.dll

param(
    [string]$GameDir = "C:\Program Files (x86)\Steam\steamapps\common\MineMogul",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"

# 1. Build
Write-Host "Building $Configuration..." -ForegroundColor Cyan
dotnet build --configuration $Configuration
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

# 2. Read version from PluginInfo.cs
$versionLine = Select-String -Path "Core\PluginInfo.cs" -Pattern 'Version\s*=\s*"([^"]+)"'
$version = $versionLine.Matches[0].Groups[1].Value
Write-Host "Version: $version" -ForegroundColor Green

# 3. Prepare staging directory
$stageDir = "bin\$Configuration\stage"
if (Test-Path $stageDir) { Remove-Item $stageDir -Recurse -Force }
New-Item -ItemType Directory -Path $stageDir | Out-Null

# 4. Copy doorstop loader files (game root)
Copy-Item "$GameDir\winhttp.dll"          "$stageDir\winhttp.dll"
Copy-Item "$GameDir\doorstop_config.ini"  "$stageDir\doorstop_config.ini"
Copy-Item "$GameDir\.doorstop_version"    "$stageDir\.doorstop_version"

# 5. Copy BepInEx core
$coreStage = "$stageDir\BepInEx\core"
New-Item -ItemType Directory -Path $coreStage | Out-Null
Copy-Item "$GameDir\BepInEx\core\*" -Destination $coreStage -Recurse -Exclude "*.xml"

# 6. Copy BepInEx default config (without user-specific mod config)
$cfgStage = "$stageDir\BepInEx\config"
New-Item -ItemType Directory -Path $cfgStage | Out-Null
Copy-Item "$GameDir\BepInEx\config\BepInEx.cfg" "$cfgStage\BepInEx.cfg"

# 7. Create empty required directories
New-Item -ItemType Directory -Path "$stageDir\BepInEx\plugins\MineMogulMultiplayer" | Out-Null
New-Item -ItemType Directory -Path "$stageDir\BepInEx\patchers" | Out-Null

# 8. Copy all mod DLLs (mod + dependencies like Steamworks, MessagePack, System.*)
Copy-Item "$GameDir\BepInEx\plugins\MineMogulMultiplayer\*.dll" `
    "$stageDir\BepInEx\plugins\MineMogulMultiplayer\" -Force
# Overwrite mod DLL with freshly built version
Copy-Item "bin\$Configuration\net472\MineMogulMultiplayer.dll" `
    "$stageDir\BepInEx\plugins\MineMogulMultiplayer\MineMogulMultiplayer.dll" -Force

# 9. Create ZIP
$zipPath = "bin\$Configuration\MineMogulMultiplayer-v$version.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path "$stageDir\*" -DestinationPath $zipPath -CompressionLevel Optimal
Write-Host "Created: $zipPath" -ForegroundColor Green

# 10. Also output the standalone DLL for the GitHub release asset
$dllPath = "bin\$Configuration\net472\MineMogulMultiplayer.dll"
Write-Host "Standalone DLL: $dllPath" -ForegroundColor Green

# Cleanup staging
Remove-Item $stageDir -Recurse -Force

Write-Host "`nRelease artifacts ready!" -ForegroundColor Cyan
Write-Host "  Full bundle (new users): $zipPath" -ForegroundColor White
Write-Host "  DLL only (existing users): $dllPath" -ForegroundColor White

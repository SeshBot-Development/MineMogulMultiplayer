<#
.SYNOPSIS
    Build the mod, bump the version, and publish a GitHub Release.

.DESCRIPTION
    1. Reads the current version from Core/PluginInfo.cs
    2. Increments the patch number (0.1.0 → 0.1.1) unless you pass -Version
    3. Updates PluginInfo.cs with the new version
    4. Builds Release
    5. Creates the mod zip
    6. Creates a GitHub Release on SeshBot-Development/MineMogulMultiplayer
       with the zip attached

.PARAMETER Version
    Explicit version string (e.g. "0.2.0"). If omitted, auto-increments the patch.

.EXAMPLE
    .\publish-release.ps1            # auto-bump patch
    .\publish-release.ps1 -Version 0.2.0   # explicit version
#>
param(
    [string]$Version
)

$ErrorActionPreference = 'Stop'

$repoOwner = 'SeshBot-Development'
$repoName  = 'MineMogulMultiplayer'
$srcDir    = $PSScriptRoot
$projDir   = Join-Path $srcDir 'src\MineMogulMultiplayer'
$buildOut  = Join-Path $srcDir 'build_out'
$zipPath   = Join-Path $srcDir 'MineMogulMultiplayer-mod.zip'
$pluginInfo = Join-Path $projDir 'Core\PluginInfo.cs'

# ── 1. Read current version ──────────────────────────────────────────
$content = Get-Content $pluginInfo -Raw
$currentMatch = [regex]::Match($content, 'Version\s*=\s*"(\d+\.\d+\.\d+)"')
if (-not $currentMatch.Success) { throw "Could not find version in $pluginInfo" }
$current = [version]$currentMatch.Groups[1].Value
Write-Host "Current version: $current" -ForegroundColor Cyan

# ── 2. Determine new version ─────────────────────────────────────────
if ($Version) {
    $newVer = [version]$Version
} else {
    $newVer = [version]"$($current.Major).$($current.Minor).$($current.Build + 1)"
}
Write-Host "New version:     $newVer" -ForegroundColor Green

# ── 3. Update PluginInfo.cs ──────────────────────────────────────────
$updated = $content -replace 'Version\s*=\s*"\d+\.\d+\.\d+"', "Version = ""$newVer"""
Set-Content $pluginInfo $updated -NoNewline
Write-Host "Updated $pluginInfo"

# ── 4. Build ─────────────────────────────────────────────────────────
Push-Location $projDir
try {
    dotnet build -c Release -o $buildOut
    if ($LASTEXITCODE -ne 0) { throw 'Build failed' }
} finally { Pop-Location }

# ── 5. Create zip ────────────────────────────────────────────────────
Compress-Archive -Path "$buildOut\*" -DestinationPath $zipPath -Force
Write-Host "Created $zipPath"

# ── 6. Create GitHub Release ─────────────────────────────────────────
$tag = "v$newVer"
Write-Host "Creating release $tag on $repoOwner/$repoName ..." -ForegroundColor Yellow
gh release create $tag $zipPath `
    --repo "$repoOwner/$repoName" `
    --title "$tag" `
    --notes "Auto-update release $tag" `
    --latest

Write-Host "`nDone! Release $tag published." -ForegroundColor Green
Write-Host "Players with the mod will auto-update on next launch."

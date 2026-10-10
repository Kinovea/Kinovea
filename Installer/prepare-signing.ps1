
# This script should be run from the repo root.
# After building and after compiling the installer with option /DEXPORT_UNINST.
# This script will prepare a ZIP file containing all files that need to be signed, including the uninstaller.

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$buildDir = Join-Path $root "Kinovea/bin/x64/Release"
$uninstaller = Join-Path $root "Installer/Uninstall-Kinovea.exe"

$staging = Join-Path $root "signing-input"
$archive = Join-Path $root "Kinovea-Signing.zip"

# Start with a clean staging directory
if (Test-Path $staging) {
    Remove-Item $staging -Recurse -Force
}

New-Item -ItemType Directory -Path $staging | Out-Null

# Main executable
Copy-Item (Join-Path $buildDir "Kinovea.exe") $staging

# All Kinovea-owned DLLs in the root output directory
Get-ChildItem $buildDir -File -Filter "Kinovea.*.dll" | Copy-Item -Destination $staging

# Language satellite assemblies, preserving relative paths
$languages = Join-Path $buildDir "Languages"

if (Test-Path $languages) {
    Get-ChildItem $languages -Recurse -File -Filter "Kinovea*.resources.dll" |
        ForEach-Object {
            $relative = $_.FullName.Substring($buildDir.Length).TrimStart('\', '/')
            $target = Join-Path $staging $relative
            $targetDir = Split-Path $target -Parent

            New-Item -ItemType Directory -Path $targetDir -Force |
                Out-Null

            Copy-Item $_.FullName $target
        }
}

# Exported NSIS uninstaller
Copy-Item $uninstaller $staging

# Create ZIP with files directly at its root
if (Test-Path $archive) {
    Remove-Item $archive -Force
}

Compress-Archive -Path "$staging/*" -DestinationPath $archive

Write-Host "Signing package created: $archive"

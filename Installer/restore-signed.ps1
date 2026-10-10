$buildDir = "Kinovea/bin/x64/Release"
$signedDir = "signing-output"

# Restore signed Kinovea binaries and satellite assemblies
Get-ChildItem $signedDir -Recurse -File |
    Where-Object { $_.Name -ne "Uninstall-Kinovea.exe" } |
    ForEach-Object {
        $relative = $_.FullName.Substring(
            (Resolve-Path $signedDir).Path.Length
        ).TrimStart('\', '/')

        $destination = Join-Path $buildDir $relative
        Copy-Item $_.FullName $destination -Force
    }

# Restore signed NSIS uninstaller
Copy-Item "$signedDir/Uninstall-Kinovea.exe" `
    "Installer/Uninstall-Kinovea.exe" -Force
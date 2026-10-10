$buildDir = "Kinovea\bin\x64\Release"

$files = @(
    Get-Item "$buildDir\Kinovea.exe"
    Get-ChildItem "$buildDir\Kinovea.*.dll"
    Get-ChildItem "$buildDir\Languages" -Recurse -Filter "Kinovea*.resources.dll"
    Get-Item "Installer\Uninstall-Kinovea.exe"
)

$results = foreach ($file in $files) {
    $sig = Get-AuthenticodeSignature -LiteralPath $file.FullName

    [PSCustomObject]@{
        File   = $file.Name
        Status = $sig.Status
        Signer = $sig.SignerCertificate.Subject
        Timestamped = ($null -ne $sig.TimeStamperCertificate)
    }
}

$results | Format-Table -AutoSize

$invalid = @($results | Where-Object { $_.Status -ne "Valid" })

if ($invalid.Count -gt 0) {
    throw "$($invalid.Count) Kinovea files have invalid or missing signatures."
}

$archive = Join-Path $env:TEMP "ffmpeg-8.1.2-full_build-shared.7z"
$url = "https://www.kinovea.org/packages/ffmpeg/8.1.2/ffmpeg-8.1.2-full_build-shared.7z"
$expectedHash = "cba748035c21ce1431d0823c7a3a711f38616f89f87a265dceddf9b7f6749d2d"
$destination = Join-Path $PSScriptRoot "FFmpeg"

Invoke-WebRequest $url -OutFile $archive

$actualHash = (Get-FileHash $archive -Algorithm SHA256).Hash

if ($actualHash -ne $expectedHash) {
    throw "FFmpeg archive checksum mismatch"
}

New-Item -ItemType Directory -Force -Path $destination | Out-Null

& 7z x $archive "-o$destination" -y


if ($LASTEXITCODE -ne 0) {
    throw "7-Zip extraction failed with exit code $LASTEXITCODE."
}
REM Rebuild and generate the installer.
makensis /DPORTABLE /DREBUILD /DX64 kinovea.nsi > build-portable-x64.txt

REM Run the installer, it will extract itself locally.
forfiles /p "." /m Kinovea-Portable-*-x64.exe /c "@file"

REM Zip the content.
cd Kinovea-0.9.1-x64
7z a -r ../Kinovea-0.9.1-x64.zip *

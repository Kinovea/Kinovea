REM Rebuild and generate the installer.
makensis /DPORTABLE /DREBUILD /DX64 kinovea.nsi > build-portable.txt

REM Run the installer, it will extract itself locally.
REM forfiles /p "." /m Kinovea-Portable-*-x64.exe /c "@file"
Kinovea-Portable-2023.1.0.exe

REM Delete the installer.
del Kinovea-Portable-2023.1.0.exe /q

REM Zip the content.
cd Kinovea-2023.1.0
7z a -r ../Kinovea-2023.1.zip *



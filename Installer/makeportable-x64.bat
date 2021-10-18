REM Rebuild and generate the installer.
makensis /DPORTABLE /DREBUILD /DX64 kinovea.nsi > build-portable-x64.txt

REM Run the installer, it will extract itself locally.
REM forfiles /p "." /m Kinovea-Portable-*-x64.exe /c "@file"
Kinovea-Portable-0.9.6-x64.exe

REM Delete the installer.
del Kinovea-Portable-0.9.6-x64.exe /q

REM Zip the content.
cd Kinovea-0.9.6-x64
7z a -r ../Kinovea-0.9.6-x64.zip *



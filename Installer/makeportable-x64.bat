makensis /DPORTABLE /DREBUILD /DX64 kinovea.nsi > build-portable-x64.txt
forfiles /p "." /m Kinovea-Portable-*-x64.exe /c "@file"

;------------------------------------------------
;Kinovea Installer
;------------------------------------------------

!verbose 4
!include "MUI.nsh"

;General
    Name "Kinovea"
    SetCompressor /SOLID lzma

    !define VERSION "2025.1.0"
    !define EXTRADIR "OtherFiles"
    !define LOCALIZATIONDIR "..\Localization\WorkingDirectory\"

    !define BUILDDIR "..\Kinovea\Bin\x64\Release"
    InstallDir "$PROGRAMFILES64\Kinovea"

    ; Rebuild
    !ifdef REBUILD
        !ifdef X86
            !system 'devenv "D:\dev\video\kinovea\Source\Kinovea2\Kinovea.VS2019.sln" /clean "Release|x86" /project "Kinovea\Kinovea.csproj" /projectconfig "Release|x86"' = 0
            !system 'devenv "D:\dev\video\kinovea\Source\Kinovea2\Kinovea.VS2019.sln" /rebuild "Release|x86" /project "Kinovea\Kinovea.csproj" /projectconfig "Release|x86"' = 0
         !else
            !system 'devenv "D:\dev\video\kinovea\Source\Kinovea2\Kinovea.VS2019.sln" /clean "Release|x64" /project "Kinovea\Kinovea.csproj" /projectconfig "Release|x64"' = 0
            !system 'devenv "D:\dev\video\kinovea\Source\Kinovea2\Kinovea.VS2019.sln" /rebuild "Release|x64" /project "Kinovea\Kinovea.csproj" /projectconfig "Release|x64"' = 0
        !endif
    !endif

    !ifdef PORTABLE
        OutFile "Kinovea-Portable-${VERSION}.exe"
        RequestExecutionLevel user
    !else
        OutFile "Kinovea-${VERSION}.exe"
        RequestExecutionLevel admin
        InstallDirRegKey HKCU "Software\Kinovea" "InstallDirectory" ;Install dir stored in registry for previous install.
        BrandingText " "
    !endif

;--------------------------------
;Variables
;--------------------------------
    Var MUI_TEMP
    Var STARTMENU_FOLDER

;--------------------------------
;Interface Configuration
;--------------------------------
    ;Icons
    !define MUI_ICON "graphics\install.ico"
    !define MUI_UNICON "graphics\uninstall.ico"

    ;Image on the header of the page. (150x57 pixels)
    !define MUI_HEADERIMAGE
    !define MUI_HEADERIMAGE_BITMAP "graphics\150x57.bmp"

    ;Bitmap for the Welcome page and the Finish page (164x314 pixels)
    !define MUI_WELCOMEFINISHPAGE_BITMAP "graphics\164x314.bmp"
    !define MUI_UNWELCOMEFINISHPAGE_BITMAP "graphics\164x314.bmp"

    ;Show a message box with a warning when the user wants to close the installer.
    !define MUI_ABORTWARNING

;--------------------------------------
;Language Selection Dialog Settings
;--------------------------------------
  ;Remember the installer language
  !define MUI_LANGDLL_REGISTRY_ROOT "HKCU"
  !define MUI_LANGDLL_REGISTRY_KEY "Software\Kinovea"
  !define MUI_LANGDLL_REGISTRY_VALUENAME "Installer Language"

;----------------------------
;Pages configuration
;----------------------------
    ;Start Menu Folder
    !define MUI_STARTMENUPAGE_REGISTRY_ROOT "HKCU"
    !define MUI_STARTMENUPAGE_REGISTRY_KEY "Software\Kinovea"
    !define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "Start Menu Folder"
    ;Finish
    !define MUI_FINISHPAGE_RUN "$INSTDIR\Kinovea.exe"

    !ifndef PORTABLE
    ;--------------------------------
    ;Pages
    ;--------------------------------
        ;Installer
        !insertmacro MUI_PAGE_WELCOME
        !insertmacro MUI_PAGE_LICENSE "${EXTRADIR}\GPLv2.txt"
        !insertmacro MUI_PAGE_DIRECTORY
        !insertmacro MUI_PAGE_STARTMENU Application $STARTMENU_FOLDER
        !insertmacro MUI_PAGE_INSTFILES
        !insertmacro MUI_PAGE_FINISH

        ;Uninstaller
        !insertmacro MUI_UNPAGE_CONFIRM
        !insertmacro MUI_UNPAGE_INSTFILES
        !insertmacro MUI_UNPAGE_FINISH
    !endif

;--------------------------------
;Languages
;--------------------------------
    !insertmacro MUI_LANGUAGE "English" ;first language is the default language
    !insertmacro MUI_LANGUAGE "French"
    !insertmacro MUI_LANGUAGE "German"
    !insertmacro MUI_LANGUAGE "Spanish"
    ;!insertmacro MUI_LANGUAGE "SpanishInternational"
    !insertmacro MUI_LANGUAGE "SimpChinese"
    ;!insertmacro MUI_LANGUAGE "TradChinese"
    !insertmacro MUI_LANGUAGE "Japanese"
    !insertmacro MUI_LANGUAGE "Korean"
    !insertmacro MUI_LANGUAGE "Italian"
    !insertmacro MUI_LANGUAGE "Dutch"
    !insertmacro MUI_LANGUAGE "Danish"
    !insertmacro MUI_LANGUAGE "Swedish"
    !insertmacro MUI_LANGUAGE "Norwegian"
    ;!insertmacro MUI_LANGUAGE "NorwegianNynorsk"
    !insertmacro MUI_LANGUAGE "Finnish"
    !insertmacro MUI_LANGUAGE "Greek"
    !insertmacro MUI_LANGUAGE "Russian"
    !insertmacro MUI_LANGUAGE "Portuguese"
    !insertmacro MUI_LANGUAGE "PortugueseBR"
    !insertmacro MUI_LANGUAGE "Polish"
    ;!insertmacro MUI_LANGUAGE "Ukrainian"
    !insertmacro MUI_LANGUAGE "Czech"
    ;!insertmacro MUI_LANGUAGE "Slovak"
    !insertmacro MUI_LANGUAGE "Croatian"
    !insertmacro MUI_LANGUAGE "Bulgarian"
    !insertmacro MUI_LANGUAGE "Hungarian"
    !insertmacro MUI_LANGUAGE "Thai"
    !insertmacro MUI_LANGUAGE "Romanian"
    ;!insertmacro MUI_LANGUAGE "Latvian"
    !insertmacro MUI_LANGUAGE "Macedonian"
    ;!insertmacro MUI_LANGUAGE "Estonian"
    !insertmacro MUI_LANGUAGE "Turkish"
    !insertmacro MUI_LANGUAGE "Lithuanian"
    ;!insertmacro MUI_LANGUAGE "Slovenian"
    !insertmacro MUI_LANGUAGE "Serbian"
    !insertmacro MUI_LANGUAGE "SerbianLatin"
    !insertmacro MUI_LANGUAGE "Arabic"
    !insertmacro MUI_LANGUAGE "Farsi"
    ;!insertmacro MUI_LANGUAGE "Hebrew"
    !insertmacro MUI_LANGUAGE "Indonesian"
    ;!insertmacro MUI_LANGUAGE "Mongolian"
    ;!insertmacro MUI_LANGUAGE "Luxembourgish"
    ;!insertmacro MUI_LANGUAGE "Albanian"
    ;!insertmacro MUI_LANGUAGE "Breton"
    ;!insertmacro MUI_LANGUAGE "Belarusian"
    !insertmacro MUI_LANGUAGE "Icelandic"
    ;!insertmacro MUI_LANGUAGE "Malay"
    ;!insertmacro MUI_LANGUAGE "Bosnian"
    ;!insertmacro MUI_LANGUAGE "Kurdish"
    ;!insertmacro MUI_LANGUAGE "Irish"
    ;!insertmacro MUI_LANGUAGE "Uzbek"
    ;!insertmacro MUI_LANGUAGE "Galician"
    ;!insertmacro MUI_LANGUAGE "Afrikaans"
    !insertmacro MUI_LANGUAGE "Catalan"

;--------------------------------
;Properties of the installer file
	VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "Kinovea"
	VIAddVersionKey /LANG=${LANG_ENGLISH} "Comments" "Video Analysis"
	VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "Joan Charmant"
	VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "Copyright © 2006-2023 Joan Charmant"
	VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "Kinovea Installer"
	VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "${VERSION}"
	VIProductVersion "${VERSION}.0"
	CRCCheck on

;--------------------------------
;Reserve Files
    ;If you are using solid compression, files that are required before
    ;the actual installation should be stored first in the data block,
    ;because this will make your installer start faster.

    !insertmacro MUI_RESERVEFILE_LANGDLL

;--------------------------------
;Installer Sections

    ;In a common installer there are several things the user can install.
    ;For example in the NSIS distribution installer you can choose to install the source code, additional plug-ins, examples and more.
    ;Each of these components has its own piece of code.
    ;If the user selects to install this component, then the installer will execute that code.
    ;In the script, that code is defined in sections.
    ;Each section corresponds to one component in the components page.
    ;The section's name is the displayed component name, and the section code will be executed if that component is selected.
    ;It is possible to build your installer with only one section, but if you want to use the components page and let the user choose what to install, you'll have to use more than one section.

    !macro CopyDirectoryContent SourceDir TargetDir SubDir Extension
        CreateDirectory "${TargetDir}\${SubDir}"
        SetOutPath "${TargetDir}\${SubDir}"
        File /r "${SourceDir}\${SubDir}\*.${Extension}"
    !macroend



;Main installer section.
;TODO: terminate app.
Section ""

    !ifdef PORTABLE
        System::Call "kernel32::GetCurrentDirectory(i ${NSIS_MAX_STRLEN}, t .r0)"
        StrCpy $INSTDIR "$0\Kinovea-${VERSION}"
    !endif

    ; Main directory
    SetOutPath "$INSTDIR"
    File "${BUILDDIR}\Kinovea.exe"
    File "${BUILDDIR}\Kinovea.exe.config"
    File "${BUILDDIR}\*.dll"
    File "${BUILDDIR}\*.manifest"
    File "${EXTRADIR}\GPLv2.txt"
    File "${EXTRADIR}\License.txt"
    File "${EXTRADIR}\Readme.txt"

    ; TODO - special version for portable.
    !ifdef PORTABLE
        File /oname=LogConf.xml "${EXTRADIR}\LogConf-portable.xml"
    !else
        File "${EXTRADIR}\LogConf.xml"
    !endif

    ; Sub directories.
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "ar" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "bg" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "ca" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "cs" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "da" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "de" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "el" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "es" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "fa" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "fi" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "fr" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "hr" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "id" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "it" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "ja" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "ko" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "lt" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "mk" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "ms" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "nl" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "no" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "pl" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "pt" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "ro" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "ru" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "sr-Cyrl-RS" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "sr-Latn-RS" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "sv" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "th" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "tr" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "zh-CHS" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "zh-Hant" "dll"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "xslt" "xsl"
    !insertmacro CopyDirectoryContent "${BUILDDIR}" "$INSTDIR" "DrawingTools" "xml"

    !ifdef PORTABLE
        CreateDirectory "$INSTDIR\AppData"
    !endif

    SetOutPath "$INSTDIR"

    !ifndef PORTABLE
        ;Store installation folder
        WriteRegStr HKCU "Software\Kinovea" "InstallDirectory" $INSTDIR

        ;Create uninstaller
        WriteUninstaller "$INSTDIR\Uninstall.exe"

        ;Register uninstaller to Windows.
        !define AppRemovePath "Software\Microsoft\Windows\CurrentVersion\Uninstall\Kinovea"
        WriteRegStr HKLM "${AppRemovePath}" "DisplayName" "Kinovea"
        WriteRegStr HKLM "${AppRemovePath}" "DisplayVersion" "${VERSION}"
        WriteRegDWORD HKLM "${AppRemovePath}" "NoModify" "1"
        WriteRegDWORD HKLM "${AppRemovePath}" "NoRepair" "1"
        WriteRegStr HKLM "${AppRemovePath}" "Publisher" "Joan Charmant"
        WriteRegStr HKLM "${AppRemovePath}" "UninstallString" "$\"$INSTDIR\Uninstall.exe$\""

        ;Create shortcuts under all users.
        !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
        SetShellVarContext all
        CreateDirectory "$SMPROGRAMS\$STARTMENU_FOLDER"
        CreateShortcut "$SMPROGRAMS\$STARTMENU_FOLDER\Kinovea.lnk" "$INSTDIR\Kinovea.exe"
        CreateShortCut "$SMPROGRAMS\$STARTMENU_FOLDER\UninstallKinovea.lnk" "$INSTDIR\Uninstall.exe"
        !insertmacro MUI_STARTMENU_WRITE_END
    !endif
SectionEnd

;----------------------------------------------------------------------------------------------------------------------
;Installer Functions
Function .onInit
  !ifdef PORTABLE
    SetSilent silent ; Suppress output for portable install
  !else
    !insertmacro MUI_LANGDLL_DISPLAY
  !endif
FunctionEnd

!ifndef PORTABLE
    ;----------------------------------------------------------------------------------------------------------------------
    ;Uninstaller Section

        !macro RemoveDirectory TargetDir
            Delete "${TargetDir}\*.*"
            RMDir "${TargetDir}"
        !macroend

    Section "Uninstall"
        ; We delete the files installed by older versions.
        ; We also delete generated files not created at installation (Prefs, ColorProfile)

        Delete "$INSTDIR\*.*"
        !insertmacro RemoveDirectory "$INSTDIR\xslt"
        !insertmacro RemoveDirectory "$INSTDIR\guides"              ; prior to 2025.1
        !insertmacro RemoveDirectory "$INSTDIR\DrawingTools\Custom"
        !insertmacro RemoveDirectory "$INSTDIR\DrawingTools\Standard"
        !insertmacro RemoveDirectory "$INSTDIR\DrawingTools\Toolbars"
        !insertmacro RemoveDirectory "$INSTDIR\DrawingTools"
        !insertmacro RemoveDirectory "$INSTDIR\ar"
        !insertmacro RemoveDirectory "$INSTDIR\bg"
        !insertmacro RemoveDirectory "$INSTDIR\ca"
        !insertmacro RemoveDirectory "$INSTDIR\cs"
        !insertmacro RemoveDirectory "$INSTDIR\da"
        !insertmacro RemoveDirectory "$INSTDIR\de"
        !insertmacro RemoveDirectory "$INSTDIR\el"
        !insertmacro RemoveDirectory "$INSTDIR\es"
        !insertmacro RemoveDirectory "$INSTDIR\fa"
        !insertmacro RemoveDirectory "$INSTDIR\fi"
        !insertmacro RemoveDirectory "$INSTDIR\fr"
        !insertmacro RemoveDirectory "$INSTDIR\hr"
        !insertmacro RemoveDirectory "$INSTDIR\it"
        !insertmacro RemoveDirectory "$INSTDIR\ja"
        !insertmacro RemoveDirectory "$INSTDIR\ko"
        !insertmacro RemoveDirectory "$INSTDIR\lt"
        !insertmacro RemoveDirectory "$INSTDIR\mk"
        !insertmacro RemoveDirectory "$INSTDIR\ms"
        !insertmacro RemoveDirectory "$INSTDIR\nl"
        !insertmacro RemoveDirectory "$INSTDIR\no"
        !insertmacro RemoveDirectory "$INSTDIR\pl"
        !insertmacro RemoveDirectory "$INSTDIR\pt"
        !insertmacro RemoveDirectory "$INSTDIR\ro"
        !insertmacro RemoveDirectory "$INSTDIR\ru"
        !insertmacro RemoveDirectory "$INSTDIR\sr-Cyrl-CS"
        !insertmacro RemoveDirectory "$INSTDIR\sr-Cyrl-RS"
        !insertmacro RemoveDirectory "$INSTDIR\sr-Latn-CS"
        !insertmacro RemoveDirectory "$INSTDIR\sr-Latn-RS"
        !insertmacro RemoveDirectory "$INSTDIR\sv"
        !insertmacro RemoveDirectory "$INSTDIR\th"
        !insertmacro RemoveDirectory "$INSTDIR\tr"
        !insertmacro RemoveDirectory "$INSTDIR\zh-CHS"
        !insertmacro RemoveDirectory "$INSTDIR\zh-Hant"
        !insertmacro RemoveDirectory "$INSTDIR\HelpVideos"
        !insertmacro RemoveDirectory "$INSTDIR\Manuals"
        !insertmacro RemoveDirectory "$INSTDIR\ColorProfiles"

        !ifdef X86
            !insertmacro RemoveDirectory "$INSTDIR\x86" ; prior to 2024.1
        !else
            !insertmacro RemoveDirectory "$INSTDIR\x64" ; prior to 2024.1
        !endif

        RMDir "$INSTDIR"

        !insertmacro MUI_STARTMENU_GETFOLDER Application $MUI_TEMP

        ;Remove user data
        SetShellVarContext current
        Delete "$APPDATA\Kinovea\*.*"
        RMDir /r "$APPDATA\Kinovea"

        Delete "$SMPROGRAMS\$MUI_TEMP\Kinovea Uninstall.lnk"    ; prior to 0.7.8
        Delete "$SMPROGRAMS\$MUI_TEMP\Kinovea.lnk"              ; prior to 0.7.8
        Delete "$DESKTOP\Kinovea.lnk"
        ;Todo: delete kinovea folder under start menu / programs of current user.

        ;Remove global shortcuts.
        SetShellVarContext all
        Delete "$SMPROGRAMS\$MUI_TEMP\UninstallKinovea.lnk"
        Delete "$SMPROGRAMS\$MUI_TEMP\Kinovea.lnk"

        ;Delete empty start menu parent directories
        StrCpy $MUI_TEMP "$SMPROGRAMS\$MUI_TEMP"
        startMenuDeleteLoop:
            ClearErrors
            RMDir $MUI_TEMP
            GetFullPathName $MUI_TEMP "$MUI_TEMP\.."
            IfErrors startMenuDeleteLoopDone
            StrCmp $MUI_TEMP $SMPROGRAMS startMenuDeleteLoopDone startMenuDeleteLoop

        startMenuDeleteLoopDone:
            DeleteRegKey /ifempty HKCU "Software\Kinovea"
            DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Kinovea"
    SectionEnd

    ;--------------------------------
    ;Uninstaller Functions
    Function un.onInit
        !insertmacro MUI_UNGETLANGUAGE
    FunctionEnd
!endif
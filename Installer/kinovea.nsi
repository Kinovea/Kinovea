;------------------------------------------------
;Kinovea Installer
;------------------------------------------------

!verbose 4
!include "MUI2.nsh"

!define VERSION "2025.2.0"
!define EXTRADIR "OtherFiles"
!define BUILDDIR "..\Kinovea\Bin\x64\Release"
    
;--------------------------------
;General
;--------------------------------
    Name "Kinovea"
    OutFile "Kinovea-${VERSION}.exe"
    
    ; Default installation folder.
    InstallDir "$PROGRAMFILES64\Kinovea"
    
    ; Request privileges for Vista and higher.
    RequestExecutionLevel admin
    
    SetCompressor /SOLID lzma

    ;Install dir stored in registry for previous install.
    InstallDirRegKey HKCU "Software\Kinovea" "InstallDirectory" 
    BrandingText " "

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

;--------------------------------
;Languages
;--------------------------------
    !insertmacro MUI_LANGUAGE "English" ;first language is the default language
    !insertmacro MUI_LANGUAGE "French"
    !insertmacro MUI_LANGUAGE "German"
    !insertmacro MUI_LANGUAGE "Spanish"
    ;!insertmacro MUI_LANGUAGE "SpanishInternational"
    !insertmacro MUI_LANGUAGE "SimpChinese"
    !insertmacro MUI_LANGUAGE "TradChinese"
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
    !insertmacro MUI_LANGUAGE "Ukrainian"
    !insertmacro MUI_LANGUAGE "Czech"
    !insertmacro MUI_LANGUAGE "Slovak"
    !insertmacro MUI_LANGUAGE "Croatian"
    !insertmacro MUI_LANGUAGE "Bulgarian"
    !insertmacro MUI_LANGUAGE "Hungarian"
    !insertmacro MUI_LANGUAGE "Thai"
    !insertmacro MUI_LANGUAGE "Romanian"
    !insertmacro MUI_LANGUAGE "Latvian"
    !insertmacro MUI_LANGUAGE "Macedonian"
    !insertmacro MUI_LANGUAGE "Estonian"
    !insertmacro MUI_LANGUAGE "Turkish"
    !insertmacro MUI_LANGUAGE "Lithuanian"
    !insertmacro MUI_LANGUAGE "Slovenian"
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
    !insertmacro MUI_LANGUAGE "Malay"
    ;!insertmacro MUI_LANGUAGE "Bosnian"
    ;!insertmacro MUI_LANGUAGE "Kurdish"
    ;!insertmacro MUI_LANGUAGE "Irish"
    ;!insertmacro MUI_LANGUAGE "Uzbek"
    ;!insertmacro MUI_LANGUAGE "Galician"
    ;!insertmacro MUI_LANGUAGE "Afrikaans"
    !insertmacro MUI_LANGUAGE "Catalan"

;--------------------------------
;Properties of the installer file
;--------------------------------
	VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "Kinovea"
	VIAddVersionKey /LANG=${LANG_ENGLISH} "Comments" "Video Analysis"
	VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "Joan Charmant"
	VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "Copyright © 2006-2026 Joan Charmant"
	VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "Kinovea Installer"
	VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "${VERSION}"
	VIProductVersion "${VERSION}.0"
	CRCCheck on

;--------------------------------
;Reserve Files
;--------------------------------
    ;If you are using solid compression, files that are required before
    ;the actual installation should be stored first in the data block,
    ;because this will make your installer start faster.

    !insertmacro MUI_RESERVEFILE_LANGDLL

;--------------------------------
;Installer Sections
;--------------------------------

    ;In a common installer there are several things the user can install.
    ;For example in the NSIS distribution installer you can choose to install the source code, additional plug-ins, examples and more.
    ;Each of these components has its own piece of code.
    ;If the user selects to install this component, then the installer will execute that code.
    ;In the script, that code is defined in sections.
    ;Each section corresponds to one component in the components page.
    ;The section's name is the displayed component name, and the section code will be executed if that component is selected.
    ;It is possible to build your installer with only one section, but if you want to use the components page and let the user choose what to install, you'll have to use more than one section.

;Main installer section.
;TODO: terminate app.
Section ""

    ; Main directory
    ; The build is setup to output the full set of files so we can copy everything.
    SetOutPath "$INSTDIR"
    File /nonfatal /a /r "${BUILDDIR}\"
    
    ; Store installation folder
    WriteRegStr HKCU "Software\Kinovea" "InstallDirectory" $INSTDIR

    ; Create uninstaller
    WriteUninstaller "$INSTDIR\Uninstall.exe"

    ; Register uninstaller to Windows.
    !define AppRemovePath "Software\Microsoft\Windows\CurrentVersion\Uninstall\Kinovea"
    WriteRegStr HKLM "${AppRemovePath}" "DisplayName" "Kinovea"
    WriteRegStr HKLM "${AppRemovePath}" "DisplayVersion" "${VERSION}"
    WriteRegDWORD HKLM "${AppRemovePath}" "NoModify" "1"
    WriteRegDWORD HKLM "${AppRemovePath}" "NoRepair" "1"
    WriteRegStr HKLM "${AppRemovePath}" "Publisher" "Joan Charmant"
    WriteRegStr HKLM "${AppRemovePath}" "UninstallString" "$\"$INSTDIR\Uninstall.exe$\""

    ; Create shortcuts under all users.
    !insertmacro MUI_STARTMENU_WRITE_BEGIN Application
    SetShellVarContext all
    CreateDirectory "$SMPROGRAMS\$STARTMENU_FOLDER"
    CreateShortcut "$SMPROGRAMS\$STARTMENU_FOLDER\Kinovea.lnk" "$INSTDIR\Kinovea.exe"
    CreateShortCut "$SMPROGRAMS\$STARTMENU_FOLDER\UninstallKinovea.lnk" "$INSTDIR\Uninstall.exe"
    !insertmacro MUI_STARTMENU_WRITE_END

SectionEnd

;--------------------------------
;Installer Functions
;--------------------------------
Function .onInit
  !insertmacro MUI_LANGDLL_DISPLAY
FunctionEnd


;--------------------------------
;Uninstaller Section
;--------------------------------
!macro RemoveDirectory TargetDir
    Delete "${TargetDir}\*.*"
    RMDir "${TargetDir}"
!macroend

Section "Uninstall"

    ; Calling RMDir /r $INSTDIR or Delete "$INSTDIR\*.*" is unsafe.
    ; Concrete example: the user installs to the desktop by mistake, then runs the uninstaller to "fix" the problem, 
    ; and the uninstaller deletes all the files on the desktop.
    ; We have to delete the files and folders one by one, including those installed by previous versions.

    ; Remove known subfolders.
    RMDir /r "$INSTDIR\DrawingTools"
    RMDir /r "$INSTDIR\Languages"
    RMDir /r "$INSTDIR\xslt"
    
    ; Folders from old versions.
    RMDir /r "$INSTDIR\guides"
    RMDir /r "$INSTDIR\HelpVideos"
    RMDir /r "$INSTDIR\Manuals"
    RMDir /r "$INSTDIR\ColorProfiles"
    RMDir /r "$INSTDIR\x86"
    RMDir /r "$INSTDIR\x64"
    
    ; Individual languages directories (prior to 2025.1).
    ; Languages added after 2025.1 do not need to be added here.
    RMDir /r "$INSTDIR\ar"
    RMDir /r "$INSTDIR\bg"
    RMDir /r "$INSTDIR\ca"
    RMDir /r "$INSTDIR\cs"
    RMDir /r "$INSTDIR\da"
    RMDir /r "$INSTDIR\de"
    RMDir /r "$INSTDIR\el"
    RMDir /r "$INSTDIR\es"
    RMDir /r "$INSTDIR\fa"
    RMDir /r "$INSTDIR\fi"
    RMDir /r "$INSTDIR\fr"
    RMDir /r "$INSTDIR\hr"
    RMDir /r "$INSTDIR\it"
    RMDir /r "$INSTDIR\ja"
    RMDir /r "$INSTDIR\ko"
    RMDir /r "$INSTDIR\lt"
    RMDir /r "$INSTDIR\mk"
    RMDir /r "$INSTDIR\ms"
    RMDir /r "$INSTDIR\nl"
    RMDir /r "$INSTDIR\no"
    RMDir /r "$INSTDIR\pl"
    RMDir /r "$INSTDIR\pt"
    RMDir /r "$INSTDIR\ro"
    RMDir /r "$INSTDIR\ru"
    RMDir /r "$INSTDIR\sr-Cyrl-CS"
    RMDir /r "$INSTDIR\sr-Cyrl-RS"
    RMDir /r "$INSTDIR\sr-Latn-CS"
    RMDir /r "$INSTDIR\sr-Latn-RS"
    RMDir /r "$INSTDIR\sv"
    RMDir /r "$INSTDIR\th"
    RMDir /r "$INSTDIR\tr"
    RMDir /r "$INSTDIR\zh-CHS"
    RMDir /r "$INSTDIR\zh-Hant"

    ; Individual files.
    Delete "$INSTDIR\Aforge.dll"
    Delete "$INSTDIR\Aforge.Video.DirectShow.dll"
    Delete "$INSTDIR\Aforge.Video.dll"
    Delete "$INSTDIR\avcodec-56.dll"
    Delete "$INSTDIR\avdevice-56.dll"
    Delete "$INSTDIR\avfilter-5.dll"
    Delete "$INSTDIR\avformat-56.dll"
    Delete "$INSTDIR\avutil-54.dll"
    Delete "$INSTDIR\DocumentFormat.OpenXml.dll"
    Delete "$INSTDIR\DocumentFormat.OpenXml.Framework.dll"
    Delete "$INSTDIR\ExpTreeLib.dll"
    Delete "$INSTDIR\FastColoredTextBox.dll"
    Delete "$INSTDIR\ffmpeg.exe"
    Delete "$INSTDIR\GPLv2.txt"
    Delete "$INSTDIR\ICSharpCode.SharpZipLib.dll"
    Delete "$INSTDIR\Kinovea.Camera.DirectShow.dll"
    Delete "$INSTDIR\Kinovea.Camera.dll"
    Delete "$INSTDIR\Kinovea.Camera.FrameGenerator.dll"
    Delete "$INSTDIR\Kinovea.Camera.HTTP.dll"
    Delete "$INSTDIR\Kinovea.exe"
    Delete "$INSTDIR\Kinovea.exe.config"
    Delete "$INSTDIR\Kinovea.FileBrowser.dll"
    Delete "$INSTDIR\Kinovea.Pipeline.dll"
    Delete "$INSTDIR\Kinovea.ScreenManager.dll"
    Delete "$INSTDIR\Kinovea.Services.dll"
    Delete "$INSTDIR\Kinovea.Updater.dll"
    Delete "$INSTDIR\Kinovea.Video.Bitmap.dll"
    Delete "$INSTDIR\Kinovea.Video.dll"
    Delete "$INSTDIR\Kinovea.Video.FFMpeg.dll"
    Delete "$INSTDIR\Kinovea.Video.GIF.dll"
    Delete "$INSTDIR\Kinovea.Video.SVG.dll"
    Delete "$INSTDIR\Kinovea.Video.Synthetic.dll"
    Delete "$INSTDIR\License.txt"
    Delete "$INSTDIR\log4net.dll"
    Delete "$INSTDIR\LogConf.xml"
    Delete "$INSTDIR\MathNet.Numerics.dll"
    Delete "$INSTDIR\Microsoft.VC90.CRT.manifest"
    Delete "$INSTDIR\Microsoft.VC90.OpenMP.manifest"
    Delete "$INSTDIR\Microsoft.Win32.Registry.dll"
    Delete "$INSTDIR\msvcm90.dll"
    Delete "$INSTDIR\msvcp110.dll"
    Delete "$INSTDIR\msvcp90.dll"
    Delete "$INSTDIR\msvcr110.dll"
    Delete "$INSTDIR\msvcr90.dll"
    Delete "$INSTDIR\NAudio.Core.dll"
    Delete "$INSTDIR\NAudio.WinForms.dll"
    Delete "$INSTDIR\NAudio.WinMM.dll"
    Delete "$INSTDIR\Newtonsoft.Json.dll"
    Delete "$INSTDIR\ObjectListView.dll"
    Delete "$INSTDIR\OpenCvSharp.dll"
    Delete "$INSTDIR\OpenCvSharp.Extensions.dll"
    Delete "$INSTDIR\OpenCvSharpExtern.dll"
    Delete "$INSTDIR\OxyPlot.dll"
    Delete "$INSTDIR\OxyPlot.WindowsForms.dll"
    Delete "$INSTDIR\postproc-53.dll"
    Delete "$INSTDIR\Readme.txt"
    Delete "$INSTDIR\SharpVectorBindings.dll"
    Delete "$INSTDIR\SharpVectorCss.dll"
    Delete "$INSTDIR\SharpVectorDom.dll"
    Delete "$INSTDIR\SharpVectorObjectModel.dll"
    Delete "$INSTDIR\SharpVectorRenderingEngine.dll"
    Delete "$INSTDIR\SharpVectorUtil.dll"
    Delete "$INSTDIR\SpreadsheetLight.dll"
    Delete "$INSTDIR\swresample-1.dll"
    Delete "$INSTDIR\swscale-3.dll"
    Delete "$INSTDIR\System.Buffers.dll"
    Delete "$INSTDIR\System.Memory.dll"
    Delete "$INSTDIR\System.Numerics.Vectors.dll"
    Delete "$INSTDIR\System.Runtime.CompilerServices.Unsafe.dll"
    Delete "$INSTDIR\System.Security.AccessControl.dll"
    Delete "$INSTDIR\System.Security.Principal.Windows.dll"
    Delete "$INSTDIR\System.Threading.Tasks.Extensions.dll"
    Delete "$INSTDIR\turbojpeg.dll"
    Delete "$INSTDIR\vcomp90.dll"
    Delete "$INSTDIR\vcruntime140.dll"
    
    ; Delete the uninstaller itself.
    Delete "$INSTDIR\Uninstall.exe"
    
    ; At this point the main directory should be empty. 
    ; If it's not it won't be deleted, it means there are still user files in there.
    RMDir "$INSTDIR"

    !insertmacro MUI_STARTMENU_GETFOLDER Application $MUI_TEMP

    ;Remove user data
    SetShellVarContext current
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
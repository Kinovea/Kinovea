import subprocess
import os
import shutil

# Delete output folder if it exists
output_folder = r"Kinovea\bin\x64\Release"
if os.path.exists(output_folder):
    shutil.rmtree(output_folder)

# Rebuild
subprocess.run(
    ["msbuild",
     r"Kinovea.VS2019.sln",
     "/p:Configuration=Release",
     "/p:Platform=x64",
     #"/t:Clean;Rebuild",
     "/p:AllowedReferenceRelatedFileExtensions=none"],
     shell=True,
     check=True)

# Run NSIS to export the uninstaller.
subprocess.run(
    ["makensis",
     "/DEXPORT_UNINST",
     r"Installer\Kinovea.nsi"],
     shell=True,
     check=True)

# Sign all files here, including the uninstaller.

# Run NSIS re-importing the uninstaller.
subprocess.run(
    ["makensis",
     "/DIMPORT_UNINST",
     r"Installer\Kinovea.nsi"],
     shell=True,
     check=True)


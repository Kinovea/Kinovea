import subprocess
import os
import shutil

# Delete output folder if it exists
output_folder = r"..\Kinovea\bin\x64\Release"
if os.path.exists(output_folder):
    shutil.rmtree(output_folder)

# Rebuild
subprocess.run(
    ["msbuild",
     r"..\Kinovea.VS2019.sln",
     "/p:Configuration=Release",
     "/p:Platform=x64",
     "/t:Clean;Rebuild",
     "/p:AllowedReferenceRelatedFileExtensions=none"],
     shell=True,
     check=True)

# The output of the build is in ..\Kinovea\bin\x64\Release
# This is already the portable version, we just need to add the AppData folder.
# The code will detect it and use it instead of the user AppData folder.
appdata_folder = os.path.join(output_folder, "AppData")
os.mkdir(appdata_folder)

# For some reason the .pdb files are still copied to the output folder.
for file in os.listdir(output_folder):
    if file.endswith(".pdb"):
        os.remove(os.path.join(output_folder, file))

# Zip the output folder
shutil.make_archive("Kinovea-2025.2", 'zip', output_folder)





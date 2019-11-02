#! python
import os
import glob
import shutil
import re
import zipfile

#
# deploy.py
# joan@kinovea.org
#
# Generate and export all the language .resx files to their final destination.
# Python >= 2.6.
#
# - The cells in the ODF file MUST have different styles for different columns.
# otherwise the OpenDocument may save identical cells as "repeated".
# This would break the XSLT stylesheet and mix languages strings.
# To have a different style for each language column, 
# select all the cells in the column (not through the column header) and in format > font > choose the language.
#
# Directory tree:
# this script + ooo2resx.xsl + Kinovea-l14n-*.ods must be in the same directory.
#


#
# Function moving resx files of a specific module to this module's language directory.
#
def move_to_destination(module, target):
    print("Moving to " + target + " directory.")
    for f in glob.glob(module + "Lang*.resx"):
        shutil.copy(f, os.path.join("..\\..\\" + target + "\\Languages", f))


#
# Function that removes empty resource strings from a resx file.
# Empty lines will be in the form of : 
# <data name="dlgPreferences_grpPersistence" xml:space="preserve"><value/></data>
#
def validate_resx(filename):
    print("Validating " + filename)
    f = open(filename, encoding='utf-8')
    resx = f.read()
    f.close()
    
    p = re.compile("<data name=\".+\" xml:space=\"preserve\"><value/></data>", re.IGNORECASE)
    resx = p.sub('', resx)
    
    p = re.compile("<data name=\"\" xml:space=\"preserve\"/>", re.IGNORECASE)
    resx = p.sub('', resx)
    
    p = re.compile("<data name=\"\" xml:space=\"preserve\"><value/></data>", re.IGNORECASE)
    resx = p.sub('', resx)
    
    f = open(filename, "wb")
    f.write(resx.encode('utf-8'))
    f.close()

# Function that call the resgen executable to create a strongly typed resource accessor
def generate_resource_accessor(module, target):
    print("Generating " + module + " resources accessor.")
    # Example:
    # "ScreenManagerLang.resx" /str:cs,Kinovea.ScreenManager.Languages,ScreenManagerLang,ScreenManagerLang.Designer.cs
    # Doc: https://msdn.microsoft.com/en-us/library/ccec7sz1(v=vs.110).aspx#Strong

    module_lang = module + "Lang"

    os.chdir("..\\..\\" + target + "\\Languages")
    os.system(resgen + " " + module_lang + ".resx /str:cs,Kinovea." + module + ".Languages," + module_lang + "," + module_lang + ".Designer.cs /publicClass")

# -------------------------------------------------------------------------------
# Program Entry point.
saxon = '"C:\\Program Files\\Saxonica\\SaxonHE9.6N\\bin\\Transform.exe"'
resgen = '"C:\\Program Files (x86)\\Microsoft SDKs\Windows\\v10.0A\\bin\\NETFX 4.8 Tools\\resgen.exe"'
al = '"C:\\Program Files (x86)\\Microsoft SDKs\Windows\\v10.0A\\bin\\NETFX 4.8 Tools\\al.exe"'


print("Cleanup")
if os.path.isfile('content.xml'):
    os.remove("content.xml")

patterns = ("*.resx", "*.resources", "*.dll")
for pattern in patterns:
    for file in glob.glob(pattern):
        os.remove(file)

# 0. Extract content.xml from the OpenOffice document.
z = zipfile.ZipFile("Kinovea-l14n-rev0021.ods", "r")
z.extract("content.xml")

print("\nGenerate all Resx, first pass.")
os.system(saxon + " -t -s:content.xml -xsl:ooo2resx.xsl -o:dummy.resx")

print("\nGenerate all Resx, second pass.")
for file in glob.glob("*.resx"):
    validate_resx(file)

print("\nMoving resources to their final destination")
move_to_destination("Root", "Kinovea")
move_to_destination("Updater", "Kinovea.Updater")
move_to_destination("FileBrowser", "Kinovea.FileBrowser")
move_to_destination("ScreenManager", "Kinovea.ScreenManager")
move_to_destination("Camera", "Kinovea.Camera")

print("\nRegenerating the strongly typed resource accessors.")
generate_resource_accessor("Root", "Kinovea")
generate_resource_accessor("Updater", "Kinovea.Updater")
generate_resource_accessor("FileBrowser", "Kinovea.FileBrowser")
generate_resource_accessor("ScreenManager", "Kinovea.ScreenManager")
generate_resource_accessor("Camera", "Kinovea.Camera")

# Extra work to handle Serbian culture name change in Windows 10.
# We compile two extra satellite assemblies per module.
# When Kinovea runs on Windows < 10 it will use these cultures.


modules = {
    "RootLang": "Kinovea.Root",
    "UpdaterLang": "Kinovea.Updater",
    "FileBrowserLang": "Kinovea.FileBrowser",
    "ScreenManagerLang": "Kinovea.ScreenManager",
    "CameraLang": "Kinovea.Camera"
}

satellites = {
    "RootLang": "Kinovea",
    "UpdaterLang": "Kinovea.Updater",
    "FileBrowserLang": "Kinovea.FileBrowser",
    "ScreenManagerLang": "Kinovea.ScreenManager",
    "CameraLang": "Kinovea.Camera"
}

extra_cultures = {
    "sr-Cyrl-RS": "sr-Cyrl-CS",
    "sr-Latn-RS": "sr-Latn-CS"
}

os.chdir(os.path.dirname(os.path.realpath(__file__)))

for new_culture, old_culture in extra_cultures.items():
    # create subdirectories.
    if not os.path.exists(old_culture):
        os.makedirs(old_culture)

    for k, v in modules.items():
        input_filename = k + "." + new_culture + ".resx"
        output_filename = v + ".Languages." + k + "." + old_culture + ".resources"

        # create binary .resources files.
        print(input_filename + " -> " + output_filename)
        os.system("resgen " + input_filename + " " + output_filename)

        # embed in satellite assembly.
        template_assembly = "..\\..\\Kinovea\\bin\\x64\\Release\\Kinovea.exe"
        satellite_assembly = satellites[k] + ".resources.dll"
        os.system("al /target:lib /embed:" + output_filename + " /culture:" + old_culture + " /template:" + template_assembly + " /out:" + os.path.join(old_culture, satellite_assembly))

print("Done.")




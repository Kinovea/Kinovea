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


# Function that compile the text based resources into a binary version.
def compile_resources(filename):
    print("Compiling " + filename)
    os.system(resgen + " " + filename)


def create_satellite_assemblies(filename):
    # al /target:lib /embed:strings.de.resources /culture:de /out:Example.resources.dll /template:Example.dll
    chunks = filename.split('.')
    culture = chunks[-2]

    # TODO:
    # RootLang.fr.resources -> Kinovea.resources.dll
    # CameraLang.fr.resources -> Kinovea.Camera.resources.dll
    # os.system(al + " /target:lib /embed:" + filename + " /culture:" + culture + " /out:" + + " /template:" + )
    pass


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
resgen = '"C:\\Program Files\\Microsoft SDKs\\Windows\\v7.0\\Bin\\resgen.exe"'
al = '"C:\\Program Files\\Microsoft SDKs\\Windows\\v7.0\\Bin\\al.exe"'


print("Cleanup")
if os.path.isfile('content.xml'):
    os.remove("content.xml")

patterns = ("*.resx", "*.resources", "*.dll")
for pattern in patterns:
    for file in glob.glob(pattern):
        os.remove(file)

# 0. Extract content.xml from the OpenOffice document.
z = zipfile.ZipFile("Kinovea-l14n-rev0018.ods", "r")
z.extract("content.xml")

print("\nGenerate all Resx, first pass.")
os.system(saxon + " -t -s:content.xml -xsl:ooo2resx.xsl -o:dummy.resx")

print("\nGenerate all Resx, second pass.")
for file in glob.glob("*.resx"):
    validate_resx(file)

# create binary .resources files.
# for file in glob.glob("*.resx"):
#    compile_resources(file)

# create satellite assemblies.
# for file in glob.glob("*.resources"):
#    create_satellite_assemblies(file)

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

print("Done.")




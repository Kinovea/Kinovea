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
def call_resgen(module, target):
    print("Generating " + module + " resources accessor.")
    # Example:
    # "ScreenManagerLang.resx" /str:cs,Kinovea.ScreenManager.Languages,ScreenManagerLang,ScreenManagerLang.Designer.cs

    module_lang = module + "Lang"

    os.chdir("..\\..\\" + target + "\\Languages")
    os.system(resgen + " " + module_lang + ".resx /str:cs,Kinovea." + module + ".Languages," + module_lang + "," + module_lang + ".Designer.cs")

# Program Entry point.
saxon = '"C:\\Program Files\\Saxonica\\SaxonHE9.6N\\bin\\Transform.exe"'
resgen = '"C:\\Program Files\\Microsoft SDKs\\Windows\\v7.0\\Bin\\resgen.exe"'

print("Cleanup")
if os.path.isfile('content.xml'):
    os.remove("content.xml")

for file in glob.glob("*.resx"):
    os.remove(file)

# 0. Extract content.xml from the OpenOffice document.
z = zipfile.ZipFile("Kinovea-l14n-rev0018.ods", "r")
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

print("\nRegenerating the strongly typed resource accessors.")
call_resgen("Root", "Kinovea")
call_resgen("Updater", "Kinovea.Updater")
call_resgen("FileBrowser", "Kinovea.FileBrowser")
call_resgen("ScreenManager", "Kinovea.ScreenManager")

print("Done.")




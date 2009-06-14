#! python
import os, glob, shutil
import re
import zipfile

#------------------------------------------------------------------------------------------
# deploy.py
# joan@kinovea.org
#
# Generate and export all the language .resx files to their final destination.
# Python >= 2.6.
#
# - The cells in the opendocument file MUST have different styles for different columns.
# otherwise the OpenDocument may save identical cells as "repeated".
# This would break the XSLT stylesheet and mix languages strings.
# To have a different style for each language column, 
# select all the cells in the column (not through the column header) and in format > font > choose the language.
#
# Directory tree:
# this script + ooo2resx.xsl + Kinovea-l14n-*.ods must be in the same directory.
#------------------------------------------------------------------------------------------


#------------------------------------------------------------------------------------------
# Function that moves resx files of a specific module to this module's language directory.
#------------------------------------------------------------------------------------------
def MoveToDestination(module):
	print "Moving to " + module + "directory."
	for file in glob.glob(module + "Lang*.resx"):
		shutil.copy(file, os.path.join(devDir + "\\" + module + "\\Languages", file))

#------------------------------------------------------------------------------------------
# Function that removes empty resource strings from a resx file.
# Empty lines will be in the form of : 
# <data name="dlgPreferences_grpPersistence" xml:space="preserve"><value/></data>
#------------------------------------------------------------------------------------------
def ValidateResx(file):
	print "Validating " + file
	f = open( file, "r")
	resx = f.read()
	f.close()
	
	p = re.compile("<data name=\".+\" xml:space=\"preserve\"><value/></data>", re.IGNORECASE)
	resx = p.sub('', resx)
	
	p = re.compile("<data name=\"\" xml:space=\"preserve\"/>", re.IGNORECASE)
	resx = p.sub('', resx)
	
	f = open( file, "wb")
	f.write(resx)
	f.close()

#------------------------------------------------------------------------------------------
# Program Entry point.
#------------------------------------------------------------------------------------------

# saxonDir : The directory of saxon binaries.
# devDir : the directory where you checked out Kinovea trunk svn.
saxonDir = '"C:\\Program Files\\saxonb9-1-0-6n\\bin\\Transform"'		 
devDir = "C:\\Documents and Settings\\Administrateur\\Mes documents\\Dev  Prog\\Videa\\Sources\\trunk"

print "Cleanup"
if (os.path.isfile('content.xml')):
	os.remove("content.xml")

for file in glob.glob("*.resx"):
	os.remove(file)


# 0. Extract content.xml from the OpenOffice document.
z = zipfile.ZipFile("Kinovea-l14n-rev0011.ods", "r")
z.extract("content.xml")


# 1. Generate all the .resx through SAXON with a specially crafted XSLT stylesheet.
print "\nGenerate all Resx, first pass."
os.system(saxonDir + " -t -s:content.xml -xsl:ooo2resx.xsl -o:dummy.resx") 


# 2. Remove empty lines from generated .resx files.
print "\nGenerate all Resx, second pass."
for file in glob.glob("*.resx"):
	ValidateResx(file)


# 3. Move final files to their destination.
print "\nMoving resources to their final destination"
MoveToDestination("Root")
MoveToDestination("Updater")
MoveToDestination("FileBrowser")
MoveToDestination("ScreenManager")
print "Done."
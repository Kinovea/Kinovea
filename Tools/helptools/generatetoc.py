#! python
import os, glob, shutil
import re

#--------------------------------------------------------------------------------------------------
# generatetoc.py
# joan@kinovea.org
#
# Creates several toc files dynamically to avoid redundant copy-pasting.
#
# Usage: 
#   Go to 000 page and copy the list of topics in a new file named master.txt in this folder.
#   This script will first clean up the master.txt from extra wiki syntax (bullets), to get an XML document.
#   then the XML document will be passed through several XSLT scripts to generate
#   - the intermediate toc pages (100, 300).
#   - the various files needed by HTML Workshop (hhc, hhp)
#   Finally the CHM will be generated.
#
# Prerequisites:
#   SiteExport zip output unzipped in a /src folder.
#   Execute export-cleanup.py in this folder to get clean HTML files.
#   Move the custom 001, 004 and 005 files to this folder.
#   Move images to this folder.
#--------------------------------------------------------------------------------------------------

#-------------------------------------------------------------------------------------------
# Function that clean up the wiki syntax generated for indentation to get a proper XML file.
#-------------------------------------------------------------------------------------------
def MasterToXml(file):
    print "Cleaning up " + file
    f = open( file, "r")
    text = f.read()
    f.close()
    
    #Remove lines with only spaces and a bullet.
    p = re.compile("[\s]{4,}?[o*+]", re.IGNORECASE)
    text = p.sub('', text)
    
    #Test regex
    #match = re.search("[\s]{4,}?o", text)
    #if match is not None: 
    #   print match.group(0)

    f = open( "master.xml", "wb")
    f.write(text)
    f.close()


#------------------------------------------------------------------------------------------
# Program Entry point.
#------------------------------------------------------------------------------------------

saxon = '"C:\\Program Files\\saxonhe9-2-1-5n\\bin\\Transform.exe"'
helpCompiler = '"C:\\Program Files\\HTML Help Workshop\\hhc.exe"'


# Remove previously generated files
for f in glob.glob('*.hhp'):
   os.unlink(f)

for f in glob.glob('*.hhc'):
   os.unlink(f)
   
for f in glob.glob('*.xml'):
   os.unlink(f)

for f in glob.glob('*.chm'):
   os.unlink(f)
   
# Clean up dokuwiki syntax (list indentation) to get a clean XML document.
MasterToXml("master.txt")

# Generates intermediate toc for books.
os.system(saxon + " -t -s:master.xml -xsl:books.xsl -o:dummy")

# Generates hhp (HTML Workshop project file).
os.system(saxon + " -t -s:master.xml -xsl:hhp.xsl -o:project.hhp")

# Generates hhc (HTML Workshop toc)
os.system(saxon + " -t -s:master.xml -xsl:hhc.xsl -o:toc.hhc")

# Execute HTML Help Workshop
os.system(helpCompiler + " project.hhp")






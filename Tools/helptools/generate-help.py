#! python
import os, glob, shutil
import re

#--------------------------------------------------------------------------------------------------
# generate-help.py
# joan@kinovea.org
#
# Generates the CHM help file and folder for online help.
#
# Usage: See workflow.txt.
#   This script will first clean up the master.txt from extra wiki syntax (bullets), to get an XML document.
#   then the XML document will be passed through several XSLT scripts to generate
#   - the intermediate toc pages (sub books / sub pages)
#   - the various files needed by HTML Workshop (hhc, hhp)
#   The CHM compiler will be called to generate the CHM.
#   The toc.htm file for online help will be generated (ndoc style).
#
# Prerequisites:
#   SiteExport zip output unzipped in a ./src folder.
#   Execute cleanup.py to get clean HTML files.
#   Move the custom 001, 004 and 005 files to ./src.
#   Copy images to this folder.
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
# (do not remove ./web/index.htm)
for type in ('*.html', '*.hhp', '*.hhc', '*.xml', '*.chm', 'web/*.html', 'web/toc.htm'):
    for f in glob.glob(type):
        os.unlink(f)

# Clean up dokuwiki syntax (list indentation) to get a clean XML document.
MasterToXml("master.txt")

# Generates intermediate toc for sub books / sub pages.
os.system(saxon + " -t -s:master.xml -xsl:books.xsl -o:dummy")

# Move the newly created files to the working directory
for f in glob.glob('*.html'):
   shutil.move(f, os.path.join("src", f))

# Copy custom files to the working directory
for f in glob.glob('en/*.html'):
   shutil.copy(f, 'src')

# Generates hhp (HTML Workshop project file).
os.system(saxon + " -t -s:master.xml -xsl:hhp.xsl -o:project.hhp")

# Generates hhc (HTML Workshop toc - an HTML document)
os.system(saxon + " -t -s:master.xml -xsl:hhc.xsl -o:toc.hhc")

# Execute HTML Help Workshop
os.system(helpCompiler + " project.hhp")

# Generates toc.htm for online help.
os.system(saxon + " -t -s:master.xml -xsl:webtoc.xsl -o:web/toc.htm")

# Copy topic files to the web folder.
for f in glob.glob('src/*.*'):
   shutil.copy(f, 'web')

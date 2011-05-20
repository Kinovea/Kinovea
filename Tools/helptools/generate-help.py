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
#   This script will first turn the master.txt to a proper XML document.
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

#----------------------------------
# Export tree to XML
#----------------------------------
def ExportTree(node, file):
    if len(node['childs']) > 0:
        # A book topic.
        line = '<Book id="%s" title="%s">' % (node['id'], node['title'])
        file.write(line + '\n')
        for n in node['childs']:
            ExportTree(n, file)
        file.write('</Book>')
    else:
        # A page topic.
        line = '<Page id="%s" title="%s" />' % (node['id'], node['title'])
        file.write(line + '\n')


#----------------------------------
# Verif function to print the tree.
#----------------------------------
def PrintTree(node):
    print node['id']
    tree = node['childs']
    for n in tree:
        PrintTree(n)

#-------------------------------------------------------------
# Add a page to the tree at the right place and update parent.
#-------------------------------------------------------------
def AddToTree(currentNode, node):
    if node['depth'] > currentNode['depth']:
        # First child of the current node.
        node['parent'] = currentNode
        currentNode['childs'].append(node)
    elif node['depth'] == currentNode['depth']:
        # Sibling of the current node.
        node['parent'] = currentNode['parent']
        currentNode['parent']['childs'].append(node)
    else:
        # Sibling of an ancestor
        
        # Move up to get a sibling
        up = currentNode['depth'] - node['depth']
        for i in xrange(up):
            currentNode = currentNode['parent']
        
        # then attach to the parent of this sibling
        node['parent'] = currentNode['parent']
        currentNode['parent']['childs'].append(node)

#-------------------------------------------------------------------------------------------
# Turn the wiki table of content to XML.
#-------------------------------------------------------------------------------------------
def MasterToXml(file):
    global lang
    f = open( file, "r")
    lines = f.readlines()
    f.close()

    # Parse the toc to create the tree in memory.
    currentNode = dict(id='000', title='', depth=0, parent=None, childs=[])
    root = currentNode
    
    for line in lines:
        # Check for language. Should be the first line.
        match = re.search("lang:([a-z]{2})", line)
        if match is not None:
            lang = match.group(1)

        # Check for topic, retrieve the number of leading spaces of indentation, id and title.
        # Will be in one of the following format:
        #  * 001 - [[001|Home]]
        #  * 100 - Using Kinovea
        match = re.search("(?P<indent>[\s]{2,}?)\* (?P<id>[\d]{3}) - (\[\[[\d]{3}\|)?(?P<title>[^\]\n]*)", line)
        if match is not None:
            depth = len(match.group('indent')) / 2
            id = match.group('id')
            title = match.group('title')
            node = dict(id=id, title=title, depth=depth, parent=None, childs=[])
            AddToTree(currentNode, node)
            currentNode = node

    # Recursive export to XML tags.
    f = open( "master.xml", "wb")
    line = '<Toc lang="%s">' % lang
    f.write(line + '\n')
    for n in root['childs']:
        ExportTree(n, f)
    f.write('</Toc>')
    f.close()

#---------------------------------------------
# Fuction to remove files that are in readonly
#---------------------------------------------  
def handleRemoveReadonly(func, path, exc_info):
    import stat
    if not os.access(path, os.W_OK):
        os.chmod(path, stat.S_IWUSR)
        func(path)
    else:
        raise

#---------------------------------------------
# Fuction to remove files that are in readonly
#---------------------------------------------  
def RemoveGeneratedFiles():
    # Remove previously generated files
    # (do not remove ./web/index.htm)
    for type in ('*.hhp', '*.hhc', '*.xml', 'web/*.html', 'web/toc.htm'):
        for f in glob.glob(type):
            os.unlink(f)

#------------------------------------------------------------------------------------------
# Program Entry point.
#------------------------------------------------------------------------------------------

saxon = '"C:\\Program Files\\saxonhe9-2-1-5n\\bin\\Transform.exe"'
helpCompiler = '"C:\\Program Files\\HTML Help Workshop\\hhc.exe"'
lang = 'en'

RemoveGeneratedFiles()

# Clean up dokuwiki syntax (list indentation) to get a clean XML document.
# Also retrieve language id.
MasterToXml("master.txt")

# Generates intermediate toc for sub books / sub pages.
os.system(saxon + " -t -s:master.xml -xsl:books.xsl -o:dummy")

# Move the newly created files to the working directory
for f in glob.glob('*.html'):
   shutil.move(f, os.path.join("src", f))

# Copy custom files to the working directory
for f in glob.glob(lang + '/*.html'):
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

# Move/copy generated files to the language folder
chm = 'kinovea.%s.chm' % lang
shutil.move(chm, os.path.join(lang, chm))
shutil.copytree('web', os.path.join(lang, 'web'))
shutil.rmtree(os.path.join(lang, 'web/.svn'), ignore_errors=False, onerror=handleRemoveReadonly)

RemoveGeneratedFiles()

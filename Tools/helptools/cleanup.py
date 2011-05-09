#! python
import os, glob, shutil
import re

#--------------------------------------------------------------------------------------------------
# export-cleanup.py
# joan@kinovea.org
#
# Post processing of exported files from SiteExport DokuWiki plugin to create CHM input HTML files.
# We remove unused meta tags and links, clean up image src, etc.
#
# Usage: 
# 	unzip the output of siteexport from \data\media\wiki\.
# 	copy the script in the exported folder and run it from there.
#
# Known to work with Dokuwiki-2009-12-25c.tgz ("Lemming") and SiteExport 2010-02-17 plugin.
# DokuWiki: http://www.splitbrain.org/projects/dokuwiki
# Site Export plugin: http://www.dokuwiki.org/plugin:siteexport
# Color plugin: http://www.dokuwiki.org/plugin:color 
#--------------------------------------------------------------------------------------------------


#------------------------------------------------------------------------------------------
# Function that modifies and simplifies an html exported from siteexport plugin. 
#------------------------------------------------------------------------------------------
def CleanupFile(file):
	print "Cleaning up " + file
	f = open( file, "r")
	html = f.read()
	f.close()

	#Remove link rel in and modify meta tags.
	p = re.compile("<link rel=.*/>", re.IGNORECASE)
	html = p.sub('', html)
	p = re.compile("<meta name=\"generator\" .*? />", re.IGNORECASE)
	html = p.sub("<meta name=\"generator\" content=\"DokuWiki, SiteExport and Custom script\" />", html)
	p = re.compile("<meta name=\"robots\" .*? />", re.IGNORECASE)
	html = p.sub('', html)
	p = re.compile("<meta name=\"keywords\" .*? />", re.IGNORECASE)
	html = p.sub("<meta name=\"keywords\" content=\"Kinovea, help, manual, guide, video analysis, sport\" />", html)

	#Change page title by looking into the page content.
	match = re.search("<h3>([\s\S]*?)</h3>", html)
	if match is not None:
		html = re.sub("<title>.*?</title>", "<title>Kinovea - " + match.group(1) + "</title>", html)

	#Remove scripts.
	p = re.compile("<script[\s\S]*?</script>", re.IGNORECASE|re.MULTILINE)
	html = p.sub('', html)

	#Set font and style.
	html = re.sub("<body>", "<body style=\"color: rgb(0, 0, 0); background-color: rgb(255, 255, 255); font-family: Arial\" alink=\"#4d9933\" link=\"#4d9933\" vlink=\"#4d9933\">", html)
	#html = re.sub("<body>", "<body style=\"font-family: Arial\" vlink=\"#4d9933\">", html)

	#Switch to H2 for main titles. 
	#We don't do that in the wiki since it creates toc for the page.
	p = re.compile("<h3>([\s\S]*?)</h3>", re.IGNORECASE)
	html = p.sub("<h2>\\1</h2>", html)

	#Remove anchors.
	p = re.compile("<a name[^>]*?>([\s\S]*?)</a>", re.IGNORECASE)
	html = p.sub("\\1", html)

	# Next step commented when working with the production wiki.
	#p = re.compile("<a[\s\S]*?>", re.IGNORECASE|re.MULTILINE)
	#html = p.sub('', html)
	#p = re.compile("</a>", re.IGNORECASE)
	#html = p.sub('', html)

	# Next step commented when working with the production wiki.
	# If running on the old doku wiki, the image are already only referencing local files.
	# Simplify img tags.
	# from 	: <img src="101-ab.png?media=help_en:101-ab.png" class="media" alt="" />
	# to 	: <img src="101-ab.png"/>
	#p = re.compile("<img src=\".*?:(.*?)\" .*? />", re.IGNORECASE)
	#html = p.sub("<img src=\"\\1\" />", html)
	#match = re.search("<img src=\".*?:(.*?)\" .*? />", html)
	#print match.group(0)

	#Remove links around images.
	p = re.compile("<a href[^>]*?>(<img[\s\S]*?/>)</a>", re.IGNORECASE)
	html = p.sub("\\1", html)

	# On the prod doku wiki, the internal links are broken and we need to go from :
	#<a href="106.html" class="wikilink1" title="help_en:102">
	#to
	#<a href="102.html">
	p = re.compile("<a href=\".*?\" class=\"wikilink1\" title=\".*?:(.*?)\">", re.IGNORECASE)
	html = p.sub("<a href=\"\\1.html\">", html)

	# Beautify tables.
	# Table.
	p = re.compile("<table.*?>", re.IGNORECASE)
	html = p.sub("<table style=\"background-color:#FFFFFF; border-collapse:collapse; border-spacing:0; margin:0; padding:0;\">", html)
	# Headers.
	p = re.compile("<th.*?>", re.IGNORECASE)
	html = p.sub("<th style=\"background-color:#dfffd6; border:1px solid #669999; padding:3px;\">", html)
	# Cells.
	p = re.compile("<td.*?>", re.IGNORECASE)
	html = p.sub("<td style=\"border:1px solid #669999; padding:3px;\">", html)

	#Remove empty lines.
	html = re.sub("\n\n+", "\n", html)

	# Test regex:
	#match = re.search("<a href[^>]*?>(<img[\s\S]*?/>)</a>", html)
	#if match is not None: 
	#	print match.group(0)

	f = open( file, "wb")
	f.write(html)
	f.close()


#------------------------------------------------------------------------------------------
# Program Entry point.
#------------------------------------------------------------------------------------------

# 1. Delete unused files and directories.
if os.path.exists('src/lib'):
	shutil.rmtree('src/lib')

for unused in ('src/feed.mode.list.html', 'src/feed.html', 'src/doku.html', 'src/000.html', 'src/001.html', 'src/004.html'):
    for f in glob.glob(unused):
        os.unlink(f)

# 2. Clean up remaining HTML files.
for file in glob.glob("src/*.html"):
	CleanupFile(file)

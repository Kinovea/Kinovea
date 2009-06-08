#! python
import os, glob, shutil

#------------------------------------------------------------------------------------------
# prepare.py
# joan@kinovea.org
#
# Copy the files from various places to the installer working dir.
# note: shutil.copy2 keeps the original date of the file.
#------------------------------------------------------------------------------------------

bindir = '..\\Root\\bin\\x86\\Release'
refdir = '..\\Refs'
xsltdir = '..\\Tools\\xsl transforms\\Kva to Kva'
otherdir = 'OtherFiles'
destdir = 'Kinovea.Files\current'

#Kinovea binaries
shutil.copy(os.path.join(bindir, "Kinovea.exe"), destdir)
shutil.copy(os.path.join(bindir, "Kinovea.FileBrowser.dll"), destdir)
shutil.copy(os.path.join(bindir, "Kinovea.ScreenManager.dll"), destdir)
shutil.copy(os.path.join(bindir, "Kinovea.Services.dll"), destdir)
shutil.copy(os.path.join(bindir, "Kinovea.Updater.dll"), destdir)
shutil.copy(os.path.join(bindir, "PlayerServer.dll"), destdir)

#AForge
shutil.copy(os.path.join(refdir, "AForge.dll"), destdir)
shutil.copy(os.path.join(refdir, "AForge.Imaging.dll"), destdir)
shutil.copy(os.path.join(refdir, "AForge.Math.dll"), destdir)
shutil.copy(os.path.join(refdir, "AForge.Video.dll"), destdir)
shutil.copy(os.path.join(refdir, "AForge.Video.DirectShow.dll"), destdir)

#FFMpeg - Todo : get from ref dir. 
shutil.copy(os.path.join(bindir, "avcodec-51.dll"), destdir)
shutil.copy(os.path.join(bindir, "avformat-52.dll"), destdir)
shutil.copy(os.path.join(bindir, "avutil-49.dll"), destdir)
shutil.copy(os.path.join(bindir, "swscale-0.dll"), destdir)

#Microsoft
shutil.copy(os.path.join(refdir, "Microsoft.VC80.CRT.manifest"), destdir)
shutil.copy(os.path.join(refdir, "msvcm80.dll"), destdir)
shutil.copy(os.path.join(refdir, "msvcp80.dll"), destdir)
shutil.copy(os.path.join(refdir, "msvcr80.dll"), destdir)

#Others libraries
shutil.copy(os.path.join(refdir, "log4net.dll"), destdir)
shutil.copy(os.path.join(refdir, "PdfSharp.dll"), destdir)		# Todo : remove dependency.
shutil.copy(os.path.join(refdir, "CPI.Plot3D.dll"), destdir)	# Todo : remove dependency.
shutil.copy(os.path.join(refdir, "pthreadGC2.dll"), destdir)
shutil.copy(os.path.join(refdir, "ExpTreeLib.dll"), destdir)

# Licenses
shutil.copy(os.path.join(otherdir, "License.txt"), destdir)
shutil.copy(os.path.join(otherdir, "GFDL.txt"), destdir)
shutil.copy(os.path.join(otherdir, "GPLv2.txt"), destdir)
shutil.copy(os.path.join(otherdir, "LAL-english.htm"), destdir)
shutil.copy(os.path.join(otherdir, "LAL-french.htm"), destdir)

#Other files
shutil.copy(os.path.join(otherdir, "Readme.txt"), destdir)
shutil.copy(os.path.join(otherdir, "kinoveabundle.ico"), destdir)
shutil.copy(os.path.join(otherdir, "HelpIndex.xml"), destdir)
shutil.copy(os.path.join(otherdir, "LogConf.xml"), destdir)

#XSLT stylesheets
if not os.path.exists(os.path.join(destdir, "xslt")):
    os.makedirs(os.path.join(destdir, "xslt"))
shutil.copy2(os.path.join(xsltdir, "kva-1.2to1.3.xsl"), os.path.join(destdir, "xslt\\kva-1.2to1.3.xsl"))

#Help files
if os.path.exists(os.path.join(destdir, "Manuals")) : 
	shutil.rmtree(os.path.join(destdir, "Manuals"))
if os.path.exists(os.path.join(destdir, "HelpVideos")) : 
	shutil.rmtree(os.path.join(destdir, "HelpVideos"))

shutil.copytree(os.path.join(otherdir, "Manuals"), os.path.join(destdir, "Manuals"))
shutil.copytree(os.path.join(otherdir, "HelpVideos"), os.path.join(destdir, "HelpVideos"))

#Languages dll, remove before copy tree.
if os.path.exists(os.path.join(destdir, "de")) : 
	shutil.rmtree(os.path.join(destdir, "de"))
if os.path.exists(os.path.join(destdir, "es")) : 
	shutil.rmtree(os.path.join(destdir, "es"))
if os.path.exists(os.path.join(destdir, "fr")) : 
	shutil.rmtree(os.path.join(destdir, "fr"))
if os.path.exists(os.path.join(destdir, "nl")) : 
	shutil.rmtree(os.path.join(destdir, "nl"))
if os.path.exists(os.path.join(destdir, "pl")) : 
	shutil.rmtree(os.path.join(destdir, "pl"))
if os.path.exists(os.path.join(destdir, "pt")) : 
	shutil.rmtree(os.path.join(destdir, "pt"))
if os.path.exists(os.path.join(destdir, "it")) : 
	shutil.rmtree(os.path.join(destdir, "it"))
if os.path.exists(os.path.join(destdir, "ro")) : 
	shutil.rmtree(os.path.join(destdir, "ro"))

shutil.copytree(os.path.join(bindir, "de"), os.path.join(destdir, "de"))
shutil.copytree(os.path.join(bindir, "es"), os.path.join(destdir, "es"))
shutil.copytree(os.path.join(bindir, "fr"), os.path.join(destdir, "fr"))
shutil.copytree(os.path.join(bindir, "nl"), os.path.join(destdir, "nl"))
shutil.copytree(os.path.join(bindir, "pl"), os.path.join(destdir, "pl"))
shutil.copytree(os.path.join(bindir, "pt"), os.path.join(destdir, "pt"))
shutil.copytree(os.path.join(bindir, "it"), os.path.join(destdir, "it"))
shutil.copytree(os.path.join(bindir, "ro"), os.path.join(destdir, "ro"))




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
xsltdir = '..\\Tools\\xsl transforms'
svgdir = '..\\Tools\\svg'
otherdir = 'OtherFiles'
destdir = 'Kinovea.Files\current'

#Kinovea binaries
shutil.copy2(os.path.join(bindir, "Kinovea.exe"), destdir)
shutil.copy2(os.path.join(bindir, "Kinovea.FileBrowser.dll"), destdir)
shutil.copy2(os.path.join(bindir, "Kinovea.ScreenManager.dll"), destdir)
shutil.copy2(os.path.join(bindir, "Kinovea.Services.dll"), destdir)
shutil.copy2(os.path.join(bindir, "Kinovea.Updater.dll"), destdir)
shutil.copy2(os.path.join(bindir, "PlayerServer.dll"), destdir)
shutil.copy2(os.path.join(bindir, "SharpVectorRenderingEngine.dll"), destdir)

#AForge
shutil.copy2(os.path.join(refdir, "AForge.dll"), destdir)
shutil.copy2(os.path.join(refdir, "AForge.Imaging.dll"), destdir)
shutil.copy2(os.path.join(refdir, "AForge.Math.dll"), destdir)
shutil.copy2(os.path.join(refdir, "AForge.Video.dll"), destdir)
shutil.copy2(os.path.join(refdir, "AForge.Video.DirectShow.dll"), destdir)

#Emgu / OpenCV
shutil.copy2(os.path.join(refdir, "Emgu.CV.dll"), destdir)
shutil.copy2(os.path.join(refdir, "Emgu.Util.dll"), destdir)
shutil.copy2(os.path.join(refdir, "cvaux200.dll"), destdir)
shutil.copy2(os.path.join(refdir, "cv200.dll"), destdir)
shutil.copy2(os.path.join(refdir, "cxcore200.dll"), destdir)

#FFMpeg
shutil.copy2(os.path.join(refdir, "FFMpeg-r23012\\bin\\avcodec-52.dll"), destdir)
shutil.copy2(os.path.join(refdir, "FFMpeg-r23012\\bin\\avformat-52.dll"), destdir)
shutil.copy2(os.path.join(refdir, "FFMpeg-r23012\\bin\\avutil-50.dll"), destdir)
shutil.copy2(os.path.join(refdir, "FFMpeg-r23012\\bin\\swscale-0.dll"), destdir)

#SVG
shutil.copy2(os.path.join(refdir, "SharpVectorBindings.dll"), destdir)
shutil.copy2(os.path.join(refdir, "SharpVectorCss.dll"), destdir)
shutil.copy2(os.path.join(refdir, "SharpVectorDom.dll"), destdir)
shutil.copy2(os.path.join(refdir, "SharpVectorObjectModel.dll"), destdir)
shutil.copy2(os.path.join(refdir, "SharpVectorUtil.dll"), destdir)

#Microsoft
shutil.copy2(os.path.join(refdir, "Microsoft.VC90.CRT.manifest"), destdir)
shutil.copy2(os.path.join(refdir, "msvcm90.dll"), destdir)
shutil.copy2(os.path.join(refdir, "msvcp90.dll"), destdir)
shutil.copy2(os.path.join(refdir, "msvcr90.dll"), destdir)
shutil.copy2(os.path.join(refdir, "Microsoft.VC90.OpenMP.manifest"), destdir)
shutil.copy2(os.path.join(refdir, "vcomp90.dll"), destdir)

#Others libraries
shutil.copy2(os.path.join(refdir, "log4net.dll"), destdir)
shutil.copy2(os.path.join(refdir, "pthreadGC2.dll"), destdir)
shutil.copy2(os.path.join(refdir, "ExpTreeLib.dll"), destdir)
shutil.copy2(os.path.join(refdir, "ICSharpCode.SharpZipLib.dll"), destdir)


# Licenses
shutil.copy2(os.path.join(otherdir, "License.txt"), destdir)
shutil.copy2(os.path.join(otherdir, "GPLv2.txt"), destdir)

#Other files
shutil.copy2(os.path.join(otherdir, "Readme.txt"), destdir)
shutil.copy2(os.path.join(otherdir, "kinoveabundle.ico"), destdir)
shutil.copy2(os.path.join(otherdir, "HelpIndex.xml"), destdir)
shutil.copy2(os.path.join(otherdir, "LogConf.xml"), destdir)

#XSLT stylesheets
if not os.path.exists(os.path.join(destdir, "xslt")):
    os.makedirs(os.path.join(destdir, "xslt"))

shutil.copy2(os.path.join(xsltdir, "Kva to Kva\\kva-1.2to1.3.xsl"), os.path.join(destdir, "xslt\\kva-1.2to1.3.xsl"))
shutil.copy2(os.path.join(xsltdir, "Kva to Spreadsheets\\kva2msxml-en.xsl"), os.path.join(destdir, "xslt\\kva2msxml-en.xsl"))
shutil.copy2(os.path.join(xsltdir, "Kva to Spreadsheets\\kva2odf-en.xsl"), os.path.join(destdir, "xslt\\kva2odf-en.xsl"))
shutil.copy2(os.path.join(xsltdir, "Kva to Spreadsheets\\kva2xhtml-en.xsl"), os.path.join(destdir, "xslt\\kva2xhtml-en.xsl"))
shutil.copy2(os.path.join(xsltdir, "Kva to Spreadsheets\\kva2txt-en.xsl"), os.path.join(destdir, "xslt\\kva2txt-en.xsl"))

#SVG guides
if not os.path.exists(os.path.join(destdir, "guides")):
    os.makedirs(os.path.join(destdir, "guides"))

shutil.copy2(os.path.join(svgdir, "footbones.svg"), os.path.join(destdir, "guides\\footbones.svg"))
shutil.copy2(os.path.join(svgdir, "hexaxial.svg"), os.path.join(destdir, "guides\\hexaxial.svg"))
shutil.copy2(os.path.join(svgdir, "Human_skeleton.svg"), os.path.join(destdir, "guides\\Human_skeleton.svg"))
shutil.copy2(os.path.join(svgdir, "protractor.svg"), os.path.join(destdir, "guides\\protractor.svg"))
shutil.copy2(os.path.join(svgdir, "ring.svg"), os.path.join(destdir, "guides\\ring.svg"))

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
if os.path.exists(os.path.join(destdir, "fi")) : 
	shutil.rmtree(os.path.join(destdir, "fi"))
if os.path.exists(os.path.join(destdir, "no")) : 
	shutil.rmtree(os.path.join(destdir, "no"))
if os.path.exists(os.path.join(destdir, "tr")) : 
	shutil.rmtree(os.path.join(destdir, "tr"))
if os.path.exists(os.path.join(destdir, "Zh-CHS")) : 
	shutil.rmtree(os.path.join(destdir, "Zh-CHS"))
if os.path.exists(os.path.join(destdir, "el")) : 
	shutil.rmtree(os.path.join(destdir, "el"))

shutil.copytree(os.path.join(bindir, "de"), os.path.join(destdir, "de"))
shutil.copytree(os.path.join(bindir, "es"), os.path.join(destdir, "es"))
shutil.copytree(os.path.join(bindir, "fr"), os.path.join(destdir, "fr"))
shutil.copytree(os.path.join(bindir, "nl"), os.path.join(destdir, "nl"))
shutil.copytree(os.path.join(bindir, "pl"), os.path.join(destdir, "pl"))
shutil.copytree(os.path.join(bindir, "pt"), os.path.join(destdir, "pt"))
shutil.copytree(os.path.join(bindir, "it"), os.path.join(destdir, "it"))
shutil.copytree(os.path.join(bindir, "ro"), os.path.join(destdir, "ro"))
shutil.copytree(os.path.join(bindir, "fi"), os.path.join(destdir, "fi"))
shutil.copytree(os.path.join(bindir, "no"), os.path.join(destdir, "no"))
shutil.copytree(os.path.join(bindir, "tr"), os.path.join(destdir, "tr"))
shutil.copytree(os.path.join(bindir, "Zh-CHS"), os.path.join(destdir, "Zh-CHS"))
shutil.copytree(os.path.join(bindir, "el"), os.path.join(destdir, "el"))




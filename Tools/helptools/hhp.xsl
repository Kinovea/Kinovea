<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

<!--
	www.kinovea.org
	This stylesheet formats help file table of content into an HTLM Workshop project file (.hpp)
-->

<xsl:output method="text" encoding="ISO-8859-1"/>

<xsl:template match="/">
[OPTIONS]
Compatibility=1.1 or later
Compiled file=kinovea.<xsl:value-of select="Toc/@lang"/>.chm
Contents file=toc.hhc
Default Window=definitions
Default topic=src\001.html
Display compile progress=No
Enhanced decompilation=No
Full-text search=Yes
Language=0x409
Title=Kinovea Help

[WINDOWS]
definitions="Kinovea","toc.hhc",,"src\001.html","src\001.html",,,,,0x63520,,0x304e,,0x1000000,,,,,,0

[FILES]
<xsl:apply-templates select="descendant::Book"/>
</xsl:template>

<xsl:template match="Book">
src\<xsl:value-of select="@id"/>.html
<xsl:apply-templates select="Page"/>
</xsl:template>

<xsl:template match="Page">
src\<xsl:value-of select="@id"/>.html
</xsl:template>

</xsl:stylesheet>
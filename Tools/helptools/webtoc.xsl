<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

<!--
	www.kinovea.org
	TOC as XML to TOC as HTML for online help.
-->

<xsl:output method="html" indent="yes" encoding="ISO-8859-1"/>

<xsl:template match="/">
<html>
<head>
    <meta name="GENERATOR" content="Kinovea custom tool"/>
    <title>Kinovea</title>
    <link rel="stylesheet" type="text/css" href="tree.css"/>
    <script src="tree.js" language="javascript" type="text/javascript" />
</head>
<body id="docBody" style="margin: 0px; color: White; background-color: rgb(111, 162, 70);" onload="resizeTree()" onresize="resizeTree()" onselectstart="return false;" alink="white" link="white" vlink="white">
&#160; &#160;
    <!-- Links and flags -->
    <small><span style="font-family: Arial;">
        <a target="_top" href="http://www.kinovea.org">Site</a> -&#160;
        <a target="_top" href="http://www.kinovea.org/en/forum/">Forum</a> -&#160;
        <a href="http://www.kinovea.org/help/en/"><img style="border: 0px solid ; width: 16px; height: 11px;" alt="English Manual" src="flag_en.png" /></a>&#160;
        <a href="http://www.kinovea.org/help/fr/"><img style="border: 0px solid ; width: 16px; height: 11px;" alt="Manuel en français" src="flag_fr.png" /></a>&#160;
        <a href="http://www.kinovea.org/help/it/"><img style="border: 0px solid ; width: 16px; height: 11px;" alt="Italiano" src="flag_it.png" /></a>&#160;
        <a href="http://www.kinovea.org/help/es/"><img style="border: 0px solid ; width: 16px; height: 11px;" alt="Español" src="flag_es.png" /></a>
    </span></small>
    <div id="tree" style="top: 35px; left: 0px;" class="treeDiv">
        <div id="treeRoot" onselectstart="return false" ondragstart="return false">
            <xsl:apply-templates select="Toc/Book"/>
        </div>
    </div>
</body>
</html>
</xsl:template>

<xsl:template match="Book">
    <div class="treeNode">
        <img src="treenodeplus.gif" class="treeLinkImage" onclick="expandCollapse(this.parentNode)" />
        <a target="main" class="treeUnselected" onclick="clickAnchor(this)">
            <xsl:attribute name="href"><xsl:value-of select="@id" />.html</xsl:attribute>
            <xsl:value-of select="@title" />
        </a>
        <div class="treeSubnodesHidden">
            <xsl:apply-templates select="Book"/>
            <xsl:apply-templates select="Page"/>
        </div>
    </div>
</xsl:template>

<xsl:template match="Page">
    <div class="treeNode">
        <img src="treenodedot.gif" class="treeNoLinkImage" />
        <a target="main" class="treeUnselected" onclick="clickAnchor(this)">
            <xsl:attribute name="href"><xsl:value-of select="@id" />.html</xsl:attribute>
            <xsl:value-of select="@title" />
        </a>
    </div>
</xsl:template>

</xsl:stylesheet>
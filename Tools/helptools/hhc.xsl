<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

<!--
	www.kinovea.org
	TOC as XML to HTLM Workshop file (.hhc)
-->

<xsl:output method="html" indent="yes" encoding="ISO-8859-1"/>

<xsl:template match="/">
<html>
<head>
    <meta name="GENERATOR" content="Kinovea custom tool"/>
</head>
<body>
    <object type="text/site properties">
        <param name="FrameName" value="a"/>
        <param name="Window Styles" value="0x800025"/>
    </object>
    <ul>
        <xsl:apply-templates select="Toc/Book"/>
    </ul>
</body>
</html>
</xsl:template>

<xsl:template match="Book">
    <li>
        <object type="text/sitemap">
            <param>
                <xsl:attribute name="name">Name</xsl:attribute>
                <xsl:attribute name="value"><xsl:value-of select="@title" /></xsl:attribute>
            </param>
            <param>
                <xsl:attribute name="name">Local</xsl:attribute>
                <xsl:attribute name="value">src/<xsl:value-of select="@id"/>.html</xsl:attribute>
            </param>
        </object>
        <ul>
            <xsl:apply-templates select="Book"/>
            <xsl:apply-templates select="Page"/>
        </ul>
    </li>
</xsl:template>

<xsl:template match="Page">
    <li>
        <object type="text/sitemap">
            <param>
                <xsl:attribute name="name">Name</xsl:attribute>
                <xsl:attribute name="value"><xsl:value-of select="@title" /></xsl:attribute>
            </param>
            <param>
                <xsl:attribute name="name">Local</xsl:attribute>
                <xsl:attribute name="value">src/<xsl:value-of select="@id"/>.html</xsl:attribute>
            </param>
        </object>
    </li>
</xsl:template>

</xsl:stylesheet>
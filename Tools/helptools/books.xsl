<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="2.0">

<!--
    www.kinovea.org
    This stylesheet creates all the help pages for intermediate books.
    It creates html files with bullet lists of sub books or sub pages.
-->

<xsl:output method = "text"/>

<xsl:template match="/">
    <xsl:apply-templates select="descendant::Book"/>
</xsl:template>

<xsl:template match="Book">
    <!-- For each book we create a new .html file that lists its sub books or pages. -->
    <xsl:variable name="filename" select="concat(@id, '.html')" />
    <xsl:result-document href="{$filename}" method="xhtml" include-content-type="no" indent="yes">
    <html>
        <head>
            <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
            <meta name="generator" content="Kinovea custom tool" />
            <meta name="date" content="{format-dateTime(current-dateTime(), '[Y0001]-[M01]-[D01]T[h01]:[m01]:00+0200')}" /> <!--2011-05-01T14:36:32+0200-->
            <meta name="keywords" content="Kinovea, help, manual, guide, video analysis, sport" />
            <title>Kinovea - <xsl:value-of select="@title" /></title>
        </head>
        <body style="color: rgb(0, 0, 0); background-color: rgb(255, 255, 255); font-family: Arial" alink="#4d9933" link="#4d9933" vlink="#4d9933">
            <div class="dokuwiki export">
                <h2><xsl:value-of select="@title" /></h2>
                <div class="level3">
                    <!-- Create a bullet list for all the sub books or sub pages. -->
                    <ul>
                    <!-- List sub books -->
                    <xsl:for-each select="Book">
                        <li class="level1">
                            <div class="li">
                                <a>
                                    <xsl:attribute name="href"><xsl:value-of select="@id" />.html</xsl:attribute>
                                    <xsl:value-of select="@title" />
                                </a>
                            </div>
                            <ul>
                            <!-- Nested bullet list for sub pages inside sub books -->
                            <xsl:for-each select="Page">
                                <li class="level2">
                                    <div class="li">
                                        <a>
                                            <xsl:attribute name="href"><xsl:value-of select="@id" />.html</xsl:attribute>
                                            <xsl:value-of select="@title" />
                                        </a>
                                    </div>
                                </li>
                            </xsl:for-each>
                            </ul>
                        </li>
                    </xsl:for-each>
                    <!-- List sub pages -->
                    <xsl:for-each select="Page">
                        <li class="level1">
                            <div class="li">
                                <a>
                                    <xsl:attribute name="href"><xsl:value-of select="@id" />.html</xsl:attribute>
                                    <xsl:value-of select="@title" />
                                </a>
                            </div>
                        </li>
                    </xsl:for-each>
                    </ul>
                </div>
            </div>
        </body>
    </html>
    </xsl:result-document>

</xsl:template>

</xsl:stylesheet>
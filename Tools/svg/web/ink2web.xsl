<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:ns="http://www.w3.org/2000/svg" exclude-result-prefixes="ns" version="2.0">
<!-- 
    www.kinovea.org
    Change an SVG file from hard coded width/height to using viewbox.
    Allows to use the svg in object elements in html.
-->

<!-- Identity transform -->
<xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
</xsl:template>

<!-- Express width and height as percentage of viewbox -->
<xsl:template match="@width[parent::ns:svg]">
  <xsl:attribute name="width">
    <xsl:value-of select="'100%'"/>
  </xsl:attribute>
</xsl:template>

<xsl:template match="@height[parent::ns:svg]">
  <xsl:attribute name="height">
    <xsl:value-of select="'100%'"/>
  </xsl:attribute>
</xsl:template>

<!-- Add viewbox attribute -->
<xsl:template match="ns:svg">
  <xsl:copy>
    <xsl:attribute name="viewBox">
        <xsl:value-of select="concat('0 0 ', replace(@width, '[a-zA-Z]+', ''), ' ', replace(@height, '[a-zA-Z]+', ''))"/>
    </xsl:attribute>
    <xsl:apply-templates select="@*|node()"/>
  </xsl:copy>
</xsl:template>

</xsl:stylesheet>

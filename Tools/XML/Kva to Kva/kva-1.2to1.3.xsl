<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

<!-- 
	www.kinovea.org
	
	This stylesheet formats a .kva 1.2 file to .kva 1.3 file.
	You shouldn't have to use it manually, it is processed by Kinovea to read pre 0.8.x files.
	
	
  WORK IN PROGRESS - 
  - It is far from complete right now.
  - The 1.3 format is likely to change until a first formal 0.8.x release is made.
-->

<xsl:template match="/">

<KinoveaVideoAnalysis>
	<FormatVersion>1.3</FormatVersion>
	<Producer>Kinovea XSLT Converter (1.2 to 1.3)</Producer>
  <xsl:copy-of select="KinoveaVideoAnalysis/OriginalFilename"/>	
  <xsl:copy-of select="KinoveaVideoAnalysis/GlobalTitle"/>
  <xsl:copy-of select="KinoveaVideoAnalysis/ImageSize"/>
  <xsl:copy-of select="KinoveaVideoAnalysis/AverageTimeStampsPerFrame"/>	
  <xsl:copy-of select="KinoveaVideoAnalysis/FirstTimeStamp"/>
  <xsl:copy-of select="KinoveaVideoAnalysis/SelectionStart"/>	
			
	<xsl:apply-templates select="//Keyframes"/>
	<xsl:apply-templates select="//Tracks"/>
	<xsl:apply-templates select="//Chronos"/>
</KinoveaVideoAnalysis>		

</xsl:template>

<xsl:template match="Keyframes">
  <Keyframes>
    <xsl:for-each select="Keyframe">
      <Keyframe>
	      <xsl:copy-of select="Position"/>
        <xsl:copy-of select="Title"/>	  
        <xsl:apply-templates select="Drawings"/>
	    </Keyframe>						
		</xsl:for-each>
  </Keyframes>
</xsl:template>


<xsl:template match="Drawings">
  <Drawings>
    <xsl:for-each select="Drawing">
      <xsl:choose>
        <xsl:when test="@Type = 'DrawingLine2D'">
          <xsl:call-template name="DrawingLine2D"/>
        </xsl:when>
        
        <!-- todo other drawings -->
        <xsl:otherwise>
          <xsl:copy-of select="."/>
        </xsl:otherwise>

      </xsl:choose>
		</xsl:for-each>
  </Drawings>
</xsl:template>

<xsl:template name="DrawingLine2D">
  <xsl:element name="Drawing">
    <xsl:attribute name="Type">
		  <xsl:value-of select="@Type"/>
		</xsl:attribute>
    <xsl:copy-of select="m_StartPoint"/>
		<xsl:copy-of select="m_EndPoint"/>
		  <!-- Convert from PenLine to LineStyle-->
      <LineStyle>
        <xsl:copy-of select="PenLine/Size"/>
          <xsl:choose>
            <xsl:when test="PenLine/StartArrow = 'True' and PenLine/EndArrow = 'True'">
              <LineShape>DoubleArrow</LineShape>
            </xsl:when>
            <xsl:when test="PenLine/EndArrow = 'True'">
              <LineShape>EndArrow</LineShape>
            </xsl:when>
            <xsl:otherwise>
              <LineShape>Simple</LineShape>
            </xsl:otherwise>
          </xsl:choose>
        <xsl:copy-of select="PenLine/ColorRGB"/>
      </LineStyle>
    <xsl:copy-of select="InfosFading"/>
  </xsl:element>
</xsl:template>


<xsl:template match="Tracks">
  <xsl:copy-of select="."/>
</xsl:template>

<xsl:template match="Chronos">
  <xsl:copy-of select="."/>
</xsl:template>

</xsl:stylesheet>

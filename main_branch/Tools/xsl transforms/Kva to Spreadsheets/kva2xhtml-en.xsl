<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

<!-- 
	www.kinovea.org
	
	This stylesheet formats a .kva file to XHTML.
	You can use it to view the content of the .kva  in your browser,
-->

<xsl:output method="html" encoding="ISO-8859-1"/>

<xsl:template match="/">

  <html>
    <head>
      <title><xsl:value-of select="KinoveaVideoAnalysis/OriginalFilename"/></title>
    </head>
    <body>
      <h3>Data file: <xsl:value-of select="KinoveaVideoAnalysis/OriginalFilename"/></h3>
      <xsl:apply-templates select="//Keyframes"/>
      <xsl:apply-templates select="//Tracks"/>
      <xsl:apply-templates select="//Chronos"/>
      <br/>
      <br/>
      <br/>
      <small style="color: silver;">www.kinovea.org</small>
    </body>
  </html>		

</xsl:template>

<xsl:template match="Keyframes">

	<br/>
	<!-- Keyframes table -->
	<table border="0" cellpadding="0" cellspacing="1">
		<tr bgcolor="#d2f5b0">
			<td>Key Images</td>
		</tr>
		<xsl:for-each select="Keyframe">
			<tr>
				<td bgcolor="#e8e8e8">Title: <xsl:value-of select="Title"/></td>
			</tr>			
		</xsl:for-each>
	</table>

</xsl:template>

<xsl:template match="Tracks">

	<br/>
	<xsl:for-each select="Track">
		<br/>
		<!-- Track table -->
		<table border="0" cellpadding="0" cellspacing="1">
			<tr bgcolor="#c2dfff">
				<td colspan="2">Track</td>
			</tr>
			<tr bgcolor="#e8e8e8">
				<td>Label:</td>
				<td><xsl:value-of select="Label/Text"/></td>
			</tr>
			<tr bgcolor="#e8e8e8">
				<td colspan="3">Coordinates (x,y: <xsl:value-of select="TrackPositionList/@UserUnitLength"/>; t: time)</td>
			</tr>
			<tr>
				<td bgcolor="#e8e8e8">x</td>
				<td bgcolor="#e8e8e8">y</td>
        <td bgcolor="#e8e8e8">t</td>
			</tr>
			<xsl:for-each select="TrackPositionList/TrackPosition">
				<tr>
          <td bgcolor="#e8e8e8"><xsl:value-of select="@UserX"/></td>
				  <td bgcolor="#e8e8e8"><xsl:value-of select="@UserY"/></td>
          <td bgcolor="#e8e8e8"><xsl:value-of select="@UserTime"/></td>
				</tr>
			</xsl:for-each>
		</table>
	</xsl:for-each>
	
</xsl:template>

<xsl:template match="Chronos">

	<br/>
	<xsl:for-each select="Chrono">
		<br/>
		<!-- Chrono table -->
		<table border="0" cellpadding="0" cellspacing="1">
			<tr bgcolor="#e3b7eb">
				<td>Chrono</td>
			</tr>
			<tr>
				<td bgcolor="#e8e8e8">Label: "<xsl:value-of select="Label/Text"/>"</td>
			</tr>
			<tr>
				<td bgcolor="#e8e8e8">Duration: <xsl:value-of select="Values/UserDuration"/></td>
			</tr>
		</table>
	</xsl:for-each>

</xsl:template>

<xsl:template name="tokenize">
	<xsl:param name="inputString"/>
	<xsl:param name="separator" select="';'"/>
	<!-- Split next value from the rest -->
	<xsl:variable name="token" select="substring-before($inputString, $separator)"/>
	<xsl:variable name="nextToken" select="substring-after($inputString, $separator)"/>
	<!-- a;b;c : 
	  1. (a)(-> b;c) 
	  2. (b)(-> c) 
	  3. () 
  -->
	<xsl:choose>
    <xsl:when test="$token">
      <!-- output first value -->      
      <td>
        <xsl:attribute name="bgcolor">
				  <xsl:value-of select="'#e8e8e8'"/>
        </xsl:attribute>				  
        <xsl:value-of select="$token"/>
      </td>
				  
      <!-- recursive call to tokenize for the rest -->
      <xsl:if test="$nextToken">
        <xsl:call-template name="tokenize">
          <xsl:with-param name="inputString" select="$nextToken"/>
          <xsl:with-param name="separator" select="$separator"/>
        </xsl:call-template>
      </xsl:if>
    </xsl:when>
      
    <xsl:otherwise>
      <!-- last value -->    
      <td>
        <xsl:attribute name="bgcolor">
          <xsl:value-of select="'#e8e8e8'"/>
        </xsl:attribute>				  
        <xsl:value-of select="$inputString"/>
      </td>
    </xsl:otherwise>
  </xsl:choose>
</xsl:template>

</xsl:stylesheet>

<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
<!-- 
	www.kinovea.org
	
	This stylesheet formats a .kva file to XHTML.
	You can use it to view the content of the .kva  in your browser.
-->

<xsl:output method="html" encoding="ISO-8859-1"/>

<xsl:template match="/">

  <html>
    <head>
      <title><xsl:value-of select="KinoveaVideoAnalysis/OriginalFilename"/></title>
      <xsl:call-template name="style"/>
    </head>
    <body>
      <h2><xsl:value-of select="/KinoveaVideoAnalysis/OriginalFilename"/></h2>
      <xsl:apply-templates select="/KinoveaVideoAnalysis/Keyframes"/>
      <xsl:apply-templates select="/KinoveaVideoAnalysis/Chronos"/>
      <xsl:apply-templates select="/KinoveaVideoAnalysis/Tracks"/>
    </body>
  </html>
  
</xsl:template>

<xsl:template match="Keyframes">

	<br/>

  <xsl:call-template name="keyframes-table"/>
  
  <xsl:if test="count(Keyframe/Drawings/CrossMark/Coordinates) &gt; 0">
    <xsl:call-template name="points-table"/>
  </xsl:if>
  
  <xsl:if test="count(Keyframe/Drawings/Line/Measure) &gt; 0">
    <xsl:call-template name="lines-table"/>
  </xsl:if>

  <xsl:if test="count(Keyframe/Drawings/Angle/Measure) &gt; 0">
    <xsl:call-template name="angles-table"/>
  </xsl:if>

</xsl:template>
<xsl:template match="Chronos">
  <br/>
	<!-- Stopwatches table -->
	<table>
		<tr>
			<td class="chronos-title" colspan="2">Stopwatches</td>
		</tr>
		<tr>
				<td class="header">Label</td>
				<td class="header">Duration</td>
    </tr>
		<xsl:for-each select="Chrono">
			<tr>
				<td class="data left"><xsl:value-of select="Label/Text"/></td>
				<td class="data right"><xsl:value-of select="Values/UserDuration"/></td>
			</tr>
		</xsl:for-each>
	</table>
	
</xsl:template>
<xsl:template match="Tracks">

	<xsl:for-each select="Track">
		<br/>
		<!-- Track table -->
		<table>
			<tr>
				<td class="track-title" colspan="3">Track</td>
			</tr>
			<tr>
				<td class="header">Label:</td>
				<td class="data left" colspan="2"><xsl:value-of select="MainLabel/@Text"/></td>
			</tr>
			<tr>
				<td class="header" colspan="3">Coordinates (x,y: <xsl:value-of select="TrackPointList/@UserUnitLength"/>; t: time)</td>
			</tr>
			<tr>
				<td class="header">x</td>
				<td class="header">y</td>
        <td class="header">t</td>
			</tr>
			<xsl:for-each select="TrackPointList/TrackPoint">
				<tr>
          <td class="data right"><xsl:value-of select="@UserX"/></td>
				  <td class="data right"><xsl:value-of select="@UserY"/></td>
          <td class="data right"><xsl:value-of select="@UserTime"/></td>
				</tr>
			</xsl:for-each>
		</table>
	</xsl:for-each>
</xsl:template>


<!-- Named templates -->
<xsl:template name="style">
<style type="text/css">
body { 
  font-family: Courier New,Courier,monospace; 
}

table {
  border-collapse:collapse;
}

td {
  padding:5px;  
  border-width:1px;  
  border-color:#000000;
  border-style:solid;
}

.keyimages-title {
  text-align:center;   
  background-color:#d2f5b0;
}

.chronos-title {
  text-align:center;   
  background-color:#c2dfff;
}

.track-title {
  text-align:center;
  background-color:#e3b7eb;
}

.header {
  text-align:center;
  background-color:#e8e8e8;
}

.data {
  background-color:#FFFFFF;
}

.left {
  text-align:left;
}

.right {
  text-align:right;
}
</style>
</xsl:template>
<xsl:template name="keyframes-table">
  <!-- Context node: Keyframes -->  
  <table>
		<tr>
			<td class="keyimages-title" colspan="2">Key Images</td>
		</tr>
		<tr>
				<td class="header">Title</td>
				<td class="header">Time</td>
    </tr>
		<xsl:for-each select="Keyframe">
			<tr>
				<td class="data left"><xsl:value-of select="Title"/></td>
				<td class="data right"><xsl:value-of select="Position/@UserTime"/></td>
			</tr>
		</xsl:for-each>
	</table>
</xsl:template>

<xsl:template name="points-table">
  <!-- Context node: Keyframes -->
	<br/>
	<table>
	  <tr>
			<td class="keyimages-title" colspan="4">Points</td>
		</tr>
		<tr>
				<td class="header">X</td>
				<td class="header">Y</td>
				<td class="header">Time</td>
				<td class="header">Key Image</td>
    </tr>
		<xsl:for-each select="Keyframe/Drawings/CrossMark/Coordinates">
      <tr>
        <td class="data right"><xsl:value-of select="@UserX"/></td>
        <td class="data right"><xsl:value-of select="@UserY"/></td>
        <td class="data right"><xsl:value-of select="../../../Position/@UserTime"/></td>
        <td class="data left"><xsl:value-of select="../../../Title"/></td>
      </tr>
		</xsl:for-each>
	</table>
</xsl:template>

<xsl:template name="lines-table">
  <!-- Context node: Keyframes -->
	<br/>
	<table>
	  <tr>
			<td class="keyimages-title" colspan="3">Lines</td>
		</tr>
		<tr>
				<td class="header">Length (<xsl:value-of select="../CalibrationHelp/LengthUnit/@UserUnitLength"/>)</td>
				<td class="header">Time</td>
				<td class="header">Key Image</td>
    </tr>
		<xsl:for-each select="Keyframe/Drawings/Line/Measure">
      <tr>
        <td class="data right"><xsl:value-of select="@UserLength"/></td>
        <td class="data right"><xsl:value-of select="../../../Position/@UserTime"/></td>
        <td class="data left"><xsl:value-of select="../../../Title"/></td>
      </tr>
		</xsl:for-each>
	</table>
</xsl:template>
<xsl:template name="angles-table">
  <!-- Context node: Keyframes -->
	<br/>
	<table>
	  <tr>
			<td class="keyimages-title" colspan="3">Angles</td>
		</tr>
		<tr>
				<td class="header">Value</td>
				<td class="header">Time</td>
				<td class="header">Key Image</td>
    </tr>
		<xsl:for-each select="Keyframe/Drawings/Angle/Measure">
      <tr>
        <td class="data right"><xsl:value-of select="@UserAngle"/></td>
        <td class="data right"><xsl:value-of select="../../../Position/@UserTime"/></td>
        <td class="data left"><xsl:value-of select="../../../Title"/></td>
      </tr>
		</xsl:for-each>
	</table>
</xsl:template>
</xsl:stylesheet>

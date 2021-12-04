<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
<!-- 
    www.kinovea.org
    
    This stylesheet formats measurement data (.xml) to XHTML.
    You can use it to view the content of the spreadsheet export in a browser.
-->

<xsl:output method="html" encoding="ISO-8859-1"/>

<xsl:template match="/">

  <html>
    <head>
      <title><xsl:value-of select="KinoveaMeasurementData/OriginalFilename"/></title>
      <xsl:call-template name="style"/>
    </head>
    <body>
      <h2><xsl:value-of select="/KinoveaMeasurementData/OriginalFilename"/></h2>
      <xsl:apply-templates select="/KinoveaMeasurementData/Keyframes"/>
      <xsl:apply-templates select="/KinoveaMeasurementData/Positions"/>
      <xsl:apply-templates select="/KinoveaMeasurementData/Distances"/>
      <xsl:apply-templates select="/KinoveaMeasurementData/Angles"/>
      <xsl:apply-templates select="/KinoveaMeasurementData/Times"/>
      <!--xsl:apply-templates select="/KinoveaVideoAnalysis/Tracks"/-->
    </body>
  </html>
  
</xsl:template>

<!-- Named templates -->

<xsl:template match="Keyframes">
    <br/>
    <table>
        <tr>
            <td class="keyimages-title" colspan="2">Key Images</td>
        </tr>
        <tr>
            <td class="header">Name</td>
            <td class="header">Time</td>
        </tr>
        <xsl:for-each select="Keyframe">
            <tr>
                <td class="data left"><xsl:value-of select="@name"/></td>
                <td class="data right"><xsl:value-of select="@time"/></td>
            </tr>
        </xsl:for-each>
    </table>
</xsl:template>

<!--xsl:template match="Tracks">

    <xsl:for-each select="Track">
        <br/>
        <!- - Track table - ->
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
</xsl:template-->

<xsl:template match="Positions">
    <br/>
    <table>
        <tr>
            <td class="keyimages-title" colspan="4">Positions</td>
        </tr>
        <tr>
            <td class="header">Name</td>
            <td class="header">X</td>
            <td class="header">Y</td>
            <td class="header">Time</td>
        </tr>
        <xsl:for-each select="Position">
            <tr>
                <td class="data left"><xsl:value-of select="@name"/></td>
                <td class="data right"><xsl:value-of select="@xLocal"/></td>
                <td class="data right"><xsl:value-of select="@yLocal"/></td>
                <!--td class="data right"><xsl:value-of select="../../../Position/@UserTime"/></td-->
            </tr>
        </xsl:for-each>
    </table>
</xsl:template>

<xsl:template match="Distances">
    <br/>
    <table>
        <tr>
            <td class="keyimages-title" colspan="3">Distances</td>
        </tr>
        <tr>
            <td class="header">Name</td>
            <td class="header">Length (<xsl:value-of select="/KinoveaMeasurementData/Units/LengthUnit/@symbol"/>)</td>
            <td class="header">Time</td>
        </tr>
        <xsl:for-each select="Distance">
            <tr>
                <td class="data left"><xsl:value-of select="@name"/></td>
                <td class="data right"><xsl:value-of select="@valueLocal"/></td>
                <!--td class="data right"><xsl:value-of select="../../../Position/@UserTime"/></td-->
            </tr>
        </xsl:for-each>
    </table>
</xsl:template>

<xsl:template match="Angles">
    <br/>
    <table>
        <tr>
            <td class="keyimages-title" colspan="3">Angles</td>
        </tr>
        <tr>
            <td class="header">Name</td>
            <td class="header">Value (<xsl:value-of select="/KinoveaMeasurementData/Units/AngleUnit/@symbol"/>)</td>
            <td class="header">Time</td>
        </tr>
        <xsl:for-each select="Angle">
            <tr>
                <td class="data left"><xsl:value-of select="@name"/></td>
                <td class="data right"><xsl:value-of select="@valueLocal"/></td>
                <!--td class="data right"><xsl:value-of select="../../../Position/@UserTime"/></td-->
            </tr>
        </xsl:for-each>
    </table>
</xsl:template>

<xsl:template match="Times">
    <br/>
    <table>
        <tr>
            <td class="chronos-title" colspan="4">Times</td>
        </tr>
        <tr>
            <td class="header">Label</td>
            <td class="header">Duration</td>
            <td class="header">Start</td>
            <td class="header">Stop</td>
        </tr>
        <xsl:for-each select="Time">
            <tr>
                <td class="data left"><xsl:value-of select="@name"/></td>
                <td class="data right"><xsl:value-of select="@duration"/></td>
                <td class="data right"><xsl:value-of select="@start"/></td>
                <td class="data right"><xsl:value-of select="@stop"/></td>
            </tr>
        </xsl:for-each>
    </table>
</xsl:template>



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

</xsl:stylesheet>

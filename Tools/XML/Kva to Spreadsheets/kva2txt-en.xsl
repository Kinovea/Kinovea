<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

<!--
	www.kinovea.org
	
	This stylesheet formats trajectories contained in a .kva file to raw text.
	You can use it to plot the trajectory graph from gnuplot for example:
	gnuplot> splot "test.txt" using 1:2:3 with lines
-->

<xsl:output method="text" encoding="ISO-8859-1"/>

<xsl:template match="/">
	<xsl:text>#Kinovea Trajectory data export&#10;</xsl:text>
	<xsl:text>#T X Y&#10;</xsl:text>
	<xsl:apply-templates select="//Tracks"/>
</xsl:template>

<xsl:template match="Tracks">
	<xsl:for-each select="Track">
		<xsl:for-each select="TrackPointList/TrackPoint">
			<xsl:value-of select="@UserTime"/><xsl:text> </xsl:text>
			<xsl:value-of select="@UserX"/><xsl:text> </xsl:text>
			<xsl:value-of select="@UserY"/><xsl:text> </xsl:text>
			<xsl:text>&#10;</xsl:text>
		</xsl:for-each>
		<xsl:text>&#10;</xsl:text>
		<xsl:text>&#10;</xsl:text>
	</xsl:for-each>
</xsl:template>

</xsl:stylesheet>

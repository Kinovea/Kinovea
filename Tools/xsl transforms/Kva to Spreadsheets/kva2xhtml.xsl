<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

<!-- 
	www.kinovea.org
	
	This stylesheet formats a .kva file to XHTML.
	You can use it to view the content of the .kva  in your browser,
	to export track data to a spreadsheet for example.
	
	Usage:
	1. Rename the .kva to .xml
	2. Add <?xml-stylesheet type="text/xsl" href="kva2html.xsl"?> in the .kva (now .xml) file at second line, just before <KinoveaVideoAnalysis>.
	3. Open the modified file in your browser.
-->

<xsl:template match="/">

<html>
	<head>
		<title><xsl:value-of select="KinoveaVideoAnalysis/OriginalFilename" /></title>
		<meta http-equiv="Content-Type" content="text/html;charset=ISO-8859-1" />
	</head>
	<body>
		<h3>Fichier de donn&#233;es: <xsl:value-of select="KinoveaVideoAnalysis/OriginalFilename" /></h3>
		<xsl:apply-templates select="//Keyframes"/>
		<xsl:apply-templates select="//Tracks"/>
		<xsl:apply-templates select="//Chronos"/>
		<br />
		<br />
		<br />
		<small style="color: silver;">www.kinovea.org</small>
	</body>
</html>		

</xsl:template>

<xsl:template match="Keyframes">

	<br />
	<!-- Keyframes table -->
	<table border="0" cellpadding="0" cellspacing="1">
		<tr bgcolor="#d2f5b0">
			<td colspan="1">Images cl&#233;s</td>
		</tr>
		<xsl:for-each select="Keyframe">
			<tr>
				<td bgcolor="#e8e8e8">Titre : <xsl:value-of select="Title" /></td>
			</tr>			
		</xsl:for-each>
	</table>

</xsl:template>

<xsl:template match="Tracks">

	<br />
	<xsl:for-each select="Track">
		<br />
		<!-- Track table -->
		<table border="0" cellpadding="0" cellspacing="1">
			<tr bgcolor="#c2dfff">
				<td colspan="2">Trajectoire</td>
			</tr>
			<tr bgcolor="#e8e8e8">
				<td colspan="2">Label: "<xsl:value-of select="Label/Text" />"</td>
			</tr>
			<tr bgcolor="#e8e8e8">
				<td colspan="2">Coordonn&#233;es (pixels)</td>
			</tr>
			<tr>
				<td bgcolor="#e8e8e8">x</td>
				<td bgcolor="#e8e8e8">y</td>
			</tr>
			<xsl:for-each select="TrackPositionList/TrackPosition">
				<tr>
					<xsl:call-template name="tokenize">
						<xsl:with-param name="inputString" select="."/>
                  			<xsl:with-param name="separator" select="';'"/>
                  			<xsl:with-param name="resultElement" select="'td'"/>
          			</xsl:call-template>
				</tr>
			</xsl:for-each>
		</table>
	</xsl:for-each>
	
</xsl:template>

<xsl:template match="Chronos">

	<br />
	<xsl:for-each select="Chrono">
		<br />
		<!-- Chrono table -->
		<table border="0" cellpadding="0" cellspacing="1">
			<tr bgcolor="#e3b7eb">
				<td>Chrono</td>
			</tr>
			<tr>
				<td bgcolor="#e8e8e8">Label : "<xsl:value-of select="Label/Text" />"</td>
			</tr>
			<tr>
				<td bgcolor="#e8e8e8">Dur&#233;e : <xsl:value-of select="Values/StopCounting - Values/StartCounting" /> (ts)</td>
			</tr>
		</table>
	</xsl:for-each>

</xsl:template>

<xsl:template name="tokenize">
	<xsl:param name="inputString"/>
	<xsl:param name="separator" select="' '"/>
	<xsl:param name="resultElement" select="'item'"/>
	
	<!-- 
	This function will not output the last element because there is no separator after it.
	So the last call to this function is a single number and substring-before fails. 
	-->
	
	<!-- Split next value from the rest -->
	<xsl:variable name="token" select="substring-before($inputString, $separator)" />
	<xsl:variable name="nextToken" select="substring-after($inputString, $separator)" />
	
	<!-- return td for first value -->
	<xsl:if test="$token">
         <xsl:element name="{$resultElement}">
			<xsl:attribute name="bgcolor">
				<xsl:value-of select="'#e8e8e8'"/>
			</xsl:attribute>
			<xsl:value-of select="$token"/>
         </xsl:element>
	</xsl:if>
 
	<!-- recursive call to tokenize for the rest -->
	<xsl:if test="$nextToken">
		<xsl:call-template name="tokenize">
			<xsl:with-param name="inputString" select="$nextToken"/>
            <xsl:with-param name="separator" select="$separator"/>
            <xsl:with-param name="resultElement" select="$resultElement"/>
        </xsl:call-template>
	</xsl:if>
</xsl:template>

</xsl:stylesheet>
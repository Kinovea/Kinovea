<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"  
  xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0"
  xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0"
  xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:xsl-fo-compatible:1.0"
  xmlns:number="urn:oasis:names:tc:opendocument:xmlns:datastyle:1.0" 
  xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0"
  xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0">
<!-- 
	www.kinovea.org
	
	This stylesheet formats a .kva file to content.xml for ODF spreadsheet.
	You can use it to view the content of the .kva  in ODF compliant applications like OpenOffice.org.
-->
<xsl:output method="xml" encoding="UTF-8" indent="yes"/>

<xsl:template match="/">

	<office:document-content office:version="1.0">
		
		<xsl:call-template name="style" />
		<office:body>
			<office:spreadsheet>
				<table:table>
					<xsl:attribute name="table:name"> 
						<xsl:value-of select="/KinoveaVideoAnalysis/OriginalFilename" />
					</xsl:attribute>					
          <table:table-column/>
					<table:table-column/>					
					<xsl:apply-templates select="/KinoveaVideoAnalysis/Keyframes"/>
          <xsl:apply-templates select="/KinoveaVideoAnalysis/Chronos"/>					
					<xsl:apply-templates select="/KinoveaVideoAnalysis/Tracks"/>
				</table:table>
			</office:spreadsheet>
		</office:body>
	</office:document-content>
</xsl:template>


<xsl:template match="Keyframes">

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

  <xsl:call-template name="empty-row"/>
  
  <table:table-row>
    <table:table-cell table:style-name="chronos-title" table:number-columns-spanned="2"><text:p>Stopwatches</text:p></table:table-cell>
  </table:table-row>	
  <table:table-row>
    <table:table-cell table:style-name="header"><text:p>Label</text:p></table:table-cell>
    <table:table-cell table:style-name="header"><text:p>Duration</text:p></table:table-cell>
  </table:table-row>
  <xsl:for-each select="Chrono">
		<table:table-row>
		  <table:table-cell table:style-name="data"><text:p><xsl:value-of select="Label/Text"/></text:p></table:table-cell>
			<table:table-cell table:style-name="data"><text:p><xsl:value-of select="Values/UserDuration"/></text:p></table:table-cell>
		</table:table-row>
	</xsl:for-each>
	
</xsl:template>
<xsl:template match="Tracks">

	<xsl:for-each select="Track">
  
    <xsl:call-template name="empty-row"/>
    
		<table:table-row>
			<table:table-cell table:style-name="track-title" table:number-columns-spanned="3"><text:p>Track</text:p></table:table-cell>
		</table:table-row>
		<table:table-row>
			<table:table-cell table:style-name="header"><text:p>Label:</text:p></table:table-cell>
			<table:table-cell table:style-name="data" table:number-columns-spanned="2"><text:p><xsl:value-of select="MainLabel/@Text" /></text:p></table:table-cell>
		</table:table-row>
		<table:table-row>
			<table:table-cell table:style-name="header" table:number-columns-spanned="3"><text:p>Coordinates (x,y: <xsl:value-of select="TrackPointList/@UserUnitLength"/>; t:time)</text:p></table:table-cell>
		</table:table-row>
		<table:table-row>
			<table:table-cell table:style-name="header"><text:p>x</text:p></table:table-cell>
			<table:table-cell table:style-name="header"><text:p>y</text:p></table:table-cell>
			<table:table-cell table:style-name="header"><text:p>t</text:p></table:table-cell>
		</table:table-row>
		<xsl:for-each select="TrackPointList/TrackPoint">
			<table:table-row>
			
			    <table:table-cell>
            <xsl:attribute name="table:style-name"><xsl:value-of select="'data'"/></xsl:attribute>
            <xsl:attribute name="office:value-type"><xsl:value-of select="'float'"/></xsl:attribute>
            <xsl:attribute name="office:value"><xsl:value-of select="@UserXInvariant"/></xsl:attribute>
			      <text:p><xsl:value-of select="@UserX"/></text:p>
          </table:table-cell>
          
			    <table:table-cell>
            <xsl:attribute name="table:style-name"><xsl:value-of select="'data'"/></xsl:attribute>
            <xsl:attribute name="office:value-type"><xsl:value-of select="'float'"/></xsl:attribute>
            <xsl:attribute name="office:value"><xsl:value-of select="@UserYInvariant"/></xsl:attribute>
			      <text:p><xsl:value-of select="@UserY"/></text:p>
          </table:table-cell>
          
          <table:table-cell>
            <xsl:attribute name="table:style-name"><xsl:value-of select="'data'"/></xsl:attribute>
            <text:p><xsl:value-of select="@UserTime"/></text:p>
          </table:table-cell>
          
			</table:table-row>
		</xsl:for-each>
	</xsl:for-each>	

</xsl:template>


<!-- Named templates -->
<xsl:template name="style">

  <office:automatic-styles>
    <!-- Key images title -->
    <style:style style:name="keyimages-title" style:family="table-cell" style:parent-style-name="Default">
      <style:table-cell-properties fo:background-color="#d2f5b0" fo:border="0.002cm solid #000000"/>
      <style:text-properties fo:font-weight="bold" style:font-weight-asian="bold" style:font-weight-complex="bold"/>
      <style:paragraph-properties fo:text-align="center" fo:margin-left="0cm"/>    
    </style:style>
    
    <!-- Chronos title -->
    <style:style style:name="chronos-title" style:family="table-cell" style:parent-style-name="Default">
      <style:table-cell-properties fo:background-color="#c2dfff" fo:border="0.002cm solid #000000"/>
      <style:text-properties fo:font-weight="bold" style:font-weight-asian="bold" style:font-weight-complex="bold"/>      
      <style:paragraph-properties fo:text-align="center" fo:margin-left="0cm"/>     
    </style:style>

    <!-- Track title -->
    <style:style style:name="track-title" style:family="table-cell" style:parent-style-name="Default">
      <style:table-cell-properties fo:background-color="#e3b7eb" fo:border="0.002cm solid #000000"/>
      <style:text-properties fo:font-weight="bold" style:font-weight-asian="bold" style:font-weight-complex="bold"/>
      <style:paragraph-properties fo:text-align="center" fo:margin-left="0cm"/>    
    </style:style>
    
    <!-- data -->
    <style:style style:name="data" style:family="table-cell" style:parent-style-name="Default">
      <style:table-cell-properties fo:border="0.002cm solid #000000"/>
    </style:style>

    <!-- data headers -->
    <style:style style:name="header" style:family="table-cell" style:parent-style-name="Default">
      <style:table-cell-properties fo:background-color="#e8e8e8" fo:border="0.002cm solid #000000"/>
      <style:paragraph-properties fo:text-align="center" fo:margin-left="0cm"/>
    </style:style>
    
  </office:automatic-styles>
</xsl:template>
<xsl:template name="empty-row">
  <table:table-row>
    <table:table-cell><text:p></text:p></table:table-cell>
  </table:table-row>
</xsl:template>
<xsl:template name="keyframes-table">
  <!-- Context node: Keyframes -->
  
  <xsl:call-template name="empty-row"/>
  
  <table:table-row>
    <table:table-cell table:style-name="keyimages-title" table:number-columns-spanned="2"><text:p>Key Images</text:p></table:table-cell>
  </table:table-row>	
  <table:table-row>
    <table:table-cell table:style-name="header"><text:p>Name</text:p></table:table-cell>
    <table:table-cell table:style-name="header"><text:p>Time</text:p></table:table-cell>
  </table:table-row>
	<xsl:for-each select="Keyframe">
		<table:table-row>
			<table:table-cell table:style-name="data"><text:p><xsl:value-of select="Title"/></text:p></table:table-cell>
			<table:table-cell table:style-name="data"><text:p><xsl:value-of select="Position/@UserTime"/></text:p></table:table-cell>
		</table:table-row>
	</xsl:for-each>
</xsl:template>

<xsl:template name="points-table">
  <!-- Context node: Keyframes -->
	
	<xsl:call-template name="empty-row"/>

  <table:table-row>
    <table:table-cell table:style-name="keyimages-title" table:number-columns-spanned="4"><text:p>Points</text:p></table:table-cell>
  </table:table-row>
  <table:table-row>
    <table:table-cell table:style-name="header"><text:p>Name</text:p></table:table-cell>
    <table:table-cell table:style-name="header"><text:p>X</text:p></table:table-cell>
    <table:table-cell table:style-name="header"><text:p>Y</text:p></table:table-cell>
    <table:table-cell table:style-name="header"><text:p>Time</text:p></table:table-cell>
  </table:table-row>
  <xsl:for-each select="Keyframe/Drawings/CrossMark/Coordinates">
    <table:table-row>
			<table:table-cell table:style-name="data"><text:p><xsl:value-of select="../@name"/></text:p></table:table-cell>

      <!-- Numerical data must be formatted using point as decimal separator. Hence the use of User*Invariant -->
      <table:table-cell>
        <xsl:attribute name="table:style-name"><xsl:value-of select="'data'"/></xsl:attribute>
        <xsl:attribute name="office:value-type"><xsl:value-of select="'float'"/></xsl:attribute>
        <xsl:attribute name="office:value"><xsl:value-of select="@UserXInvariant"/></xsl:attribute>
        <text:p><xsl:value-of select="@UserX"/></text:p>
      </table:table-cell>
    
      <table:table-cell>
        <xsl:attribute name="table:style-name"><xsl:value-of select="'data'"/></xsl:attribute>
        <xsl:attribute name="office:value-type"><xsl:value-of select="'float'"/></xsl:attribute>
        <xsl:attribute name="office:value"><xsl:value-of select="@UserYInvariant"/></xsl:attribute>
        <text:p><xsl:value-of select="@UserY"/></text:p>
      </table:table-cell>
      
      <table:table-cell table:style-name="data"><text:p><xsl:value-of select="../../../Position/@UserTime"/></text:p></table:table-cell>
    </table:table-row>
  </xsl:for-each>
</xsl:template>

<xsl:template name="lines-table">
  <!-- Context node: Keyframes -->
	
	<xsl:call-template name="empty-row"/>

  <table:table-row>
    <table:table-cell table:style-name="keyimages-title" table:number-columns-spanned="3"><text:p>Lines</text:p></table:table-cell>
  </table:table-row>
  <table:table-row>
    <table:table-cell table:style-name="header"><text:p>Name</text:p></table:table-cell>
    <table:table-cell table:style-name="header"><text:p>Length (<xsl:value-of select="../Calibration/Unit/@Abbreviation"/>)</text:p></table:table-cell>
    <table:table-cell table:style-name="header"><text:p>Time</text:p></table:table-cell>
  </table:table-row>
  <xsl:for-each select="Keyframe/Drawings/Line/Measure">
    <table:table-row>
			<table:table-cell table:style-name="data"><text:p><xsl:value-of select="../@name"/></text:p></table:table-cell>

      <!-- Numerical data must be formatted using point as decimal separator. Hence the use of UserLengthInvariant -->
      <table:table-cell>
        <xsl:attribute name="table:style-name"><xsl:value-of select="'data'"/></xsl:attribute>
        <xsl:attribute name="office:value-type"><xsl:value-of select="'float'"/></xsl:attribute>
        <xsl:attribute name="office:value"><xsl:value-of select="@UserLengthInvariant"/></xsl:attribute>
        <text:p><xsl:value-of select="@UserLength"/></text:p>
      </table:table-cell>
    
      <table:table-cell table:style-name="data"><text:p><xsl:value-of select="../../../Position/@UserTime"/></text:p></table:table-cell>
    </table:table-row>
  </xsl:for-each>
</xsl:template>
<xsl:template name="angles-table">
  <!-- Context node: Keyframes -->
	
	<xsl:call-template name="empty-row"/>

  <table:table-row>
    <table:table-cell table:style-name="keyimages-title" table:number-columns-spanned="3"><text:p>Angles</text:p></table:table-cell>
  </table:table-row>
  <table:table-row>
    <table:table-cell table:style-name="header"><text:p>Name</text:p></table:table-cell>
    <table:table-cell table:style-name="header"><text:p>Value (°)</text:p></table:table-cell>
    <table:table-cell table:style-name="header"><text:p>Time</text:p></table:table-cell>
  </table:table-row>
  <xsl:for-each select="Keyframe/Drawings/Angle/Measure">
    <table:table-row>
			<table:table-cell table:style-name="data"><text:p><xsl:value-of select="../@name"/></text:p></table:table-cell>
    
      <table:table-cell>
        <xsl:attribute name="table:style-name"><xsl:value-of select="'data'"/></xsl:attribute>
        <xsl:attribute name="office:value-type"><xsl:value-of select="'float'"/></xsl:attribute>
        <xsl:attribute name="office:value"><xsl:value-of select="@UserAngle"/></xsl:attribute>
        <text:p><xsl:value-of select="@UserAngle"/></text:p>
      </table:table-cell>
    
      <table:table-cell table:style-name="data"><text:p><xsl:value-of select="../../../Position/@UserTime"/></text:p></table:table-cell>
    </table:table-row>
  </xsl:for-each>
</xsl:template>

</xsl:stylesheet>
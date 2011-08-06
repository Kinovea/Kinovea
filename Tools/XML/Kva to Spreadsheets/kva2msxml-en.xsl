<?xml version="1.0" encoding="ISO-8859-1"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns="urn:schemas-microsoft-com:office:spreadsheet" xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet" xmlns:html="http://www.w3.org/TR/REC-html40" version="1.0">
<!-- 
	www.kinovea.org
	
	This stylesheet formats a .kva file to MS-EXCEL XML spreadsheet.
	You can use it to view the content of the .kva  in MS-EXCEL 2003 and later.
-->

<xsl:output method="xml" encoding="UTF-8" indent="yes"/>
  
<xsl:template match="/">
    
    <xsl:text disable-output-escaping="yes">&lt;?mso-application progid="Excel.Sheet"?&gt;</xsl:text>
    
    <Workbook>      
      <DocumentProperties xmlns="urn:schemas-microsoft-com:office:office">
        <Title><xsl:value-of select="KinoveaVideoAnalysis/OriginalFilename"/></Title>
      </DocumentProperties>
      
      <xsl:call-template name="ExcelWorkbook"/>
      <xsl:call-template name="style"/>
      
      <Worksheet>
        <xsl:attribute name="ss:Name">
          <xsl:value-of select="/KinoveaVideoAnalysis/OriginalFilename"/>
        </xsl:attribute>
        <Table x:FullColumns="1" x:FullRows="1" ss:DefaultColumnWidth="60">
          <xsl:apply-templates select="/KinoveaVideoAnalysis/Keyframes"/>
          <xsl:apply-templates select="/KinoveaVideoAnalysis/Chronos"/>
          <xsl:apply-templates select="/KinoveaVideoAnalysis/Tracks"/>
        </Table>
      </Worksheet>
      
    </Workbook>
    
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
    
  <Row>
    <Cell ss:MergeAcross="1" ss:StyleID="chronos-title"><Data ss:Type="String">Stopwatches</Data></Cell>
  </Row>  
  <Row>
    <Cell ss:StyleID="header"><Data ss:Type="String">Label</Data></Cell>
    <Cell ss:StyleID="header"><Data ss:Type="String">Duration</Data></Cell>
  </Row>
  <xsl:for-each select="Chrono">
    <Row>
      <Cell ss:StyleID="data"><Data ss:Type="String"><xsl:value-of select="Label/Text"/></Data></Cell>
      <Cell ss:StyleID="data"><Data ss:Type="String"><xsl:value-of select="Values/UserDuration"/></Data></Cell>
    </Row>
  </xsl:for-each>
</xsl:template>
<xsl:template match="Tracks">
    
  <xsl:for-each select="Track">
    <xsl:call-template name="empty-row"/>
    <Row>
        <Cell ss:MergeAcross="2" ss:StyleID="track-title"><Data ss:Type="String">Track</Data></Cell>
    </Row>
    <Row>
        <Cell ss:StyleID="header"><Data ss:Type="String">Label :</Data></Cell>
        <Cell ss:MergeAcross="1" ss:StyleID="data"><Data ss:Type="String"><xsl:value-of select="MainLabel/@Text"/></Data></Cell>
    </Row>
    <Row>
        <Cell ss:MergeAcross="2" ss:StyleID="header">
          <Data ss:Type="String">Coords (x,y:<xsl:value-of select="TrackPointList/@UserUnitLength"/>; t:time)</Data>
        </Cell>
    </Row>
    <Row>
      <Cell ss:StyleID="header"><Data ss:Type="String">x</Data></Cell>
      <Cell ss:StyleID="header"><Data ss:Type="String">y</Data></Cell>
      <Cell ss:StyleID="header"><Data ss:Type="String">t</Data></Cell>
    </Row>
    <xsl:for-each select="TrackPointList/TrackPoint">
      <Row>
        <Cell ss:StyleID="data"><Data ss:Type="Number"><xsl:value-of select="@UserXInvariant"/></Data></Cell>
        <Cell ss:StyleID="data"><Data ss:Type="Number"><xsl:value-of select="@UserYInvariant"/></Data></Cell>
        <Cell ss:StyleID="data"><Data ss:Type="String"><xsl:value-of select="@UserTime"/></Data></Cell>
      </Row>
    </xsl:for-each>
  </xsl:for-each>
</xsl:template>

<!-- Named templates -->
<xsl:template name="ExcelWorkbook">
  <ExcelWorkbook xmlns="urn:schemas-microsoft-com:office:excel">
    <WindowHeight>12270</WindowHeight>
    <WindowWidth>14955</WindowWidth>
    <WindowTopX>720</WindowTopX>
    <WindowTopY>315</WindowTopY>
    <ProtectStructure>False</ProtectStructure>
    <ProtectWindows>False</ProtectWindows>
  </ExcelWorkbook>
</xsl:template>  
<xsl:template name="style">
  
  <Styles>
    <!-- Key images title -->
    <Style ss:ID="keyimages-title">
      <Alignment ss:Horizontal="Center" ss:Vertical="Bottom"/>      
      <Borders>
        <Border ss:Position="Bottom" ss:LineStyle="Continuous" ss:Weight="1"/>
        <Border ss:Position="Left" ss:LineStyle="Continuous" ss:Weight="1"/>
        <Border ss:Position="Right" ss:LineStyle="Continuous" ss:Weight="1"/>
        <Border ss:Position="Top" ss:LineStyle="Continuous" ss:Weight="1"/>
      </Borders>
      <Interior ss:Color="#ccffcc" ss:Pattern="Solid"/>
    </Style>
    
    <!-- Chronos title -->
    <Style ss:ID="chronos-title">
      <Alignment ss:Horizontal="Center" ss:Vertical="Bottom"/>      
      <Borders>
        <Border ss:Position="Bottom" ss:LineStyle="Continuous" ss:Weight="1"/>
        <Border ss:Position="Left" ss:LineStyle="Continuous" ss:Weight="1"/>
        <Border ss:Position="Right" ss:LineStyle="Continuous" ss:Weight="1"/>
        <Border ss:Position="Top" ss:LineStyle="Continuous" ss:Weight="1"/>
      </Borders>
      <Interior ss:Color="#99ccff" ss:Pattern="Solid"/>
    </Style>
    
    <!-- Track title -->
    <Style ss:ID="track-title">
      <Alignment ss:Horizontal="Center" ss:Vertical="Bottom"/>      
      <Borders>
        <Border ss:Position="Bottom" ss:LineStyle="Continuous" ss:Weight="1"/>
        <Border ss:Position="Left" ss:LineStyle="Continuous" ss:Weight="1"/>
        <Border ss:Position="Right" ss:LineStyle="Continuous" ss:Weight="1"/>
        <Border ss:Position="Top" ss:LineStyle="Continuous" ss:Weight="1"/>
      </Borders>
      <Interior ss:Color="#cc99ff" ss:Pattern="Solid"/>
    </Style>
    
    <!-- data -->
    <Style ss:ID="data">
      <Borders>
        <Border ss:Position="Bottom" ss:LineStyle="Continuous" ss:Weight="1"/>
        <Border ss:Position="Left" ss:LineStyle="Continuous" ss:Weight="1"/>
        <Border ss:Position="Right" ss:LineStyle="Continuous" ss:Weight="1"/>
        <Border ss:Position="Top" ss:LineStyle="Continuous" ss:Weight="1"/>
      </Borders>
      <Interior ss:Color="#ffffff" ss:Pattern="Solid"/>
    </Style>
    
    <!-- data headers -->
    <Style ss:ID="header">
      <Alignment ss:Horizontal="Center" ss:Vertical="Bottom"/>
      <Borders>
        <Border ss:Position="Bottom" ss:LineStyle="Continuous" ss:Weight="1"/>
        <Border ss:Position="Left" ss:LineStyle="Continuous" ss:Weight="1"/>
        <Border ss:Position="Right" ss:LineStyle="Continuous" ss:Weight="1"/>
        <Border ss:Position="Top" ss:LineStyle="Continuous" ss:Weight="1"/>
      </Borders>
      <Interior ss:Color="#C0C0C0" ss:Pattern="Solid"/>
    </Style>
  </Styles>

</xsl:template>
<xsl:template name="empty-row">
  <Row>
    <Cell><Data ss:Type="String"/></Cell>
  </Row>
</xsl:template>
<xsl:template name="keyframes-table">
  <!-- Context node: Keyframes -->  

  <xsl:call-template name="empty-row"/>  
  
  <Row>
    <Cell ss:MergeAcross="1" ss:StyleID="keyimages-title"><Data ss:Type="String">Key Images</Data></Cell>
  </Row>
  <Row>
    <Cell ss:StyleID="header"><Data ss:Type="String">Title</Data></Cell>
    <Cell ss:StyleID="header"><Data ss:Type="String">Time</Data></Cell>  
  </Row>
  <xsl:for-each select="Keyframe">
    <Row>
      <Cell ss:StyleID="data"><Data ss:Type="String"><xsl:value-of select="Title"/></Data></Cell>
      <Cell ss:StyleID="data"><Data ss:Type="String"><xsl:value-of select="Position/@UserTime"/></Data></Cell>
    </Row>
  </xsl:for-each>
</xsl:template>

<xsl:template name="points-table">
  <!-- Context node: Keyframes -->
  
  <xsl:call-template name="empty-row"/>	

  <Row>
    <Cell ss:MergeAcross="3" ss:StyleID="keyimages-title"><Data ss:Type="String">Points</Data></Cell>
  </Row>
  <Row>
    <Cell ss:StyleID="header"><Data ss:Type="String">X</Data></Cell>
    <Cell ss:StyleID="header"><Data ss:Type="String">Y</Data></Cell>
    <Cell ss:StyleID="header"><Data ss:Type="String">Time</Data></Cell>
    <Cell ss:StyleID="header"><Data ss:Type="String">Key Image</Data></Cell>
  </Row>
  <xsl:for-each select="Keyframe/Drawings/CrossMark/Coordinates">
    <Row>
      <Cell ss:StyleID="data"><Data ss:Type="Number"><xsl:value-of select="@UserXInvariant"/></Data></Cell>
      <Cell ss:StyleID="data"><Data ss:Type="Number"><xsl:value-of select="@UserYInvariant"/></Data></Cell>
      <Cell ss:StyleID="data"><Data ss:Type="String"><xsl:value-of select="../../../Position/@UserTime"/></Data></Cell>
      <Cell ss:StyleID="data"><Data ss:Type="String"><xsl:value-of select="../../../Title"/></Data></Cell>
    </Row>
  </xsl:for-each>
</xsl:template>

<xsl:template name="lines-table">
  <!-- Context node: Keyframes -->
  
  <xsl:call-template name="empty-row"/>	

  <Row>
    <Cell ss:MergeAcross="2" ss:StyleID="keyimages-title"><Data ss:Type="String">Lines</Data></Cell>
  </Row>
  <Row>
    <Cell ss:StyleID="header"><Data ss:Type="String">Length (<xsl:value-of select="../CalibrationHelp/LengthUnit/@UserUnitLength"/>)</Data></Cell>
    <Cell ss:StyleID="header"><Data ss:Type="String">Time</Data></Cell>
    <Cell ss:StyleID="header"><Data ss:Type="String">Key Image</Data></Cell>
  </Row>
  <xsl:for-each select="Keyframe/Drawings/Line/Measure">
    <Row>
      <Cell ss:StyleID="data"><Data ss:Type="Number"><xsl:value-of select="@UserLengthInvariant"/></Data></Cell>
      <Cell ss:StyleID="data"><Data ss:Type="String"><xsl:value-of select="../../../Position/@UserTime"/></Data></Cell>
      <Cell ss:StyleID="data"><Data ss:Type="String"><xsl:value-of select="../../../Title"/></Data></Cell>
    </Row>
  </xsl:for-each>
</xsl:template>
<xsl:template name="angles-table">
  <!-- Context node: Keyframes -->

	<xsl:call-template name="empty-row"/>	

  <Row>
    <Cell ss:MergeAcross="2" ss:StyleID="keyimages-title"><Data ss:Type="String">Angles</Data></Cell>
  </Row>
  <Row>
    <Cell ss:StyleID="header"><Data ss:Type="String">Value</Data></Cell>
    <Cell ss:StyleID="header"><Data ss:Type="String">Time</Data></Cell>
    <Cell ss:StyleID="header"><Data ss:Type="String">Key Image</Data></Cell>
  </Row>
  <xsl:for-each select="Keyframe/Drawings/Angle/Measure">
    <Row>
      <Cell ss:StyleID="data"><Data ss:Type="Number"><xsl:value-of select="@UserAngle"/></Data></Cell>
      <Cell ss:StyleID="data"><Data ss:Type="String"><xsl:value-of select="../../../Position/@UserTime"/></Data></Cell>
      <Cell ss:StyleID="data"><Data ss:Type="String"><xsl:value-of select="../../../Title"/></Data></Cell>  
    </Row>
  </xsl:for-each>
</xsl:template>
</xsl:stylesheet>

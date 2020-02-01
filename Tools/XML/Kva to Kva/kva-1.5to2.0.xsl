<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

<!-- 
    www.kinovea.org

    This stylesheet formats a .kva 1.5 file to .kva 2.0 file.
    You shouldn't have to use it manually, it is processed by Kinovea to read pre 0.8.16 files.
    
    2019-04-02 - Update to calibration, tracks and drawings for 0.8.27 flavor.
    2011-08-06 - Initial converter. Styles are not converted.

    Note: Conversion of track points from relative time to absolute time is done in the main codebase.
-->

<xsl:template match="/">

<KinoveaVideoAnalysis>
    <FormatVersion>2.0</FormatVersion>
    <Producer>Kinovea XSLT Converter (1.5 to 2.0)</Producer>
    <xsl:copy-of select="KinoveaVideoAnalysis/OriginalFilename"/>

    <xsl:if test="KinoveaVideoAnalysis/GlobalTitle">
        <xsl:copy-of select="KinoveaVideoAnalysis/GlobalTitle"/>
    </xsl:if>
    <xsl:copy-of select="KinoveaVideoAnalysis/ImageSize"/>
    <xsl:copy-of select="KinoveaVideoAnalysis/AverageTimeStampsPerFrame"/>
    
    <xsl:copy-of select="KinoveaVideoAnalysis/FirstTimeStamp"/>
    <xsl:copy-of select="KinoveaVideoAnalysis/SelectionStart"/>
    
    <xsl:apply-templates select="//CalibrationHelp"/>
    <xsl:apply-templates select="//Keyframes"/>
    <xsl:apply-templates select="//Chronos"/>
    <xsl:apply-templates select="//Tracks"/>

    <Trackability />
</KinoveaVideoAnalysis>

</xsl:template>

<xsl:template match="CalibrationHelp">
    <Calibration>
        <!-- Only line calibration was supported. -->
        <CalibrationLine>
            <xsl:if test="not(CoordinatesOrigin='-1;-1')">
                <Origin><xsl:value-of select="CoordinatesOrigin"/></Origin>
            </xsl:if>
            <Scale><xsl:value-of select="PixelToUnit"/></Scale>
        </CalibrationLine>
        <Unit>
            <xsl:attribute name="Abbreviation">
                <xsl:value-of select="LengthUnit/@UserUnitLength"/>
            </xsl:attribute>
            <xsl:choose>
                <xsl:when test="LengthUnit='0'">Centimeters</xsl:when>
                <xsl:when test="LengthUnit='1'">Meters</xsl:when>
                <xsl:when test="LengthUnit='2'">Inches</xsl:when>
                <xsl:when test="LengthUnit='3'">Feet</xsl:when>
                <xsl:when test="LengthUnit='4'">Yards</xsl:when>
                <xsl:when test="LengthUnit='5'">Pixels</xsl:when>
                <xsl:otherwise>Pixels</xsl:otherwise>
            </xsl:choose>
        </Unit>
    </Calibration>
</xsl:template>

<xsl:template match="Keyframes">
    <Keyframes>
    <xsl:for-each select="Keyframe">
        <Keyframe>
            <xsl:copy-of select="Position"/>
            <xsl:if test="Title">
                <xsl:copy-of select="Title"/>
            </xsl:if>
            <xsl:if test="Title">
                <xsl:copy-of select="Comment"/>
            </xsl:if>
            <xsl:apply-templates select="Drawings"/>
        </Keyframe>
    </xsl:for-each>
    </Keyframes>
</xsl:template>

<xsl:template match="Drawings">
    <Drawings>
    <xsl:for-each select="Drawing">
        <xsl:choose>
            <xsl:when test="@Type = 'DrawingAngle2D'">
                <xsl:call-template name="DrawingAngle2D"/>
            </xsl:when>
            <xsl:when test="@Type = 'DrawingCircle'">
                <xsl:call-template name="DrawingCircle"/>
            </xsl:when>
            <xsl:when test="@Type = 'DrawingCross2D'">
                <xsl:call-template name="DrawingCross2D"/>
            </xsl:when>
            <xsl:when test="@Type = 'DrawingLine2D'">
                <xsl:call-template name="DrawingLine2D"/>
            </xsl:when>
            <xsl:when test="@Type = 'DrawingPencil'">
                <xsl:call-template name="DrawingPencil"/>
            </xsl:when>
            <xsl:when test="@Type = 'DrawingText'">
                <xsl:call-template name="DrawingText"/>
            </xsl:when>
            <xsl:otherwise>
                <xsl:copy-of select="."/>
            </xsl:otherwise>
        </xsl:choose>
    </xsl:for-each>
  </Drawings>
</xsl:template>

<!-- Drawings conversion -->
<xsl:template name="DrawingAngle2D">
    <xsl:element name="Angle">
        <xsl:copy-of select="PointO"/>
        <xsl:copy-of select="PointA"/>
        <xsl:copy-of select="PointB"/>
        <Signed>false</Signed>
        <CCW>true</CCW>
        <Supplementary>false</Supplementary>
        <DrawingStyle>
            <Color Key="line color">
                <Value><xsl:value-of select="TextDecoration/BackColor"/></Value>
            </Color>
        </DrawingStyle>
        <xsl:copy-of select="InfosFading"/>
        <xsl:copy-of select="Measure"/>
    </xsl:element>
</xsl:template>

<xsl:template name="DrawingCircle">
    <xsl:element name="Circle">
        <xsl:copy-of select="Origin"/>
        <xsl:copy-of select="Radius"/>
        <ExtraData>None</ExtraData>
        <MeasureLabel>
            <SpacePosition><xsl:value-of select="Origin"/></SpacePosition>
            <TimePosition>0</TimePosition>
        </MeasureLabel>
        <DrawingStyle>
            <Color Key="color">
                <Value><xsl:value-of select="LineStyle/ColorRGB"/></Value>
            </Color>
            <PenSize Key="pen size">
                <Value><xsl:value-of select="LineStyle/Size"/></Value>
            </PenSize>
            <PenShape Key="pen shape">
                <Value>Solid</Value>
            </PenShape>
        </DrawingStyle>
        <xsl:copy-of select="InfosFading"/>
    </xsl:element>
</xsl:template>

<xsl:template name="DrawingCross2D">
    <xsl:element name="CrossMark">
        <xsl:copy-of select="CenterPoint"/>
        <ExtraData>
            <xsl:choose>
                <xsl:when test="CoordinatesVisible='True'">Position</xsl:when>
                <xsl:otherwise>None</xsl:otherwise>
            </xsl:choose>
        </ExtraData>
        <MeasureLabel>
            <!-- Fake space position to avoid it being invisible. -->
            <SpacePosition><xsl:value-of select="CenterPoint"/></SpacePosition>
            <TimePosition>0</TimePosition>
        </MeasureLabel>
        <DrawingStyle>
            <Color Key="back color">
                <Value><xsl:value-of select="LineStyle/ColorRGB"/></Value>
            </Color>
        </DrawingStyle>
        <xsl:copy-of select="InfosFading"/>
        <xsl:copy-of select="Coordinates"/>
    </xsl:element>
</xsl:template>

<xsl:template name="DrawingLine2D">
    <Line>
        <Start><xsl:value-of select="m_StartPoint"/></Start>
        <End><xsl:value-of select="m_EndPoint"/></End>

        <ExtraData>
            <xsl:choose>
                <xsl:when test="MeasureIsVisible='True'">TotalDistance</xsl:when>
                <xsl:otherwise>None</xsl:otherwise>
            </xsl:choose>
        </ExtraData>
        <MeasureLabel>
            <!-- Fake space position to avoid it being invisible. -->
            <SpacePosition><xsl:value-of select="m_StartPoint"/></SpacePosition>
            <TimePosition>0</TimePosition>
        </MeasureLabel>
        
        <DrawingStyle>
            <Color Key="color">
                <Value><xsl:value-of select="LineStyle/ColorRGB"/></Value>
            </Color>
            <LineSize Key="line size">
                <Value><xsl:value-of select="LineStyle/Size"/></Value>
            </LineSize>
            <LineShape Key="line shape">
                <Value>Solid</Value>
            </LineShape>
            <Arrows Key="arrows">
                <Value>
                    <xsl:choose>
                        <xsl:when test="LineStyle/LineShape='Simple'">None</xsl:when>
                        <xsl:when test="LineStyle/LineShape='EndArrow'">EndArrow</xsl:when>
                        <xsl:when test="LineStyle/LineShape='DoubleArrow'">DoubleArrow</xsl:when>
                        <xsl:otherwise>None</xsl:otherwise>
                    </xsl:choose>
                </Value>
            </Arrows>
        </DrawingStyle>
        <xsl:copy-of select="InfosFading"/>
        <xsl:copy-of select="Measure"/>
    </Line>
</xsl:template>

<xsl:template name="DrawingPencil">
    <xsl:element name="Pencil">
        <xsl:copy-of select="PointList"/>
        <DrawingStyle>
            <Color Key="color">
                <Value><xsl:value-of select="LineStyle/ColorRGB"/></Value>                
            </Color>
            <PenSize Key="pen size">
                <Value><xsl:value-of select="LineStyle/Size"/></Value>
            </PenSize>
            <PenShape Key="pen shape">
                <Value>Solid</Value>
            </PenShape>
        </DrawingStyle>
        <xsl:copy-of select="InfosFading"/>
    </xsl:element>
</xsl:template>

<xsl:template name="DrawingText">
    <xsl:element name="Label">
        <xsl:attribute name="name">
            <xsl:value-of select="Text"/>
        </xsl:attribute>
        <xsl:copy-of select="Text"/>
        <xsl:copy-of select="Position"/>
        <ArrowVisible>false</ArrowVisible>
        <ArrowEnd><xsl:value-of select="Position"/></ArrowEnd>
        <DrawingStyle>
            <Color Key="back color">
              <Value><xsl:value-of select="TextDecoration/BackColor"/></Value>
            </Color>
            <FontSize Key="font size">
              <Value><xsl:value-of select="TextDecoration/FontSize"/></Value>
            </FontSize>
          </DrawingStyle>
        <xsl:copy-of select="InfosFading"/>
    </xsl:element>
</xsl:template>

<xsl:template match="Chronos">
    <xsl:copy-of select="."/>
</xsl:template>

<xsl:template match="Tracks">
    <Tracks>
    <xsl:for-each select="Track">
        <Track>
            <xsl:attribute name="name">
                <xsl:value-of select="Label/Text"/>
            </xsl:attribute>
            <xsl:copy-of select="TimePosition"/>
            <xsl:if test="Mode">
                <xsl:copy-of select="Mode"/>
            </xsl:if>
            <xsl:if test="ExtraData">
                <xsl:copy-of select="ExtraData"/>
            </xsl:if>
            <xsl:element name="TrackPointList">
                <xsl:attribute name="Count"><xsl:value-of select="TrackPositionList/@Count"/></xsl:attribute>
                <xsl:attribute name="UserUnitLength"><xsl:value-of select="TrackPositionList/@UserUnitLength"/></xsl:attribute>
                <xsl:for-each select="TrackPositionList/TrackPosition">
                    <xsl:element name="TrackPoint">
                        <xsl:attribute name="UserX"><xsl:value-of select="@UserX"/></xsl:attribute>
                        <xsl:attribute name="UserXInvariant"><xsl:value-of select="@UserXInvariant"/></xsl:attribute>
                        <xsl:attribute name="UserY"><xsl:value-of select="@UserY"/></xsl:attribute>
                        <xsl:attribute name="UserYInvariant"><xsl:value-of select="@UserYInvariant"/></xsl:attribute>
                        <xsl:attribute name="UserTime"><xsl:value-of select="@UserTime"/></xsl:attribute>
                        <xsl:value-of select="."/>
                    </xsl:element>
                </xsl:for-each>
            </xsl:element>

            <xsl:element name="MainLabel">
                <xsl:attribute name="Text">
                    <xsl:value-of select="Label/Text"/>
                </xsl:attribute>
                <xsl:element name="SpacePosition">
                    <xsl:value-of select="MainLabel/KeyframeLabel/SpacePosition"/>
                </xsl:element>
                <xsl:element name="TimePosition">
                    <xsl:value-of select="MainLabel/KeyframeLabel/TimePosition"/>
                </xsl:element>
            </xsl:element>

            <DrawingStyle>
                <Color Key="color">
                    <Value><xsl:value-of select="TrackLine/LineStyle/ColorRGB"/></Value>
                </Color>
                <LineSize Key="line size">
                    <Value><xsl:value-of select="TrackLine/LineStyle/Size"/></Value>
                </LineSize>
                <TrackShape Key="track shape">
                    <Value>Solid;false</Value>
                </TrackShape>
            </DrawingStyle>
        </Track>
    </xsl:for-each>
    </Tracks>
</xsl:template>

</xsl:stylesheet>

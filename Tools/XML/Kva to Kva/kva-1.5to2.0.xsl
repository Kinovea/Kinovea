<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

<!-- 
    www.kinovea.org

    This stylesheet formats a .kva 1.5 file to .kva 2.0 file.
    You shouldn't have to use it manually, it is processed by Kinovea to read pre 0.8.16 files.
    2011-08-06 - Initial converter. Styles are not converted.
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
    <xsl:if test="KinoveaVideoAnalysis/DuplicationFactor">
        <xsl:copy-of select="KinoveaVideoAnalysis/DuplicationFactor"/>
    </xsl:if>
    <xsl:if test="KinoveaVideoAnalysis/CalibrationHelp">
        <xsl:copy-of select="KinoveaVideoAnalysis/CalibrationHelp"/>
    </xsl:if>
    
    <xsl:apply-templates select="//Keyframes"/>
    <xsl:apply-templates select="//Chronos"/>
    <xsl:apply-templates select="//Tracks"/>
</KinoveaVideoAnalysis>

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
        <!-- FIXME: Convert style -->
        <xsl:if test="InfosFading">
            <xsl:copy-of select="InfosFading"/>
        </xsl:if>
        <xsl:if test="Measure">
            <xsl:copy-of select="Measure"/>
        </xsl:if>
    </xsl:element>
</xsl:template>

<xsl:template name="DrawingCircle">
    <xsl:element name="Circle">
        <xsl:copy-of select="Origin"/>
        <xsl:copy-of select="Radius"/>
        <!-- FIXME: Convert style -->
        <xsl:if test="InfosFading">
            <xsl:copy-of select="InfosFading"/>
        </xsl:if>
    </xsl:element>
</xsl:template>

<xsl:template name="DrawingCross2D">
    <xsl:element name="CrossMark">
        <xsl:copy-of select="CenterPoint"/>
        <xsl:copy-of select="CoordinatesVisible"/>
        <!-- FIXME: Convert style -->
        <xsl:if test="InfosFading">
            <xsl:copy-of select="InfosFading"/>
        </xsl:if>
        <xsl:if test="Coordinates">
            <xsl:copy-of select="Coordinates"/>
        </xsl:if>
    </xsl:element>
</xsl:template>

<xsl:template name="DrawingLine2D">
    <xsl:element name="Line">
        <xsl:element name="Start"><xsl:value-of select="m_StartPoint"/></xsl:element>
        <xsl:element name="End"><xsl:value-of select="m_EndPoint"/></xsl:element>
        <xsl:element name="MeasureVisible"><xsl:value-of select="MeasureIsVisible"/></xsl:element>
        <!-- FIXME: Convert style -->
        <xsl:if test="InfosFading">
            <xsl:copy-of select="InfosFading"/>
        </xsl:if>
        <xsl:if test="Measure">
            <xsl:copy-of select="Measure"/>
        </xsl:if>
    </xsl:element>
</xsl:template>

<xsl:template name="DrawingPencil">
    <xsl:element name="Pencil">
        <xsl:copy-of select="PointList"/>
        <!-- FIXME: Convert style -->
        <xsl:if test="InfosFading">
            <xsl:copy-of select="InfosFading"/>
        </xsl:if>
    </xsl:element>
</xsl:template>

<xsl:template name="DrawingText">
    <xsl:element name="Label">
        <xsl:copy-of select="Text"/>
        <xsl:copy-of select="Position"/>
        <!-- FIXME: Convert style -->
        <xsl:if test="InfosFading">
            <xsl:copy-of select="InfosFading"/>
        </xsl:if>
    </xsl:element>
</xsl:template>

<xsl:template match="Chronos">
    <xsl:copy-of select="."/>
</xsl:template>

<xsl:template match="Tracks">
    <Tracks>
    <xsl:for-each select="Track">
        <Track>
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
        </Track>
    </xsl:for-each>
    </Tracks>
</xsl:template>

</xsl:stylesheet>

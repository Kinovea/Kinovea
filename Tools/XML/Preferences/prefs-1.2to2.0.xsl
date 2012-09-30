<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

<!-- 
    www.kinovea.org

    This stylesheet formats preferences from Kinovea 0.8.18 and previous (1.x format) to 2.0 format.
    You shouldn't have to use it manually, it is processed by Kinovea automatically.
    
    2012-09-30 - Initial converter. Format 1.2 is completely covered.
-->

<xsl:template match="/KinoveaPreferences">
    <KinoveaPreferences>
        <FormatVersion>2.0</FormatVersion>
        <xsl:apply-templates select="HistoryCount"/>
        <xsl:apply-templates select="Language"/>
        <xsl:apply-templates select="TimeCodeFormat"/>
        <xsl:apply-templates select="SpeedUnit"/>
        <xsl:apply-templates select="ImageAspectRatio"/>
        <xsl:apply-templates select="DeinterlaceByDefault"/>
        <xsl:apply-templates select="WorkingZoneSeconds"/>
        <xsl:apply-templates select="WorkingZoneMemory"/>
        <xsl:apply-templates select="MaxFading"/>
        <xsl:apply-templates select="DrawOnPlay"/>
        <xsl:apply-templates select="ExplorerThumbnailsSize"/>
        <xsl:apply-templates select="ExplorerVisible"/>
        <xsl:apply-templates select="ExplorerSplitterDistance"/>
        <xsl:apply-templates select="ActiveFileBrowserTab"/>
        <xsl:apply-templates select="ExplorerFilesSplitterDistance"/>
        <xsl:apply-templates select="ShortcutsFilesSplitterDistance"/>
        <xsl:apply-templates select="Shortcuts"/>
        <xsl:apply-templates select="RecentColors"/>
        <xsl:apply-templates select="CaptureImageDirectory"/>
        <xsl:apply-templates select="CaptureVideoDirectory"/>
        <xsl:apply-templates select="CaptureImageFormat"/>
        <xsl:apply-templates select="CaptureVideoFormat"/>
        <xsl:apply-templates select="CaptureUsePattern"/>
        <xsl:apply-templates select="CapturePattern"/>
        <xsl:apply-templates select="CaptureImageCounter"/>
        <xsl:apply-templates select="CaptureVideoCounter"/>
        <xsl:apply-templates select="CaptureMemoryBuffer"/>
        <xsl:apply-templates select="DeviceConfigurations"/>
        <xsl:apply-templates select="NetworkCameraUrl"/>
        <xsl:apply-templates select="NetworkCameraFormat"/>
    </KinoveaPreferences>
</xsl:template>

<xsl:template match="HistoryCount">
    <FileExplorer>
        <MaxRecentFiles><xsl:value-of select="."/></MaxRecentFiles>
    </FileExplorer>
</xsl:template>

<xsl:template match="Language">
    <General>
        <Culture><xsl:value-of select="."/></Culture>
    </General>
</xsl:template>

<xsl:template match="TimeCodeFormat">
    <Player>
        <TimecodeFormat><xsl:value-of select="."/></TimecodeFormat>
    </Player>
</xsl:template>

<xsl:template match="SpeedUnit">
    <Player>
        <xsl:copy-of select="."/>
    </Player>
</xsl:template>

<xsl:template match="ImageAspectRatio">
    <Player>
        <AspectRatio><xsl:value-of select="."/></AspectRatio>
    </Player>
</xsl:template>

<xsl:template match="DeinterlaceByDefault">
    <Player>
        <DeinterlaceByDefault>
            <xsl:choose>
                <xsl:when test="text() = 'True' or text() = 'true' or text() = '1'"><xsl:text>true</xsl:text></xsl:when>
                <xsl:otherwise><xsl:text>false</xsl:text></xsl:otherwise>
            </xsl:choose>
        </DeinterlaceByDefault>
    </Player>
</xsl:template>

<xsl:template match="WorkingZoneSeconds">
    <Player>
        <xsl:copy-of select="."/>
    </Player>
</xsl:template>

<xsl:template match="WorkingZoneMemory">
    <Player>
        <xsl:copy-of select="."/>
    </Player>
</xsl:template>

<xsl:template match="MaxFading">
    <Player>
        <xsl:copy-of select="."/>
    </Player>
</xsl:template>

<xsl:template match="DrawOnPlay">
    <Player>
        <DrawOnPlay>
            <xsl:choose>
                <xsl:when test="text() = 'True' or text() = 'true' or text() = '1'"><xsl:text>true</xsl:text></xsl:when>
                <xsl:otherwise><xsl:text>false</xsl:text></xsl:otherwise>
            </xsl:choose>
        </DrawOnPlay>
    </Player>
</xsl:template>

<xsl:template match="ExplorerThumbnailsSize">
    <FileExplorer>
        <ThumbnailSize><xsl:value-of select="."/></ThumbnailSize>
    </FileExplorer>
</xsl:template>

<xsl:template match="ExplorerVisible">
    <General>
        <ExplorerVisible>
            <xsl:choose>
                <xsl:when test="text() = 'True' or text() = 'true' or text() = '1'"><xsl:text>true</xsl:text></xsl:when>
                <xsl:otherwise><xsl:text>false</xsl:text></xsl:otherwise>
            </xsl:choose>
        </ExplorerVisible>
    </General>
</xsl:template>

<xsl:template match="ExplorerSplitterDistance">
    <General>
        <xsl:copy-of select="."/>
    </General>
</xsl:template>

<xsl:template match="ActiveFileBrowserTab">
    <FileExplorer>
        <ActiveTab><xsl:value-of select="."/></ActiveTab>
    </FileExplorer>
</xsl:template>

<xsl:template match="ExplorerFilesSplitterDistance">
    <FileExplorer>
        <xsl:copy-of select="."/>
    </FileExplorer>
</xsl:template>

<xsl:template match="ShortcutsFilesSplitterDistance">
    <FileExplorer>
        <xsl:copy-of select="."/>
    </FileExplorer>
</xsl:template>

<xsl:template match="Shortcuts">
    <FileExplorer>
        <xsl:copy-of select="."/>
    </FileExplorer>
</xsl:template>

<xsl:template match="RecentColors">
    <Player>
        <RecentColors>
            <xsl:for-each select="Color">
                <RecentColor><xsl:value-of select="."/></RecentColor>
            </xsl:for-each>
        </RecentColors>
    </Player>
</xsl:template>

<xsl:template match="Shortcuts">
    <FileExplorer>
        <xsl:copy-of select="."/>
    </FileExplorer>
</xsl:template>

<xsl:template match="CaptureImageDirectory">
    <Capture>
        <ImageDirectory><xsl:value-of select="."/></ImageDirectory>
    </Capture>
</xsl:template>

<xsl:template match="CaptureVideoDirectory">
    <Capture>
        <VideoDirectory><xsl:value-of select="."/></VideoDirectory>
    </Capture>
</xsl:template>

<xsl:template match="CaptureImageFormat">
    <Capture>
        <ImageFormat><xsl:value-of select="."/></ImageFormat>
    </Capture>
</xsl:template>

<xsl:template match="CaptureVideoFormat">
    <Capture>
        <VideoFormat><xsl:value-of select="."/></VideoFormat>
    </Capture>
</xsl:template>

<xsl:template match="CaptureUsePattern">
    <Capture>
        <UsePattern>
            <xsl:choose>
                <xsl:when test="text() = 'True' or text() = 'true' or text() = '1'"><xsl:text>true</xsl:text></xsl:when>
                <xsl:otherwise><xsl:text>false</xsl:text></xsl:otherwise>
            </xsl:choose>
        </UsePattern>
    </Capture>
</xsl:template>

<xsl:template match="CapturePattern">
    <Capture>
        <Pattern><xsl:value-of select="."/></Pattern>
    </Capture>
</xsl:template>

<xsl:template match="CaptureImageCounter">
    <Capture>
        <ImageCounter><xsl:value-of select="."/></ImageCounter>
    </Capture>
</xsl:template>

<xsl:template match="CaptureVideoCounter">
    <Capture>
        <VideoCounter><xsl:value-of select="."/></VideoCounter>
    </Capture>
</xsl:template>

<xsl:template match="CaptureMemoryBuffer">
    <Capture>
        <MemoryBuffer><xsl:value-of select="."/></MemoryBuffer>
    </Capture>
</xsl:template>

<xsl:template match="DeviceConfigurations">
    <Capture>
        <xsl:copy-of select="."/>
    </Capture>
</xsl:template>

<xsl:template match="NetworkCameraUrl">
    <Capture>
        <xsl:copy-of select="."/>
    </Capture>
</xsl:template>

<xsl:template match="NetworkCameraFormat">
    <Capture>
        <xsl:copy-of select="."/>
    </Capture>
</xsl:template>

</xsl:stylesheet>

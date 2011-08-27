#! python
import os, glob, shutil

# usage:
# python ink2web.py > table.txt

def transform():
    '''Transforms the svg files to use viewBox instead of hard size, put them in /web subdir'''
    saxon = '"C:\\Program Files\\saxonhe9-2-1-5n\\bin\\Transform.exe"'
    if os.path.exists('web'):
        shutil.rmtree('web')
    for f in glob.glob('*.svg'):
        os.system("%s -t -s:%s -xsl:ink2web.xsl -o:web/%s" % (saxon, f, f))

def lines(groupBy):
    '''Walk the svg files and build an iterator of groups of n'''
    line = []
    for f in glob.glob('*.svg'):
        line.append(f)
        if(len(line) == groupBy):
            yield line
            line = []
    if(len(line) > 0):
        yield line

def makeHtml():
    '''Creates the html table for use on the ObsRef Repository page'''
    baseUrl = 'http://www.kinovea.org/obsref/svg/'
    thumbHtml = '\t<td style="background-color: #ffffff"><object data="%s%s" width="150" height="100" type="image/svg+xml"/></td>\n'
    legendHtml = '\t<td style="text-align: center"><a href="%s%s">%s</a></td>\n'
    
    def createRow(tdFormat, line):
        
        
        html.append('</tr>\n')

    html = []
    html.append('<table>\n')
    for line in lines(3):
        html.append('<tr>\n')
        for item in line:
            html.append(thumbHtml % (baseUrl,item))
        html.append('</tr>\n<tr>\n')
        for item in line:
            html.append(legendHtml % (baseUrl,item,os.path.splitext(item)[0]))
        html.append('</tr>\n')
    html.append('</table>')
    table = ''.join(html)
    print table

# Entry point
#transform()    # <-- Currently the transform works only for documents expressed in pixel as internal unit.
makeHtml()

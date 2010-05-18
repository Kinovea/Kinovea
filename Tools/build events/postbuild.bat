rem Post build event.
rem Recreates the guides folder and put all the svg files there.
rem Recreates the xslt folder and put all the stylesheets there.

rd /s/q guides
md guides
xcopy /s "..\..\..\..\tools\svg\*.svg" guides

rd /s/q xslt 
md xslt
xcopy /s "..\..\..\..\tools\xsl transforms\Kva to Spreadsheets\*.xsl" xslt
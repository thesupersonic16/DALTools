# PCKTool
A tool to unpack and repack .pck archives from the PC version of DATE A LIVE: Rio Reincarnation

Thanks to Sajid for helping figure out some missing stuff.

## PCKTool
PCKTool only takes in one argument which is the .pck file path for extracting or a directory to pack all the files back into a .pck

## TEXTool
TEXTool is a tool used to extract and rebuild Texture files. (Not all .tex files are are supported currently)
You can get a list of options and examples by running TEXTool.exe without any arguments
```
  TEXTool [Switches] [-o {path}] {tex file}
  Switches:
    -p                             Exports all frames
    -s                             Exports Sheet as PNG
    -i                             Exports frame information
    -f {id}                        Exports a single frame
    -b {sheet.png} {frame.xml}     Build TEX using sheet and FrameXML
    -m {path}                      Build TEX using sheet and FrameXML (Search by name)
  Examples:
    TEXTool -p title.tex           Extracts all frames
    TEXTool -s title.tex           Extracts sheet from TEX
    TEXTool -i title.tex           Extracts frame information
    TEXTool -b title.png title.xml Builds TEX using sheet
    TEXTool -m title               Builds TEX using frames
    TEXTool -f 0 title             Extracts the first frame
    TEXTool -p -o frames title.tex Extracts all frames into a folder called frames
```
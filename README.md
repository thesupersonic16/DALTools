# DALTools
A collection of tools to help assist unpack and repack files for the PC version of DATE A LIVE: Rio Reincarnation

Thanks to Sajid for helping with the missing fields.

## PCKTool
PCKTool only takes in one argument which is the .pck file path for extracting or a directory to pack all the files back into a .pck

## TEXTool
TEXTool is a tool used to extract and rebuild Texture files.  
TEXTool does support extracting and rebuilding of .tex files using drag and drop and through CLI.  
using Drag and Drop, you can extract a .tex by dragging the .tex into the exe and rebuild it by dragging its .xml, .png or frames folder into the exe.  
You can get a list of options and examples by running TEXTool.exe without any arguments
```
  TEXTool [Switches] [-o {path}] {tex file}
  Switches:
    -p                             Exports all frames
    -s                             Exports Sheet as PNG
    -i                             Exports frame information
    -f {id}                        Exports a single frame
    -b {sheet.png} {frame.xml}     Build TEX using sheet and FrameXML
    -m {path}                      Build TEX using sheet and FrameXML (Search by name) (Recommended over -b)
  Examples:
    TEXTool -p title.tex           Extracts all frames
    TEXTool -s title.tex           Extracts sheet from TEX
    TEXTool -i title.tex           Extracts frame information
    TEXTool -b title.png title.xml Builds TEX using sheet
    TEXTool -m title               Builds TEX using frames
    TEXTool -f 0 title             Extracts the first frame
    TEXTool -p -o frames title.tex Extracts all frames into a folder called frames
```

## STSCTool
STSCTool is a tool used to disassemble and reassemble scripts (.bin). (This only works with scripts from DATE A LIVE: Rio Reincarnation, other games will not work due to them having different instruction sets)  
To disassemble/reassemble supported scripts, just drag and drop the script(s) you want (.txt or .bin)

Scripts are disassembled into a custom text syntax designed to be easily read by the assembler and easy for the user to modify the code.  
  
These code files contain most of the instructions for the game to process, This includes stuff like, Story, Choices, Flagging, Music, Maps, Animation calling and etc.  
  
This repo has a small list of known functions in [FUNCTIONS.md][functions_url]

STSCTool is currently really far from being complete but is usable at this point, and outputs all the instructions in as much detail as needed to be easily reassembled.

## FontEditor - DATE A LIVE: Rio Reincarnation Font Editor
Incomplete, Can be used for editing font codes, but adding new character definitions do not work due to the game refusing to use the code for unknown reasons.


[functions_url]: ./FUNCTIONS.md

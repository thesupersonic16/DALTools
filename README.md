# DALTools
A collection of tools aimed to assist with modify files for Date A Live: Rio Reincarnation, Ren Dystopia and some other similar games. 

## Download / Building
If you are here to just use the tools, You can download the pre-built binaries from [GitHub Actions][actions_url] or compile them yourself. If the latest GitHub Actions build is expired, you may grab and older build from [releases][releases_url]. 

**Please be aware this project utilise Fody, which can often cause false positives with some Anti-Virus, feel free to submit the samples or build the code yourself**

**Releases**: You will need .NET Framework 4.8 or newer installed for them to run.
  
**Building**: You will need Visual Studio 2022 or newer installed with the .NET Framework 4.8 Targeting Pack installed. 

## Issues / Requests
If you have found any issues with any of the tools in the repository or would like to request a feature or etc., Please don't hesitate to [submit an issue][newIssue_url]. 
When submitting an issue or a request, It is important to provide as much details as possible as it will help us to resolve your issue.

## PCKTool
PCKTool is a CLI tool which is designed to unpack and repack .pck archives seen in many of the Date A Live visual novels and some other games. 
For PC/PS4 .pck(s), you can unpack the archives by just dragging the archive file into PCKTool.exe. Repacking is the same process except you drag the archive folder into the executable. 
 
For Big Endian .pck(s) like the ones seen on the PS3, You will need to add the -e switch in the command line like ``PCKTool.exe -e Script.pck``

Some other games may also use the same PCK format but with a different header size and padding. Make sure to check the help page by running the tool with no arguments.

Special thanks to Sajid for helping me with figuring out the missing fields.


## TEXTool
TEXTool is a tool used to extract and rebuild texture files.  
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
    -e                             Read/Write in Big Endian (PS3) Default is Little Endian (PC/PS4)
    -c                             Disable overwrite check
    -o {path}                      Sets the output path
    -z                             Output 0x08 padded signatures over the default 0x14 (DAL: RR uses 0x14)
  Examples:
    TEXTool -p title.tex           Extracts all frames
    TEXTool -s title.tex           Extracts sheet from TEX
    TEXTool -s -e title.tex        Extracts sheet from a BE TEX (PS3)
    TEXTool -i title.tex           Extracts frame information
    TEXTool -b title.png title.xml Builds TEX using sheet
    TEXTool -m title               Builds TEX using frames
    TEXTool -f 0 title             Extracts the first frame
    TEXTool -p -o frames title.tex Extracts all frames into a folder called frames
```

## STSCTool
STSCTool is a tool used to disassemble and reassemble scripts (.bin), this tool will only accept scripts from Rio Reincarnation, other games will not work due to them having different instruction sets or uses a newer script format. 
 
To disassemble/reassemble scripts, just drag and drop the script(s) you want into STSCTool.exe which should generate a .txt or .bin file.
 
Scripts disassembled with STSCTool will produce a text file with a custom text syntax designed to be easily read back by the assembler and to be easy for the user to modify the code.

NOTE: If you are planning to using this tool for translation, Please checkout [ScriptDialogueEditor][scriptdialogueeditor_info_url] which is designed to make text editing process much easier to work with. 
  
These code files contain most of the instructions for the game to process, This includes stuff like, Story, Choices, Flagging, Music, Maps, Animation calling and etc. 
 
This repo has a small list some known functions in [FUNCTIONS.md][functions_url]

## FontEditor - Date A Live: Rio Reincarnation Font Editor
Incomplete, Can be used for editing font codes, but adding new character definitions do not work due to the game refusing to use the code for unknown reasons.
 
## ScriptDatabaseEditor - Date A Live: Rio Reincarnation Script Database Editor
A GUI tool made to allow editing the database.bin file located in the Script directory. The database file contains information mainly extras, menu and some common definitions like character and voice names. 
 
Note: This tool can also preview some resources if Rio Reincarnation is found installed.
 
Using this tool is easy, drag your database.bin file onto the exe file or run it alone for it to load your current script archive from your game installation (By default English will be loaded). 

![Screenshot of ScriptDatabaseEditor viewing ][scriptdatabaseeditor_screenshot_00]

## ScriptDialogueEditor - DALTools Script Dialogue Editor
A GUI tool made to help simplify the process of editing the text within the scripts by basically integrating many of the tools needed into one GUI application. 

ScriptDialogueEditor currently only supports Rio Reincarnation and Ren Dystopia
 
ScriptDialogueEditor also features:
 - Loads/Saves scripts in memory so you won't need to keep rebuilding the .bin file and .pck file manually
 - Most of the confusing/unnecessary code has been filtered out, only showing the information needed by most use cases.
 - Text exporting and importing support (currently only .tsv, .csv, .po and .xlsx(import only) are supported)
 - Experimental Live Preview, allowing for live message editing (Just edit the text while the feature is active)
   - This feature is only available for Rio Reincarnation
 
![Screenshot of ScriptDialogueEditor][scriptdialogueeditor_screenshot_00]

## TableEditor - DALTools DALTools Table Editor
A simple GUI tool to allow editing table files found within Ren Dystopia's game files.  

![Screenshot of TableEditor][tableeditor_screenshot_00]


 
[scriptdatabaseeditor_screenshot_00]: ./Images/ScriptDatabaseEditor_Screenshot_00.png
[scriptdialogueeditor_screenshot_00]: ./Images/ScriptDialogueEditor_Screenshot_00.png
[scriptdialogueeditor_info_url]: #scriptdialogueeditor---date-a-live-rio-reincarnation-script-dialogue-editor
[tableeditor_screenshot_00]: ./Images/TableEditor_Screenshot_00.png
[functions_url]: ./FUNCTIONS.md
[releases_url]: ../../releases
[newIssue_url]: ../../issues/new
[actions_url]: https://nightly.link/thesupersonic16/DALTools/workflows/build/master/DALTools-Release.zip

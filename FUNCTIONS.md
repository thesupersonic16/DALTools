# DAL: RR Script Basic Function List
A little list of some known script functions used in DATE A LIVE: Rio Reincarnation  
A lot of assumptions are being used in this list which could be wrong and please use common sense when using this list.

## Exit()
Exits the script and returns to Title  
Example:
```csharp
Exit()
```

## Wait(Int32: delay)
Halts until `delay` milliseconds has passed  
Example:
```csharp
// Waits for 1 second
Wait(60)
```

## Goto(Pointer: location)
Sets the Instruction Pointer to `location`  
Example:  
```csharp
// Jumps to label
Goto(LABEL_063F)
or
// Jump to position (0x3C is the location of the first instuction)
Goto(0x3C)
```

## RandJump(PointerArray: locations)
Randomly picks a location in the `locations` array and sets the Instruction Pointer to it  
Example:  
```csharp
// In this example the game will randomly pick any one of the 5 locations and jump to it
RandJump(LABEL_0611, LABEL_063F, LABEL_066D, LABEL_069B, LABEL_06C9)
```

## Printf(String: text)
Unused, Might be used to output text on debug builds  
Example:  
```csharp
// Writes "TESTSCRIPT" into the console if enabled
Printf("TESTSCRIPT")
```

## FileJump(String: file)
Loads and jumps to the start of a language script file `file`  
Note: `file` is an absoute path inside of the PCK and must not contain an extension  
Example:  
```csharp
// In this example the game will load Data/{language}/Script/Script.pck/3rd/digest03.bin and start execution at the start of the file
FileJump("3rd/digest03")
```

## Call(Pointer: location)
Calls a function(Sub) by storing the address of the next instruction into the stack and setting the Instruction Pointer to `location`  
Note: Make sure at the end of the function to call CallReturn()  
Example:  
```csharp
// Calls the Sub with the name of SUB_089B
Call(SUB_089B)
```

## CallReturn()
Sets the Instruction Pointer to the address after the Call  
Note: Must be used at the end of a function(Sub)  
Example:  
```csharp
// Nothing else to say, its like a normal return keyword
CallReturn()
```

## PlayMovie(String: movieName, Int16: unknown01, Byte: unknown02)
Plays language movie file from Data/{language}/Movie/`movieName`.movie   
Note: `movieName` is relative to the language movie folder and must not contain the extension  
Example:  
```csharp
// Plays Opening theme Data/{language}/Movie/3rd_op.movie
PlayMovie("3rd_op", 1, 0)
```

## SePlay(Byte: soundFXID, Int16: unknown01, Bool: loop)
Plays sound effect from Data/Se.pck/`soundFXID:D4`.snd   
Note: If `loop` is set to true, You will need to call SeStop(Byte, Byte) to stop the playback  
Example:  
```csharp
SePlay(158, 0, false)
or
SePlay(126, 0, true)
```

## SeStop(Byte: soundFXID, Int16: unknown01)
Stops playback of `soundFXID`    
Example:  
```csharp
SeStop(126, 0)
```

## Mes(Byte: positionX, Bool: centerX, Byte: positionY, Bool: centerY, String: text, Int16: messageID)
Writes `text` at the position of `positionX` and `positionY` and is centered if `centerX` or `centerY` is set to true  
Note: This does not include waiting for user input, That job is for MesWait()  
Note: `messageID` is used for tracking if a message has been read in a script   
Note: You can use MesDel() instead if you only want to clear the textbox without waiting.
Example:  
```csharp
// Writes "I choose... both of you!" to the top of the textbox
Mes(80, false, 167, true, "I choose... both of you!", 17)
```

## MesWait()
Waits for user input or continue if skip is enabled, After called, the next Mes(Byte, Bool, Byte, Bool, String, Int16) call will reset all the text  
Note: This should be used after Mes(Byte, Bool, Byte, Bool, String, Int16) to let time for readers to read  
Example:  
```csharp
// Waits for user input or skip if skipping is enabled
MesWait()
```

## MesTitle(Byte: characterNameID)
Sets the title of the dialogue to `characterNameID`  
Note: Use of CharacterNames Macros are allowed which can be found in [STSCMacros.cs][macros_url]  
Note: To disable titles, use the id of 255 or CHAR_NONE  
Example:  
```csharp
// Shows YAMAI on the title of the message box
MesTitle(0x32)
or
// Shows YAMAI on the title of the message box (same as above but with a macro)
MesTitle(CHAR_YAMAI)
or
// Disables the title of the message box
MesTitle(CHAR_NONE)
```

## SetChoice(Pointer: location, String: text, Int16: choiceID)
Adds a choice option with the text `text`, When Selected with ShowChoices(Bool) the game will jump to its `location`. `choiceID` is used to track if the option has been used  
Note: This must be used before ShowChoices(Bool) is called for choices to appear  
Example:  
```csharp
// Adds "Go on a date with the Origami." and "Go on a date with the Yamai Sisters."
//  to the list of options. 
SetChoice(LABEL_063F, "Go on a date with the Origami.", 0)
SetChoice(LABEL_066D, "Go on a date with the Yamai Sisters.", 1)
```

## ShowChoices(Bool: unknown)
Displays a list of choices and waits for the user to pick a choice, and jumps to the selected choice's location  
Note: After the user chooses an option, the choices list is then reset, ready for the next calls of SetChoice(Pointer, String, Int16)  
Example:  
```csharp
// Adds two choices to the list
SetChoice(LABEL_017D, "Start from prologue.", 0)
SetChoice(LABEL_0128, "Skip prologue.", 1)
// Displays the choices and waits for a selected option, and jumps to its location
ShowChoices(true)
```

## SetFontSize(Byte: fontSize)
Sets the current font size to `fontSize`. Size 52 is common throughout the whole game  
Note: You should always call this atleast once at the start of the script to ensure text is shown correctly  
Example:  
```csharp
// Sets the font size to 52 which is common
SetFontSize(52)
or
// Sets the font size to 104 which is twice as large as the common size
SetFontSize(104)
```

## PlayVoice(Byte: unknown00, Int32: unknown01, String name)
Loads voice file `name` and plays it once  
Note: `name` is a relative path inside of the game folder in the Voice PCK and must not contain the extension   
Example:  
```csharp
// In the example when playing RR (3rd) the game will load Data/Voices.pck/3rd/900_00_00_136_23.snd
//  when playing RU the folder name is 1st, AI is 2nd and RR is 3rd
PlayVoice(255, 0, "900_00_00_136_23")
```

## Trophy(Byte: trophyID)
Unlocks Trophy (Achievement). Please do not mess around with this function.  
Note: Achievement names are hardcoded in the game's executable, `trophyID` is used as an index to an array of achievement names   
Example:  
```csharp
// This will unlock the "Start of Eden" Trophy
Trophy(1)
```



[macros_url]: ./DALTools/DALLib/Scripting/STSCMacros.cs 

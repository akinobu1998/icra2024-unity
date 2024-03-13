# Interactive Customer Service

This is a Unity project for Interactive Customer Service task.  
The task is one of the competition tasks in "Future Convenience Store Challenge in Cyber Space".

Please refer to [the rulebook](https://github.com/FCSCinCyberSpace/documents) and [wiki](https://github.com/FCSCinCyberSpace/icra2024-unity/wiki) for details of the competition.

## Prerequisites

See below for OS and Unity version.  
https://github.com/FCSCinCyberSpace/documents/blob/main/SoftwareManual/Environment.md#windows-pc

## How to Clone

This repository uses the submodule function, so please clone it with the following command.

```bash:
git clone --recursive https://github.com/FCSCinCyberSpace/icra2024-unity
```

## How to Build

### Import Executable file and Dll for TTS

Please follow these two steps.

#### Windows Settings
Please install the English language if you are using other than English.  
The procedure is like as follows.
1. Open the Windows settings menu
1. Click [Time & Language] - [Region & language]
1. Click [Add a language] in [Languages]-[Preferred languages]
1. Select "English (United States)" and Install

#### Import Files
Please import files by following the steps below.
1. Prepare "ConsoleSimpleTTS.exe" and "Interop.SpeechLib.dll".  
For details on these files, see [here](https://github.com/RoboCupatHomeSim/console-simple-tts).
1. Copy those files to the "TTS" folder in the same directory as SIGVerseConfig folder.


### Build
1. Create a "Build" folder under this project folder.
1. Open this project with Unity.
1. Click [File]-[Build Settings].
1. Click [Build]
1. Select the "Build" folder , and type a file name (e.g. InteractiveCustomerService) and save the file.
1. Copy the "TTS" folder under the "Build" folder.

## How to Set Up

### Modify Configuration

1. Open this project with Unity.
1. Click [SIGVerse]-[SIGVerse Settings].  
SIGVerse window will be opened.
1. Type the IP address of ROS to "Rosbridge IP" in SIGVerse window.

## How to Execute

Please start the ROS side application beforehand.  
See [icra2024-ros](https://github.com/FCSCinCyberSpace/icra2024-ros) for an example application.

### Execute on Unity Editor
1. Click [SIGVerse]-[Set Default GameView Size].
1. Double click "Assets/Competition/Scenes/InteractiveCustomerService(.unity)" in Project window.
1. Click the Play button at the top of the Unity editor.

### Execute the Executable file
1. Double Click the "InteractiveCustomerService.exe" in the "Build" folder.

## License

This project is licensed under the SIGVerse License - see the LICENSE.txt file for details.

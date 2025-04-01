# Biped.UI.Fixes.Plugin.Mono
[![Github All Releases](https://img.shields.io/github/downloads/JoeTomkinson/Biped.UI.Fixes.Plugin/total.svg)]()

**Updated Biped UI Fix Plugin - Inspiration and updated based on the original Lyall BipedFix plugin**

![biped ui fix](https://github.com/user-attachments/assets/0b5f5bd2-436c-4de2-85f3-8a0f1f8f15d2)

## Version Details

**Current Version: 1.0.1**

- Unity Version: 2022.3.20
- Biped Version: 2.6
- BepInEx - 6.0.0-be.697
- CLR runtime version: 4.0.30319.42000
- Supports x64 (64 bit) and x86 (32 bit) versions of the game.

## Overview

BipedWideScreenFix is a plugin designed to enhance the visual experience of the game *Biped* on ultra-wide monitors. This updated version improves upon the original Lyall BipedFix plugin by incorporating more modern .NET Standard 2.1 practices, dynamic configuration binding, and enhanced UI adjustments. The plugin ensures that screen resolution, aspect ratios, and UI elements are properly scaled and positioned, providing a seamless ultra-wide display experience.

## Features

- **Ultra-Wide Monitor Support:**  
  Automatically adjusts UI elements and screen resolution to cater for ultra-wide displays.
  
- **Dynamic Configuration Binding:**  
  Uses BepInEx’s configuration system to bind user-defined values at runtime. Changes in the configuration file are immediately reflected in the plugin's behaviour when reloaded.
  
- **Enhanced Stability and Robustness:**  
  Implements comprehensive error checking (null checks and logging) to safeguard against missing GameObjects or components.
  
- **Optimized Performance:**  
  Consolidates duplicate logic (e.g., frame rate management) using helper methods, ensuring cleaner and more maintainable code.
  
- **Targeting .Net Standard 2.1**  
  Updated to target `.NET Standard 2.1` and leverage current Unity functionalities, ensuring improved compatibility and performance.

## Installation

1. **Download the Plugin**  
   Visit the [BipedWideScreenFix GitHub releases page](https://github.com/JoeTomkinson/BipedWideScreenFix/releases) and download the latest release ZIP file for either x64 bit or x86 bit (32 bit) depending on what you're running the game on.

2. **Extract the Files**  
   Extract the contents of the ZIP file into your game directory (e.g., `steamapps/common/Biped`). This ensures that the plugin DLL and related files are placed in the correct folder structure.

   -  **Important Note** Do not use the 'Extract All' option on the Zip file as it'll put the files into a directory of the same name as the zip package.
   -  The Directory **should look like this** if extracted properly:
  
     ![image](https://github.com/user-attachments/assets/5fc38171-1208-431c-8e4f-530162d13ca8)

4. **Initial Run**  
   Launch the game once. This initial run allows BepInEx to load the plugin and automatically generate a configuration file at:

   ``` c#
   Biped/BepInEx/config/Biped.UI.Fixes.Plugin.Mono.cfg
   ```

5. **Configuration**  
   Open the generated configuration file to adjust various settings—such as resolution, fullscreen mode, UI fixes, frame rate, and VSync—according to your system and preferences.


### Debugging

By default the debugging console is turned on in BepInEx, this will output to the BepInEx log file. If you're having issues with the plugin, please check the log file for any errors or issues.

**If you would like to turn this off, follow the below instructions:**

1. Navigate to `'Biped\BepInEx\config'`
2. Open up the `'BepInEx.cfg'` file in NotePad or NotePad++ (Any text editor will work).
3. Amend the following line to be `'false'` if it's not already:
   
   ```md
    [Logging.Console]
	## Enables showing a console for log output.
	# Setting type: Boolean
	# Default value: true
	Enabled = false
   ```

## Configuration Options

The plugin exposes several configuration entries that allow users to tailor the display settings:

- **ResolutionWidth (DesiredResolutionX):**  
  The desired width for the game’s resolution.  
  _Default:_ Set to the display's current width.
  
- **ResolutionHeight (DesiredResolutionY):**  
  The desired height for the game’s resolution.  
  _Default:_ Set to the display's current height.
  
- **Fullscreen:**  
  Boolean value determining whether the game runs in fullscreen mode.  
  _Default:_ `true`
  
- **UIFixes:**  
  Toggle to enable or disable UI adjustments for ultra-wide monitors.  
  _Default:_ `true`
  
- **UnlockedFPS:**  
  Sets the desired frame rate limit.  
  _Default:_ `120`
  
- **EnableVSync (ToggleVSync):**  
  Boolean to enable or disable VSync.  
  _Default:_ `true`

## Usage

Once installed and configured, simply start the game. The plugin will automatically:

- Set the screen resolution and fullscreen mode as specified.
- Dynamically compute and apply the correct aspect ratio adjustments for UI elements.
- Adjust frame rate settings by applying a unified framerate configuration across different game states.
- Log all adjustments to the BepInEx log file, making troubleshooting straightforward.

## Differences from the Original Plugin

### Initialisation and Configuration Binding

- **Original:**  
  Configuration values were bound in the `Awake()` method, using static fields that computed values only once.
  
- **Updated:**  
  The revised version binds configuration entries in the `Awake()` method and additionally, computed values (such as aspect ratio and multiplier) are now properties rather than static fields, ensuring that any change in configuration is immediately reflected without needing a restart.

### Code Structure and Maintainability

- **Original:**  
  Duplicate logic was spread across multiple Harmony patches (for example, setting the frame rate in several places).
  
- **Updated:**  
  A helper method (`SetFramerate()`) has been introduced to consolidate frame rate management. This reduces redundancy and makes the codebase more maintainable.

### Error Handling and Logging

- **Original:**  
  Minimal error handling for GameObject retrieval; missing objects could lead to null reference exceptions.
  
- **Updated:**  
  Enhanced robustness by adding null-checks and detailed logging for every GameObject and component retrieval. This prevents runtime errors and aids in debugging by clearly indicating when a component is not found.

### Documentation and Readability

- **Original:**  
  Limited inline comments and method summaries.
  
- **Updated:**  
  Comprehensive XML documentation comments and inline comments have been added. Each method now includes a clear summary of its purpose and behaviour, improving readability and ease of future modifications.

### Framework and Best Practices

- **Original:**  
  The plugin was built on an older framework and had static configurations.
  
- **Updated:**  
  Now targeting .NET Standard 2.1, leveraging modern C# features and best practices, which not only improves performance and compatibility with current Unity versions but also sets a foundation for easier future enhancements.

## Changelog

- v1.0.0 - Initial Release
- v1.0.1 - Updated to .NET Standard 2.1 and added additional configuration options. Added more detailed logging and error handling. Updated to target Unity Version 2022.3.20. Added updated game dlls for Biped version 2.6

## Known Issues

The below are some of the known issues that may occur when using the plugin; they specifically relate to the UI adjustments and screen resolution settings for the Main UI Screens.

- **Main UI Screen:** The main UI screen may not be perfectly centered vertically on ultra-wide monitors. This is due to the game's UI design and the limitations of adjusting the screen resolution and UI elements. The plugin attempts to center the UI as best as possible, but some minor misalignment may still occur.

## License

This project is licensed under the [MIT License](LICENSE).

## Credits

- **Original Plugin:**  
  Thanks to Lyall for the original BipedFix plugin inspiration [found Here](https://github.com/Lyall/BipedFix).
  
- [BepinEx](https://github.com/BepInEx/BepInEx) is licensed under the GNU Lesser General Public License v2.1.

- **Contributors:**  
  
  - Andrew – for pointing me in the direction of the original version of the UI Fix Plugin and still playing co-op games with me nearly a decade on.

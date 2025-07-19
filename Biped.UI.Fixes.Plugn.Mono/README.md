# Biped.UI.Fixes.Plugin.Mono
[![Github All Releases](https://img.shields.io/github/downloads/JoeTomkinson/Biped.UI.Fixes.Plugin/total.svg)]()

**Updated Biped UI Fix Plugin - Inspiration and updated based on the original Lyall BipedFix plugin**

![biped ui fix](https://github.com/user-attachments/assets/0b5f5bd2-436c-4de2-85f3-8a0f1f8f15d2)

## Version Details

**Current Version: 1.1.1**

- Unity Version: 2022.3.20f1
- Biped Version: 2.6+
- BepInEx - 6.0.0-be.697
- CLR runtime version: 4.0.30319.42000
- Supports x64 (64 bit) and x86 (32 bit) versions of the game.

## Overview

BipedWideScreenFix is a plugin designed to enhance the visual experience of the game *Biped* on ultra-wide monitors. This updated version improves upon the original Lyall BipedFix plugin by incorporating more modern .NET Standard 2.1 practices, dynamic configuration binding, enhanced UI adjustments, and **revolutionary game update resilience**. The plugin ensures that screen resolution, aspect ratios, and UI elements are properly scaled and positioned, providing a seamless ultra-wide display experience that survives game updates.

## New Features in v1.1.0

### **Game Update Resilience System**
- **Automatic Game Version Detection**: Detects game assembly changes and adapts accordingly
- **Multi-Layer Fallback System**: If Harmony patches fail, automatically switches to alternative methods
- **Persistent Framerate Enforcement**: Continuously monitors and maintains your settings even if the game tries to override them
- **Enhanced Error Recovery**: Graceful degradation when game updates break specific features

### **Advanced Troubleshooting**
- **Comprehensive Class Verification**: Validates all required game components before patching
- **Detailed Diagnostic Logging**: Enhanced logging shows exactly what's working and what's not
- **Safe Patch Execution**: Each patch operation is isolated to prevent cascading failures
- **Experimental Hex Patching**: Optional advanced mode for maximum compatibility (use with caution)

### **SoftMask Material Fixes**
- **Automatic TextMeshPro Material Compatibility**: Fixes SoftMask warnings with Axion RND SDF materials
- **Dynamic Material Detection**: Uses reflection to detect and fix problematic materials
- **Hierarchy-Based Filtering**: Only fixes materials when SoftMask is actually present

## Features

- **Ultra-Wide Monitor Support:**  
  Automatically adjusts UI elements and screen resolution to cater for ultra-wide displays.
  
- **Dynamic Configuration Binding:**  
  Uses BepInEx's configuration system to bind user-defined values at runtime. Changes in the configuration file are immediately reflected in the plugin's behaviour when reloaded.
  
- **Game Update Resilience:**  
  **NEW** Advanced multi-layer system that automatically adapts to game updates, ensuring continued functionality even when the game changes.
  
- **Enhanced Stability and Robustness:**  
  Implements comprehensive error checking (null checks and logging) to safeguard against missing GameObjects or components.
  
- **Optimized Performance:**  
  Consolidates duplicate logic (e.g., frame rate management) using helper methods, ensuring cleaner and more maintainable code.
  
- **Material Compatibility Fixes:**  
  **NEW!** Automatically resolves SoftMask material compatibility issues with TextMeshPro components.
  
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
Biped/BepInEx/config/Biped.UI.Fixes.Plugin.Mono.cfg

5. **Configuration**  
   Open the generated configuration file to adjust various settings—such as resolution, fullscreen mode, UI fixes, frame rate, and VSync—according to your system and preferences.

### Debugging

By default the debugging console is turned on in BepInEx, this will output to the BepInEx log file. If you're having issues with the plugin, please check the log file for any errors or issues.

**Enhanced Diagnostics in v1.1.0:**
- The plugin now provides detailed diagnostic information about game version detection
- Patch application status is logged individually for easier troubleshooting
- Fallback method activation is clearly indicated in the logs

**If you would like to turn this off, follow the below instructions:**

1. Navigate to `'Biped\BepInEx\config'`
2. Open up the `'BepInEx.cfg'` file in NotePad or NotePad++ (Any text editor will work).
3. Amend the following line to be `'false'` if it's not already:
    [Logging.Console]
	- Enables showing a console for log output.
	- Setting type: Boolean
	- Default value: true
	- Enabled = false

## Configuration Options

The plugin exposes several configuration entries that allow users to tailor the display settings:

### General Settings

- **ResolutionWidth (DesiredResolutionX):**  
  The desired width for the game's resolution.  
  _Default:_ Set to the display's current width.
  
- **ResolutionHeight (DesiredResolutionY):**  
  The desired height for the game's resolution.  
  _Default:_ Set to the display's current height.
  
- **Fullscreen:**  
  Boolean value determining whether the game runs in fullscreen mode.  
  _Default:_ `true`
  
- **UnlockedFPS:**  
  Sets the desired frame rate limit.  
  _Default:_ `120`
  
- **EnableVSync (ToggleVSync):**  
  Boolean to enable or disable VSync.  
  _Default:_ `true`

### Tweaks Settings

- **UIFixes:**  
  Toggle to enable or disable UI adjustments for ultra-wide monitors.  
  _Default:_ `true`

- **FixSoftMaskMaterials:** **NEW!**  
  Automatically fixes SoftMask material compatibility issues with TextMeshPro components.  
  _Default:_ `true`

### Advanced Settings

- **EnableHexPatching:** **NEW! EXPERIMENTAL**  
  Enable experimental hex patching as last resort for framerate unlock. **USE WITH CAUTION!**  
  _Default:_ `false`

> **⚠️ Warning:** Hex patching is an experimental feature that directly modifies game memory. Only enable this if other methods fail and you understand the risks. This feature is disabled by default for safety.

## Usage

Once installed and configured, simply start the game. The plugin will automatically:

- **Detect the game version** and log compatibility information
- **Verify game components** before attempting to patch them
- **Apply Harmony patches** for optimal integration, with automatic fallback if they fail
- **Set the screen resolution** and fullscreen mode as specified
- **Dynamically compute and apply** the correct aspect ratio adjustments for UI elements
- **Fix SoftMask material issues** automatically in the background
- **Continuously monitor and enforce** frame rate settings throughout gameplay
- **Log all adjustments** to the BepInEx log file, making troubleshooting straightforward

### Resilience Features

The plugin now automatically handles game updates through multiple fallback layers:

1. **Primary Mode**: Harmony patches (preferred method)
2. **Fallback Mode**: Direct Unity API calls if patches fail
3. **Persistent Mode**: Continuous monitoring and re-application of settings
4. **Last Line Mode**: Experimental hex patching (if enabled)

## Differences from the Original Plugin

### Game Update Resilience **NEW**

- **Original:**  
  Plugin would break completely when game updates changed the target classes or methods.
  
- **Updated:**  
  Multi-layer resilience system with automatic game version detection, fallback mechanisms, and persistent enforcement. The plugin should handle game updates more gracefully.

### Advanced Error Handling **Enhanced**

- **Original:**  
  Minimal error handling for GameObject retrieval; missing objects could lead to null reference exceptions.
  
- **Updated:**  
  Comprehensive error handling with safe patch execution, individual patch status tracking, and graceful degradation when components are missing.

### Material Compatibility **NEW**

- **Original:**  
  No handling of SoftMask material compatibility issues.
  
- **Updated:**  
  Automatic detection and fixing of TextMeshPro material compatibility issues with SoftMask components.

### Initialisation and Configuration Binding

- **Original:**  
  Configuration values were bound in the `Awake()` method, using static fields that computed values only once.
  
- **Updated:**  
  The revised version binds configuration entries in the `Awake()` method and additionally, computed values (such as aspect ratio and multiplier) are now properties rather than static fields, ensuring that any change in configuration is immediately reflected without needing a restart.

### Code Structure and Maintainability

- **Original:**  
  Duplicate logic was spread across multiple Harmony patches (for example, setting the frame rate in several places).
  
- **Updated:**  
  A helper method (`SetFramerate()`) has been introduced to consolidate frame rate management. Safe execution wrappers prevent individual patch failures from affecting the entire system.

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

## Troubleshooting

### Plugin Not Working After Game Update

**v1.1.0 includes advanced resilience features to handle this automatically!**

1. **Check the BepInEx logs** - Look for game version detection and fallback activation messages
2. **Verify fallback activation** - The plugin should automatically switch to alternative methods
3. **Check patch status** - Individual patch success/failure is now logged separately
4. **Consider hex patching** - As a last resort, you can enable experimental hex patching (use with caution)

### SoftMask Material Warnings

These are now automatically handled by the plugin! If you still see warnings:

1. Ensure `FixSoftMaskMaterials` is set to `true` in your config
2. Check the logs for material fix application messages
3. The plugin uses reflection to detect and fix these issues automatically

## Changelog

- **v1.1.0** - **Major Update: Game Update Resilience**
  - Added better game update resilience system with multi-layer fallbacks
  - Implemented automatic game version detection and compatibility checking
  - Added SoftMask material compatibility fixes for TextMeshPro components
  - Introduced experimental hex patching framework for maximum compatibility
  - Enhanced error handling with safe patch execution and status tracking
  - Added comprehensive class and method verification before patching
  - Implemented persistent framerate enforcement system
  - Added detailed diagnostic logging for troubleshooting
  - Enhanced configuration with advanced options section

- **v1.0.1** - Updated to .NET Standard 2.1 and added additional configuration options. Added more detailed logging and error handling. Updated to target Unity Version 2022.3.20. Added updated game dlls for Biped version 2.6

- **v1.0.0** - Initial Release

## Known Issues

### Minor UI Alignment Issues

- **Main UI Screen:** The main UI screen may not be perfectly centered vertically on ultra-wide monitors. This is due to the game's UI design and the limitations of adjusting the screen resolution and UI elements. The plugin attempts to center the UI as best as possible, but some minor misalignment may still occur.

### Experimental Features

- **Hex Patching:** This is an experimental feature that directly modifies game memory. While the framework is in place, actual implementation is limited for safety reasons. Only enable if you understand the risks and other methods have failed.

## License

This project is licensed under the [MIT License](LICENSE).

## Credits

- **Original Plugin:**  
  Thanks to Lyall for the original BipedFix plugin inspiration [found Here](https://github.com/Lyall/BipedFix).
  
- [BepinEx](https://github.com/BepInEx/BepInEx) is licensed under the GNU Lesser General Public License v2.1.

- **Contributors:**  
  
  - [SuperSamus](https://github.com/SuperSamus) for pointing out a manual work around for VSYNC that has been included as a fallback option in the plugin.
  - Andrew – for pointing me in the direction of the original version of the UI Fix Plugin and still playing co-op games with me nearly a decade on.
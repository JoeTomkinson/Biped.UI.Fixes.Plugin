# BipedWideScreenFix

**Updated plugin inspiration and updated based on the original Lyall BipedFix plugin**

## Overview

BipedWideScreenFix is a plugin designed to enhance the visual experience of the game *Biped* on ultra-wide monitors. This updated version improves upon the original Lyall BipedFix plugin by incorporating modern .NET6 practices, dynamic configuration binding, and enhanced UI adjustments. The plugin ensures that screen resolution, aspect ratios, and UI elements are properly scaled and positioned, providing a seamless ultra-wide display experience.

## Features

- **Ultra-Wide Monitor Support:**  
  Automatically adjusts UI elements and screen resolution to cater for ultra-wide displays.
  
- **Dynamic Configuration Binding:**  
  Uses BepInEx’s configuration system to bind user-defined values at runtime. Changes in the configuration file are immediately reflected in the plugin's behaviour when reloaded.
  
- **Enhanced Stability and Robustness:**  
  Implements comprehensive error checking (null checks and logging) to safeguard against missing GameObjects or components.
  
- **Optimized Performance:**  
  Consolidates duplicate logic (e.g., frame rate management) using helper methods, ensuring cleaner and more maintainable code.
  
- **Modern .NET6 and Unity Support:**  
  Updated to target .NET6 and leverage current Unity functionalities, ensuring improved compatibility and performance.

## Installation

1. **Download the Plugin**  
   Visit the [BipedWideScreenFix GitHub releases page](https://github.com/JoeTomkinson/BipedWideScreenFix/releases) and download the latest release ZIP file.

2. **Extract the Files**  
   Extract the contents of the ZIP file into your game directory (e.g., `steamapps/common/Biped`). This ensures that the plugin DLL and related files are placed in the correct folder structure. 
   
	-  **Importnat Note** Do not use the 'Extract All' option on the Zip file as it'll put the files into a directory of the same name as the zip package.

3. **Initial Run**  
   Launch the game once. This initial run allows BepInEx to load the plugin and automatically generate a configuration file at:

   ``` c#
   Biped/BepInEx/config/BipedWideScreenFix.cfg
   ```

4. **Configuration**  
   Open the generated configuration file to adjust various settings—such as resolution, fullscreen mode, UI fixes, frame rate, and VSync—according to your system and preferences.

## BiPed Version

### Testing working with Biped version:

- Build ID: 16581518
- App ID: 1071870

#### This can be found by:

1) Right clicking the game in your steam library and selecting properties. 
2) Navigate to Updates Blade Tab.
3) App ID and Build ID are on this page.

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

### Initialization and Configuration Binding

- **Original:**  
  Configuration values were bound in the `Awake()` method, using static fields that computed values only once.
  
- **Updated:**  
  The revised version binds configuration entries in the `Load()` method, which is more in line with modern BepInEx practices. Additionally, computed values (such as aspect ratio and multiplier) are now properties rather than static fields, ensuring that any change in configuration is immediately reflected without needing a restart.

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
  Now targeting .NET6, leveraging modern C# features and best practices, which not only improves performance and compatibility with current Unity versions but also sets a foundation for easier future enhancements.

## Changelog

- **v1.0.1:**  
  - Refactored configuration binding to occur in `Load()` rather than `Awake()`.
  - Converted computed configuration values into properties for dynamic recalculation.
  - Consolidated frame rate adjustments into a single helper method.
  - Added comprehensive error handling and logging for UI component adjustments.
  - Enhanced inline documentation and code clarity.

- **v1.0.0:**  
  - Initial release based on the original Lyall BipedFix plugin with basic ultra-wide support.

## License

This project is licensed under the [MIT License](LICENSE).

## Credits

- **Original Plugin:**  
  Thanks to Lyall for the original BipedFix plugin inspiration [found Here](https://github.com/Lyall/BipedFix).
  
- [BepinEx](https://github.com/BepInEx/BepInEx) is licensed under the GNU Lesser General Public License v2.1.

- **Contributors:**  
 
 1) **Joe** – for the updated version and ongoing improvements.
 2) **Andrew B** – for pointing me to the original version and still playing co-op games with me nearly a decade on.
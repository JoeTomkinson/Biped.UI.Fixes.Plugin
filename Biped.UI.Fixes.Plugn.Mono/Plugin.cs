using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UI;
using System.Reflection;
using System;
using System.IO;
using System.Security.Cryptography;

namespace Biped.UI.Fixes.Plugin.Mono
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        /// <summary>
        /// Logger instance for plugin logging.
        /// </summary>
        internal static new ManualLogSource Logger { get; private set; }

        /// <summary>
        /// Static reference to the plugin instance.
        /// </summary>
        public static Plugin Instance { get; private set; }

        // Configuration entries for the plugin
        public static ConfigEntry<bool> UIFixes { get; private set; }
        public static ConfigEntry<bool> FixSoftMaskMaterials { get; private set; }
        public static ConfigEntry<int> UnlockedFPS { get; private set; }
        public static ConfigEntry<bool> ToggleVSync { get; private set; }
        public static ConfigEntry<bool> Fullscreen { get; private set; }
        public static ConfigEntry<float> DesiredResolutionX { get; private set; }
        public static ConfigEntry<float> DesiredResolutionY { get; private set; }
        public static ConfigEntry<bool> EnableHexPatching { get; private set; }

        // Game version detection
        private static string _detectedGameVersion;
        private static string _assemblyHash;

        private void Awake()
        {
            // Plugin startup logic
            Instance = this;
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            // Bind configuration entries from the config file.
            DesiredResolutionX = Config.Bind("General", "ResolutionWidth", (float)Display.main.systemWidth, "Set desired resolution width.");
            DesiredResolutionY = Config.Bind("General", "ResolutionHeight", (float)Display.main.systemHeight, "Set desired resolution height.");
            Fullscreen = Config.Bind("General", "Fullscreen", true, "Set to true for fullscreen or false for windowed.");
            UIFixes = Config.Bind("Tweaks", "UIFixes", true, "Fixes user interface issues at wider than 16:9 aspect ratios.");
            FixSoftMaskMaterials = Config.Bind("Tweaks", "FixSoftMaskMaterials", true, "Fixes SoftMask material compatibility issues with TextMeshPro components.");
            UnlockedFPS = Config.Bind("General", "UnlockedFPS", 120, "Set the desired framerate limit.");
            ToggleVSync = Config.Bind("General", "EnableVSync", true, "Enable VSync");
            EnableHexPatching = Config.Bind("Advanced", "EnableHexPatching", false, "Enable experimental hex patching as last resort for framerate unlock. USE WITH CAUTION!");

            // Detect game version and assembly hash
            DetectGameVersion();

            // Try multiple approaches in order of preference
            bool patchingSuccessful = false;

            // Try Harmony patches first, fall back to alternative methods if they fail
            if (TryHarmonyPatches())
            {
                patchingSuccessful = true;
                Logger.LogInfo("Harmony patching successful");
            }
            else
            {
                Logger.LogWarning("Harmony patches failed, attempting alternative framerate unlock method...");
                TryAlternativeFramerateUnlock();
                
                // If enabled, try hex patching as last resort
                if (EnableHexPatching.Value && !patchingSuccessful)
                {
                    Logger.LogWarning("Attempting experimental hex patching...");
                    TryHexPatching();
                }
            }
            
            // Start coroutine to periodically fix SoftMask material issues
            if (FixSoftMaskMaterials.Value)
            {
                StartCoroutine(PeriodicMaterialFix());
            }
        }

        /// <summary>
        /// Detects the game version and assembly characteristics
        /// </summary>
        private void DetectGameVersion()
        {
            try
            {
                string assemblyPath = "Biped_Data/Managed/Assembly-CSharp.dll";
                if (File.Exists(assemblyPath))
                {
                    // Calculate assembly hash for version detection
                    _assemblyHash = CalculateFileHash(assemblyPath);
                    
                    // Try to get game version from assembly
                    var gameAssembly = Assembly.LoadFrom(assemblyPath);
                    _detectedGameVersion = gameAssembly.GetName().Version?.ToString() ?? "Unknown";
                    
                    // Log version information
                    Logger.LogInfo($"Detected game assembly hash: {_assemblyHash.Substring(0, 8)}...");
                    Logger.LogInfo($"Assembly version: {_detectedGameVersion}");
                    
                    // Check for known problematic versions
                    CheckKnownVersionIssues();
                }
                else
                {
                    Logger.LogWarning("Game assembly not found - plugin may not work correctly");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to detect game version: {ex.Message}");
            }
        }

        /// <summary>
        /// Check for known version-specific issues
        /// </summary>
        private void CheckKnownVersionIssues()
        {
            // Known problematic assembly hashes (first 8 characters for identification)
            var knownIssues = new Dictionary<string, string>
            {
                // Add known problematic versions here as they're discovered
                // Example: {"ABC12345", "This version has known framerate unlock issues"}
            };

            string shortHash = _assemblyHash.Substring(0, 8);
            if (knownIssues.ContainsKey(shortHash))
            {
                Logger.LogWarning($"Known issue detected: {knownIssues[shortHash]}");
                Logger.LogWarning("Consider using alternative unlock methods if Harmony patches fail");
            }
        }

        /// <summary>
        /// Calculate SHA256 hash of a file
        /// </summary>
        private string CalculateFileHash(string filePath)
        {
            using (var sha256 = SHA256.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hash = sha256.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }

        /// <summary>
        /// Attempts to apply Harmony patches with error handling
        /// </summary>
        /// <returns>True if patches were successful, false otherwise</returns>
        private bool TryHarmonyPatches()
        {
            try
            {
                // Verify critical classes exist before patching
                if (!VerifyGameClasses())
                {
                    Logger.LogError("Required game classes not found - game may have been updated");
                    return false;
                }

                Harmony.CreateAndPatchAll(typeof(Patches));
                Logger.LogInfo("Harmony patches applied successfully");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to apply Harmony patches: {ex.Message}");
                Logger.LogError($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Verifies that required game classes exist with enhanced checking
        /// </summary>
        private bool VerifyGameClasses()
        {
            try
            {
                var gameAssembly = Assembly.LoadFrom("Biped_Data/Managed/Assembly-CSharp.dll");
                var requiredTypes = new Dictionary<string, string[]>
                {
                    { "Biped.Game", new[] { "Awake" } },
                    { "Biped.Level", new[] { "Awake", "OnDestroy" } },
                    { "Biped.ScreenResolutionUtils", new[] { "ApplyResolution" } },
                    { "Biped.HintBubbleDialogHandler", new[] { "Init" } },
                    { "Biped.BipedGameUI", new[] { "Start", "Awake" } },
                    { "Biped.SavingNode", new[] { "Start" } },
                    { "Biped.TitleVideo", new[] { "GetRandomVideoUrl" } },
                    { "Biped.DefaultVideoPlayer", new[] { "DoPlay" } }
                };

                foreach (var kvp in requiredTypes)
                {
                    var type = gameAssembly.GetType(kvp.Key);
                    if (type == null)
                    {
                        Logger.LogWarning($"Required type {kvp.Key} not found");
                        return false;
                    }

                    // Verify required methods exist
                    foreach (var methodName in kvp.Value)
                    {
                        var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                        if (method == null)
                        {
                            Logger.LogWarning($"Required method {kvp.Key}.{methodName} not found");
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error verifying game classes: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Alternative framerate unlock method using direct Unity APIs
        /// </summary>
        private void TryAlternativeFramerateUnlock()
        {
            try
            {
                // Force framerate settings using Unity's built-in APIs
                Application.targetFrameRate = UnlockedFPS.Value;
                QualitySettings.vSyncCount = ToggleVSync.Value ? 1 : 0;
                
                Logger.LogInfo($"Alternative framerate unlock applied: FPS={Application.targetFrameRate}, VSync={QualitySettings.vSyncCount}");
                
                // Start a coroutine to periodically reapply these settings
                StartCoroutine(PersistentFramerateEnforcement());
            }
            catch (Exception ex)
            {
                Logger.LogError($"Alternative framerate unlock failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Experimental hex patching method as last resort
        /// WARNING: This is potentially dangerous and should be used with caution
        /// </summary>
        private void TryHexPatching()
        {
            try
            {
                Logger.LogWarning("EXPERIMENTAL: Attempting hex patching - this may cause instability!");
                
                // Known hex patterns for different game versions
                var knownPatterns = new Dictionary<string, (byte[] search, byte[] replace, string description)>
                {
                    // Pattern from GitHub comment: Search for 16281218000A1F3C, change 16 to 17
                    {"biped_vsync_fps_unlock", (new byte[] {0x16, 0x28, 0x12, 0x18, 0x00, 0x0A, 0x1F, 0x3C}, 
                                                new byte[] {0x17, 0x28, 0x12, 0x18, 0x00, 0x0A, 0x1F, 0x3C}, 
                                                "Enable VSync to ignore 60 FPS limit")}
                };

                bool patched = false;
                foreach (var pattern in knownPatterns)
                {
                    if (TryApplyHexPatch(pattern.Value.search, pattern.Value.replace, pattern.Value.description))
                    {
                        patched = true;
                        break;
                    }
                }

                if (!patched)
                {
                    Logger.LogWarning("No suitable hex patterns found for this game version");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Hex patching failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply a hex patch to the game assembly
        /// WARNING: This is experimental and potentially dangerous
        /// </summary>
        private bool TryApplyHexPatch(byte[] searchPattern, byte[] replacement, string description)
        {
            try
            {
                string assemblyPath = "Biped_Data/Managed/Assembly-CSharp.dll";
                
                if (!File.Exists(assemblyPath))
                {
                    Logger.LogError($"Assembly file not found: {assemblyPath}");
                    return false;
                }

                Logger.LogWarning($"Attempting to apply hex patch: {description}");
                
                // Create backup before modifying
                string backupPath = assemblyPath + ".backup";
                if (!File.Exists(backupPath))
                {
                    File.Copy(assemblyPath, backupPath);
                    Logger.LogInfo($"Created backup: {backupPath}");
                }

                // Read the assembly file
                byte[] assemblyBytes = File.ReadAllBytes(assemblyPath);
                
                // Search for the pattern
                int patternIndex = FindBytePattern(assemblyBytes, searchPattern);
                
                if (patternIndex == -1)
                {
                    Logger.LogWarning($"Pattern not found in assembly: {BitConverter.ToString(searchPattern)}");
                    return false;
                }

                Logger.LogInfo($"Found pattern at offset: 0x{patternIndex:X8}");
                
                // Apply the replacement
                for (int i = 0; i < replacement.Length; i++)
                {
                    assemblyBytes[patternIndex + i] = replacement[i];
                }

                // Write the modified assembly back
                File.WriteAllBytes(assemblyPath, assemblyBytes);
                
                Logger.LogInfo($"Successfully applied hex patch: {description}");
                Logger.LogInfo($"Modified byte at offset 0x{patternIndex:X8}: 0x{searchPattern[0]:X2} -> 0x{replacement[0]:X2}");
                
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to apply hex patch: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Find a byte pattern in a byte array
        /// </summary>
        /// <param name="haystack">The byte array to search in</param>
        /// <param name="needle">The byte pattern to search for</param>
        /// <returns>The index of the first occurrence, or -1 if not found</returns>
        private int FindBytePattern(byte[] haystack, byte[] needle)
        {
            for (int i = 0; i <= haystack.Length - needle.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < needle.Length; j++)
                {
                    if (haystack[i + j] != needle[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Continuously enforces framerate settings to override game attempts to change them
        /// </summary>
        private IEnumerator PersistentFramerateEnforcement()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f); // Check every second
                
                if (Application.targetFrameRate != UnlockedFPS.Value)
                {
                    Application.targetFrameRate = UnlockedFPS.Value;
                    Logger.LogDebug($"Re-applied target framerate: {UnlockedFPS.Value}");
                }
                
                int expectedVSync = ToggleVSync.Value ? 1 : 0;
                if (QualitySettings.vSyncCount != expectedVSync)
                {
                    QualitySettings.vSyncCount = expectedVSync;
                    Logger.LogDebug($"Re-applied VSync setting: {expectedVSync}");
                }
            }
        }

        /// <summary>
        /// Coroutine that periodically checks and fixes SoftMask material issues.
        /// This helps handle dynamically created UI elements and compensates for the removed Canvas.OnEnable patch.
        /// </summary>
        private IEnumerator PeriodicMaterialFix()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f); // Check every 1 second (increased frequency)
                
                if (FixSoftMaskMaterials.Value)
                {
                    Patches.FixTextMeshProMaterials();
                }
            }
        }

        /// <summary>
        /// Contains Harmony patches to apply resolution and UI fixes.
        /// </summary>
        [HarmonyPatch]
        public class Patches
        {
            // User properties to ensure values are recalculated dynamically if configuration changes
            private static int VSyncFrames => Plugin.ToggleVSync.Value ? 1 : 0;
            private static float DefaultAspectRatio => 1920f / 1080f; // 1920x1080 as default resolution
            private static float NewAspectRatio => Plugin.DesiredResolutionX.Value / Plugin.DesiredResolutionY.Value;
            private static float AspectMultiplier => NewAspectRatio / DefaultAspectRatio;
            private static Vector2 DefaultReferenceResolution => new(1920, 1080);
            private static Vector2 NewReferenceResolution => new(AspectMultiplier * 1920, 1080);

            // Track patch application status for diagnostics
            private static Dictionary<string, bool> _patchStatus = new();

            /// <summary>
            /// Helper method to set the application's framerate and VSync count.
            /// </summary>
            private static void SetFramerate()
            {
                try
                {
                    Application.targetFrameRate = Plugin.UnlockedFPS.Value;
                    QualitySettings.vSyncCount = VSyncFrames;
                    Plugin.Logger.LogInfo($"Set target frame rate to {Application.targetFrameRate} and VSyncCount to {QualitySettings.vSyncCount}");
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError($"Failed to set framerate: {ex.Message}");
                }
            }

            /// <summary>
            /// Generic method to safely execute patch logic with error handling
            /// </summary>
            private static void SafeExecutePatch(string patchName, Action patchAction)
            {
                try
                {
                    patchAction?.Invoke();
                    _patchStatus[patchName] = true;
                }
                catch (Exception ex)
                {
                    Plugin.Logger.LogError($"Patch '{patchName}' failed: {ex.Message}");
                    _patchStatus[patchName] = false;
                }
            }

            /// <summary>
            /// Applies the desired screen resolution and fullscreen setting.
            /// </summary>
            [HarmonyPatch(typeof(Biped.ScreenResolutionUtils), nameof(Biped.ScreenResolutionUtils.ApplyResolution))]
            [HarmonyPostfix]
            public static void SetResolution()
            {
                SafeExecutePatch("SetResolution", () =>
                {
                    // Apply new screen resolution settings.
                    Screen.SetResolution((int)Plugin.DesiredResolutionX.Value, (int)Plugin.DesiredResolutionY.Value, Plugin.Fullscreen.Value);
                    Plugin.Logger.LogInfo($"Screen resolution set to {(int)Plugin.DesiredResolutionX.Value}x{(int)Plugin.DesiredResolutionY.Value}");
                });
            }

            /// <summary>
            /// Unlocks the framerate during the initialization of Biped.Game.
            /// </summary>
            [HarmonyPatch(typeof(Biped.Game), "Awake")]
            [HarmonyPostfix]
            public static void UnlockFramerate_GameAwake()
            {
                SafeExecutePatch("UnlockFramerate_GameAwake", () => SetFramerate());
            }

            /// <summary>
            /// Unlocks the framerate during the initialization of Biped.Level.
            /// </summary>
            [HarmonyPatch(typeof(Biped.Level), "Awake")]
            [HarmonyPostfix]
            public static void UnlockFramerate_LevelAwake()
            {
                SafeExecutePatch("UnlockFramerate_LevelAwake", () => SetFramerate());
            }

            /// <summary>
            /// Unlocks the framerate during the destruction phase of Biped.Level.
            /// </summary>
            [HarmonyPatch(typeof(Biped.Level), "OnDestroy")]
            [HarmonyPostfix]
            public static void UnlockFramerate_LevelOnDestroy()
            {
                SafeExecutePatch("UnlockFramerate_LevelOnDestroy", () => SetFramerate());
            }

            /// <summary>
            /// Updates the reference resolution for the hint bubble dialog to correct misalignment on ultra wide screens.
            /// </summary>
            /// <param name="__instance">Instance of the HintBubbleDialogHandler.</param>
            [HarmonyPatch(typeof(Biped.HintBubbleDialogHandler), nameof(Biped.HintBubbleDialogHandler.Init))]
            [HarmonyPostfix]
            public static void UpdateHintReferenceResolution(Biped.HintBubbleDialogHandler __instance)
            {
                SafeExecutePatch("UpdateHintReferenceResolution", () =>
                {
                    if (Plugin.UIFixes.Value)
                    {
                        // Find the dialog canvas and update its CanvasScaler reference resolution.
                        GameObject canvasObj = GameObject.Find("DialogCanvas(Clone)");
                        if (canvasObj != null)
                        {
                            CanvasScaler canvasScaler = canvasObj.GetComponent<CanvasScaler>();
                            if (canvasScaler != null)
                            {
                                canvasScaler.referenceResolution = NewReferenceResolution;
                                Plugin.Logger.LogInfo($"Hint bubble canvas reference resolution updated to {canvasScaler.referenceResolution}");
                            }
                            else
                            {
                                Plugin.Logger.LogWarning("CanvasScaler component not found on 'DialogCanvas(Clone)'");
                            }
                        }
                        else
                        {
                            Plugin.Logger.LogWarning("'DialogCanvas(Clone)' GameObject not found");
                        }
                    }
                    
                    // Fix SoftMask material compatibility issues after dialog initialization
                    if (Plugin.FixSoftMaskMaterials.Value)
                    {
                        FixTextMeshProMaterials();
                    }
                });
            }

            /// <summary>
            /// Updates the main game UI's reference resolution to properly scale on ultra wide monitors.
            /// </summary>
            [HarmonyPatch(typeof(Biped.BipedGameUI), nameof(Biped.BipedGameUI.Start))]
            [HarmonyPostfix]
            public static void UpdateGameMainUIReferenceResolution()
            {
                SafeExecutePatch("UpdateGameMainUIReferenceResolution", () =>
                {
                    if (Plugin.UIFixes.Value)
                    {
                        var allCanvasScalerObjects = Resources.FindObjectsOfTypeAll<CanvasScaler>();
                        // now find a canvas scaler with the name "GameMainUI"
                        foreach (var go in allCanvasScalerObjects)
                        {
                            if (go.name == "GameMainUI")
                            {
                                Plugin.Logger.LogInfo($"Found GameMainUI");
                                go.referenceResolution = NewReferenceResolution;
                                Plugin.Logger.LogInfo($"Game UI reference resolution updated to {go.referenceResolution}");
                            }
                        }
                    }
                    
                    // Fix SoftMask material compatibility issues after UI start
                    if (Plugin.FixSoftMaskMaterials.Value)
                    {
                        FixTextMeshProMaterials();
                    }
                });
            }

            /// <summary>
            /// Adjusts the position of the saving UI element to better fit ultra wide screens.
            /// </summary>
            [HarmonyPatch(typeof(Biped.SavingNode), "Start")]
            [HarmonyPostfix]
            public static void MoveSavingUI()
            {
                SafeExecutePatch("MoveSavingUI", () =>
                {
                    if (Plugin.UIFixes.Value)
                    {
                        // Locate the saving UI prefab and adjust its local position based on the aspect multiplier.
                        GameObject savingObj = GameObject.Find("GameMainUI/GamingUICoopMode/Saving/Prefab_Saving");
                        if (savingObj != null)
                        {
                            RectTransform savingRect = savingObj.GetComponent<RectTransform>();
                            if (savingRect != null)
                            {
                                // Adjust position; the X value is modified by the aspect multiplier.
                                savingRect.localPosition = new Vector3(749f / AspectMultiplier, -104, 0);
                                Plugin.Logger.LogInfo($"Saving UI local position updated to {savingRect.localPosition}");
                            }
                            else
                            {
                                Plugin.Logger.LogWarning("RectTransform component not found on 'Prefab_Saving'");
                            }
                        }
                        else
                        {
                            Plugin.Logger.LogWarning("'Prefab_Saving' GameObject not found");
                        }
                    }
                });
            }

            [HarmonyPatch(typeof(Biped.BipedGameUI), "Awake")]
            [HarmonyPostfix]
            public static void AdjustGameUI()
            {
                SafeExecutePatch("AdjustGameUI", () =>
                {
                    if (Plugin.UIFixes.Value)
                    {
                        // Grab all of the RectTransform objects in the scene.
                        var allRectTransformObjects = Resources.FindObjectsOfTypeAll<RectTransform>();

                        // Define the target names. 
                        string cinematicUIName = "CinematicUI";
                        string uIMaskName = "UI_Mask";

                        foreach (var go in allRectTransformObjects)
                        {
                            if (go.name == cinematicUIName)
                            {
                                Plugin.Logger.LogInfo($"Found RectTransform target: {go.name} at path: {GetRectTransformPath(go)}");
                                AdjustLetterboxing(go);
                            }

                            if (go.name == uIMaskName)
                            {
                                Plugin.Logger.LogInfo($"Found RectTransform target: {go.name} at path: {GetRectTransformPath(go)}");
                                AdjustUIMask(go);
                            }
                        }
                    }

                    // Fix SoftMask material compatibility issues with TextMeshPro
                    if (Plugin.FixSoftMaskMaterials.Value)
                    {
                        FixTextMeshProMaterials();
                    }
                });
            }

            /// <summary>
            /// Helper method to retrieve the full hierarchy path of a GameObject.
            /// </summary>
            /// <param name="obj">The GameObject whose path is desired.</param>
            /// <returns>A string representing the full path in the scene hierarchy.</returns>
            private static string GetRectTransformPath(RectTransform obj)
            {
                string path = obj.name;
                Transform current = obj.transform.parent;
                while (current != null)
                {
                    path = current.name + "/" + path;
                    current = current.parent;
                }
                return path;
            }

            /// <summary>
            /// Fixes SoftMask material compatibility issues with TextMeshPro components.
            /// This addresses the warning: "SoftMask will not work on Text (TMPro.TextMeshProUGUI) because material doesn't support masking."
            /// Enhanced version with better performance and logging.
            /// </summary>
            public static void FixTextMeshProMaterials()
            {
                try
                {
                    // Find all TextMeshPro components using reflection since we don't have direct access to TMPro namespace
                    var allComponents = Resources.FindObjectsOfTypeAll<Component>();
                    
                    int fixedCount = 0;
                    int checkedCount = 0;
                    
                    foreach (var component in allComponents)
                    {
                        if (component != null && component.GetType().Name.Contains("TextMeshPro"))
                        {
                            checkedCount++;
                            
                            // Use reflection to access the material property
                            var materialProperty = component.GetType().GetProperty("material", BindingFlags.Public | BindingFlags.Instance);
                            if (materialProperty != null)
                            {
                                var material = materialProperty.GetValue(component) as Material;
                                if (material != null)
                                {
                                    // Check if the material name contains "Axion RND SDF" or other problematic material names
                                    if (material.name.Contains("Axion RND SDF") || 
                                        (material.name.Contains("SDF") && !material.name.Contains("UI")))
                                    {
                                        // Check if the component has a SoftMask in its hierarchy
                                        if (HasSoftMaskInHierarchy(component.transform))
                                        {
                                            Plugin.Logger.LogInfo($"Fixing SoftMask material compatibility for {component.GetType().Name}: {component.name} with material: {material.name}");
                                            
                                            // Set material to null to force Unity to use the default UI material which supports masking
                                            materialProperty.SetValue(component, null);
                                            fixedCount++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                    if (fixedCount > 0)
                    {
                        Plugin.Logger.LogInfo($"Fixed {fixedCount} TextMeshPro components with SoftMask material compatibility issues (checked {checkedCount} components)");
                    }
                    else if (checkedCount > 0)
                    {
                        Plugin.Logger.LogDebug($"No SoftMask material issues found (checked {checkedCount} TextMeshPro components)");
                    }
                }
                catch (System.Exception ex)
                {
                    Plugin.Logger.LogError($"Error fixing TextMeshPro materials: {ex.Message}");
                }
            }

            /// <summary>
            /// Checks if a Transform has a SoftMask component in its hierarchy (parent chain).
            /// </summary>
            /// <param name="transform">The transform to check</param>
            /// <returns>True if SoftMask is found in the hierarchy</returns>
            private static bool HasSoftMaskInHierarchy(Transform transform)
            {
                Transform current = transform;
                while (current != null)
                {
                    // Check for SoftMask component (using reflection since we don't have direct access to the SoftMask type)
                    var components = current.GetComponents<Component>();
                    foreach (var component in components)
                    {
                        if (component != null && component.GetType().Name.Contains("SoftMask"))
                        {
                            return true;
                        }
                    }
                    current = current.parent;
                }
                return false;
            }

            /// <summary>
            /// Adjusts the cinematic letterboxing scale to fit ultra wide displays.
            /// </summary>
            public static void AdjustLetterboxing(RectTransform cinematicObj)
            {
                if (cinematicObj != null)
                {
                    RectTransform cinematicRect = cinematicObj.GetComponent<RectTransform>();
                    if (cinematicRect != null)
                    {
                        Plugin.Logger.LogInfo("AdjustLetterboxing - CinematicUI Object Found.");
                        cinematicRect.localScale = new Vector3(1 * AspectMultiplier, 1, 1);
                        Plugin.Logger.LogInfo($"Cinematic UI local scale updated to {cinematicRect.localScale}");
                    }
                    else
                    {
                        Plugin.Logger.LogWarning("AdjustLetterboxing - RectTransform component not found on 'CinematicUI'");
                    }
                }
                else
                {
                    Plugin.Logger.LogWarning("AdjustLetterboxing - 'CinematicUI' GameObject not found");
                }
            }

            /// <summary>
            /// Adjusts the UI mask scale for ultra wide monitors.
            /// </summary>
            public static void AdjustUIMask(RectTransform maskObj)
            {
                if (maskObj != null)
                {
                    RectTransform maskRect = maskObj.GetComponent<RectTransform>();
                    if (maskRect != null)
                    {
                        maskRect.localScale = new Vector3(1 * AspectMultiplier, 1, 1);
                        Plugin.Logger.LogInfo($"UI mask local scale updated to {maskRect.localScale}");
                    }
                    else
                    {
                        Plugin.Logger.LogWarning("AdjustUIMask - RectTransform component not found on 'UI_Mask'");
                    }
                }
                else
                {
                    Plugin.Logger.LogWarning("AdjustUIMask - 'UI_Mask' GameObject not found");
                }
            }

            /// <summary>
            /// Adjusts the title video's aspect ratio for ultra wide displays.
            /// </summary>
            [HarmonyPatch(typeof(Biped.TitleVideo), "GetRandomVideoUrl")]
            [HarmonyPostfix]
            public static void TitleVideoAR()
            {
                if (Plugin.UIFixes.Value)
                {
                    // Locate the title video player and adjust its scale to correct the aspect ratio.
                    GameObject titleVideoObj = GameObject.Find("TitleVideo/Player");
                    if (titleVideoObj != null)
                    {
                        RectTransform titleVideoRect = titleVideoObj.GetComponent<RectTransform>();
                        if (titleVideoRect != null)
                        {
                            titleVideoRect.localScale = new Vector3(1f / AspectMultiplier, 1, 1);
                            Plugin.Logger.LogInfo($"Title video local scale updated to {titleVideoRect.localScale}");
                        }
                        else
                        {
                            Plugin.Logger.LogWarning("RectTransform component not found on 'TitleVideo/Player'");
                        }
                    }
                    else
                    {
                        Plugin.Logger.LogWarning("'TitleVideo/Player' GameObject not found");
                    }
                }
            }

            /// <summary>
            /// Adjusts the game menu video's aspect ratio for ultra wide displays.
            /// </summary>
            [HarmonyPatch(typeof(Biped.DefaultVideoPlayer), "DoPlay")]
            [HarmonyPostfix]
            public static void GameMenuVideoAR()
            {
                if (Plugin.UIFixes.Value)
                {
                    // Locate the game menu video player and adjust its scale to correct the aspect ratio.
                    GameObject menuVideoObj = GameObject.Find("GameMainUI/VideoUI/Player");
                    if (menuVideoObj != null)
                    {
                        RectTransform menuVideoRect = menuVideoObj.GetComponent<RectTransform>();
                        if (menuVideoRect != null)
                        {
                            menuVideoRect.localScale = new Vector3(1f / AspectMultiplier, 1, 1);
                            Plugin.Logger.LogInfo($"Game menu video local scale updated to {menuVideoRect.localScale}");
                        }
                        else
                        {
                            Plugin.Logger.LogWarning("RectTransform component not found on 'GameMainUI/VideoUI/Player'");
                        }
                    }
                    else
                    {
                        Plugin.Logger.LogWarning("'GameMainUI/VideoUI/Player' GameObject not found");
                    }
                }
            }
        }
    }
}

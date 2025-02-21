using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.NET.Common;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace BipedWideScreenFixPlugin
{
    /// <summary>
    /// BepInEx plugin that applies fixes to the game "Biped" to support ultra wide monitors.
    /// It adjusts screen resolution, framerate settings, and UI layouts to ensure proper display.
    /// </summary>
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        /// <summary>
        /// Logger instance for plugin logging.
        /// </summary>
        internal static new ManualLogSource Log { get; private set; }

        // Configuration entries for the plugin
        public static ConfigEntry<bool> UIFixes { get; private set; }
        public static ConfigEntry<int> UnlockedFPS { get; private set; }
        public static ConfigEntry<bool> ToggleVSync { get; private set; }
        public static ConfigEntry<bool> Fullscreen { get; private set; }
        public static ConfigEntry<float> DesiredResolutionX { get; private set; }
        public static ConfigEntry<float> DesiredResolutionY { get; private set; }

        /// <summary>
        /// Called when the plugin is loaded. Initializes the logger.
        /// </summary>
        public override void Load()
        {
            Log = base.Log;
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        /// <summary>
        /// Contains Harmony patches to apply resolution and UI fixes.
        /// </summary>
        [HarmonyPatch]
        public class Patches
        {
            // Use properties to ensure values are recalculated dynamically if configuration changes
            private static int VSyncFrames => Plugin.ToggleVSync.Value ? 1 : 0;
            private static float DefaultAspectRatio => 1920f / 1080f; // 1920x1080 as default resolution
            private static float NewAspectRatio => Plugin.DesiredResolutionX.Value / Plugin.DesiredResolutionY.Value;
            private static float AspectMultiplier => NewAspectRatio / DefaultAspectRatio;
            private static Vector2 DefaultReferenceResolution => new(1920, 1080);
            private static Vector2 NewReferenceResolution => new(AspectMultiplier * 1920, 1080);

            /// <summary>
            /// Helper method to set the application's framerate and VSync count.
            /// </summary>
            private static void SetFramerate()
            {
                Application.targetFrameRate = Plugin.UnlockedFPS.Value;
                QualitySettings.vSyncCount = VSyncFrames;
                Plugin.Log.LogInfo($"Set target frame rate to {Application.targetFrameRate} and VSyncCount to {QualitySettings.vSyncCount}");
            }

            /// <summary>
            /// Applies the desired screen resolution and fullscreen setting.
            /// </summary>
            [HarmonyPatch(typeof(Biped.ScreenResolutionUtils), nameof(Biped.ScreenResolutionUtils.ApplyResolution))]
            [HarmonyPostfix]
            public static void SetResolution()
            {
                // Apply new screen resolution settings.
                Screen.SetResolution((int)Plugin.DesiredResolutionX.Value, (int)Plugin.DesiredResolutionY.Value, Plugin.Fullscreen.Value);
                Plugin.Log.LogInfo($"Screen resolution set to {(int)Plugin.DesiredResolutionX.Value}x{(int)Plugin.DesiredResolutionY.Value}");
            }

            /// <summary>
            /// Unlocks the framerate during the initialization of Biped.Game.
            /// </summary>
            [HarmonyPatch(typeof(Biped.Game), "Awake")]
            [HarmonyPostfix]
            public static void UnlockFramerate_GameAwake()
            {
                SetFramerate();
            }

            /// <summary>
            /// Unlocks the framerate during the initialization of Biped.Level.
            /// </summary>
            [HarmonyPatch(typeof(Biped.Level), "Awake")]
            [HarmonyPostfix]
            public static void UnlockFramerate_LevelAwake()
            {
                SetFramerate();
            }

            /// <summary>
            /// Unlocks the framerate during the destruction phase of Biped.Level.
            /// </summary>
            [HarmonyPatch(typeof(Biped.Level), "OnDestroy")]
            [HarmonyPostfix]
            public static void UnlockFramerate_LevelOnDestroy()
            {
                SetFramerate();
            }

            /// <summary>
            /// Updates the reference resolution for the hint bubble dialog to correct misalignment on ultra wide screens.
            /// </summary>
            /// <param name="__instance">Instance of the HintBubbleDialogHandler.</param>
            [HarmonyPatch(typeof(Biped.HintBubbleDialogHandler), nameof(Biped.HintBubbleDialogHandler.Init))]
            [HarmonyPostfix]
            public static void UpdateHintReferenceResolution(Biped.HintBubbleDialogHandler __instance)
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
                            Plugin.Log.LogInfo($"Hint bubble canvas reference resolution updated to {canvasScaler.referenceResolution}");
                        }
                        else
                        {
                            Plugin.Log.LogWarning("CanvasScaler component not found on 'DialogCanvas(Clone)'");
                        }
                    }
                    else
                    {
                        Plugin.Log.LogWarning("'DialogCanvas(Clone)' GameObject not found");
                    }
                }
            }

            /// <summary>
            /// Updates the main game UI's reference resolution to properly scale on ultra wide monitors.
            /// </summary>
            [HarmonyPatch(typeof(Biped.BipedGameUI), nameof(Biped.BipedGameUI.Start))]
            [HarmonyPostfix]
            public static void UpdateGameMainUIReferenceResolution()
            {
                if (Plugin.UIFixes.Value)
                {
                    // Find the main game UI canvas and update its CanvasScaler reference resolution.
                    GameObject canvasObj = GameObject.Find("GameMainUI");
                    if (canvasObj != null)
                    {
                        CanvasScaler canvasScaler = canvasObj.GetComponent<CanvasScaler>();
                        if (canvasScaler != null)
                        {
                            canvasScaler.referenceResolution = NewReferenceResolution;
                            Plugin.Log.LogInfo($"Game UI reference resolution updated to {canvasScaler.referenceResolution}");
                        }
                        else
                        {
                            Plugin.Log.LogWarning("CanvasScaler component not found on 'GameMainUI'");
                        }
                    }
                    else
                    {
                        Plugin.Log.LogWarning("'GameMainUI' GameObject not found");
                    }
                }
            }

            /// <summary>
            /// Adjusts the position of the saving UI element to better fit ultra wide screens.
            /// </summary>
            [HarmonyPatch(typeof(Biped.SavingNode), "Start")]
            [HarmonyPostfix]
            public static void MoveSavingUI()
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
                            Plugin.Log.LogInfo($"Saving UI local position updated to {savingRect.localPosition}");
                        }
                        else
                        {
                            Plugin.Log.LogWarning("RectTransform component not found on 'Prefab_Saving'");
                        }
                    }
                    else
                    {
                        Plugin.Log.LogWarning("'Prefab_Saving' GameObject not found");
                    }
                }
            }

            /// <summary>
            /// Adjusts the cinematic letterboxing scale to fit ultra wide displays.
            /// </summary>
            [HarmonyPatch(typeof(Biped.GameMainUI), "Awake")]
            [HarmonyPostfix]
            public static void AdjustLetterboxing()
            {
                if (Plugin.UIFixes.Value)
                {
                    // Find the cinematic UI and scale its letterbox based on the aspect multiplier.
                    GameObject cinematicObj = GameObject.Find("GameMainUI/GamingUICoopMode/CinematicUI");
                    if (cinematicObj != null)
                    {
                        RectTransform cinematicRect = cinematicObj.GetComponent<RectTransform>();
                        if (cinematicRect != null)
                        {
                            cinematicRect.localScale = new Vector3(AspectMultiplier, 1, 1);
                            Plugin.Log.LogInfo($"Cinematic UI local scale updated to {cinematicRect.localScale}");
                        }
                        else
                        {
                            Plugin.Log.LogWarning("RectTransform component not found on 'CinematicUI'");
                        }
                    }
                    else
                    {
                        Plugin.Log.LogWarning("'CinematicUI' GameObject not found");
                    }
                }
            }

            /// <summary>
            /// Adjusts the UI mask scale for ultra wide monitors.
            /// </summary>
            [HarmonyPatch(typeof(Biped.GameMainUI), "Awake")]
            [HarmonyPostfix]
            public static void AdjustUIMask()
            {
                if (Plugin.UIFixes.Value)
                {
                    // Find the UI mask and update its scale based on the aspect multiplier.
                    GameObject maskObj = GameObject.Find("GameMainUI/GamingUICoopMode/UI_Mask");
                    if (maskObj != null)
                    {
                        RectTransform maskRect = maskObj.GetComponent<RectTransform>();
                        if (maskRect != null)
                        {
                            maskRect.localScale = new Vector3(AspectMultiplier, 1, 1);
                            Plugin.Log.LogInfo($"UI mask local scale updated to {maskRect.localScale}");
                        }
                        else
                        {
                            Plugin.Log.LogWarning("RectTransform component not found on 'UI_Mask'");
                        }
                    }
                    else
                    {
                        Plugin.Log.LogWarning("'UI_Mask' GameObject not found");
                    }
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
                            Plugin.Log.LogInfo($"Title video local scale updated to {titleVideoRect.localScale}");
                        }
                        else
                        {
                            Plugin.Log.LogWarning("RectTransform component not found on 'TitleVideo/Player'");
                        }
                    }
                    else
                    {
                        Plugin.Log.LogWarning("'TitleVideo/Player' GameObject not found");
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
                            Plugin.Log.LogInfo($"Game menu video local scale updated to {menuVideoRect.localScale}");
                        }
                        else
                        {
                            Plugin.Log.LogWarning("RectTransform component not found on 'GameMainUI/VideoUI/Player'");
                        }
                    }
                    else
                    {
                        Plugin.Log.LogWarning("'GameMainUI/VideoUI/Player' GameObject not found");
                    }
                }
            }
        }
    }
}

using System.Diagnostics;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using sv08;
using UnityEngine;

namespace KKLB_Tweaks
{
    [BepInPlugin(GUID, PluginName, Version)]
    public class TweaksPlugin : BaseUnityPlugin
    {
        public const string PluginName = "Graphical tweaks for KoiKoi Love Blossoms";
        public const string GUID = "KKLB_Tweaks";
        public const string Version = "1.0";

        private static TweaksPlugin _instance;
        private static Harmony _hi;

        private static ConfigEntry<int> _bestAA;

        private void Awake()
        {
            _bestAA = Config.Bind("Graphics", "Best quality AA", 4, new ConfigDescription("Level of Anti-aliasing when 'Performance' is set to '3' in game settings (best graphics, but slowest). Higher AA values require much more powerful GPU to keep the game fluid in VR.", new AcceptableValueList<int>(0, 2, 4, 8)));

            _instance = this;
            _hi = Harmony.CreateAndPatchAll(typeof(TweaksPlugin), GUID);
        }

        private void OnDestroy()
        {
            _hi?.UnpatchSelf();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Command_Base), nameof(Command_Base.SetPerformance))]
        private static void BetterQualitySettings(float value)
        {
            _instance.Logger.LogDebug($"Setting Performance to {value}");

            switch (value)
            {
                default:
                case 1:
                    QualitySettings.pixelLightCount = 4;
                    QualitySettings.shadows = ShadowQuality.All;
                    QualitySettings.shadowProjection = ShadowProjection.StableFit;
                    QualitySettings.shadowCascades = 4;
                    QualitySettings.shadowDistance = 150;
                    QualitySettings.shadowResolution = ShadowResolution.High;
                    QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask; //DistanceShadowmask breaks shadows
                    QualitySettings.lodBias = 2;
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
                    QualitySettings.particleRaycastBudget = 4096;
                    QualitySettings.antiAliasing = _bestAA.Value;
                    QualitySettings.realtimeReflectionProbes = true;
                    QualitySettings.billboardsFaceCameraPosition = true;
                    QualitySettings.skinWeights = SkinWeights.Unlimited;
                    break;

                case 2:
                    QualitySettings.pixelLightCount = 1;
                    QualitySettings.shadows = ShadowQuality.HardOnly;
                    QualitySettings.shadowProjection = ShadowProjection.CloseFit;
                    QualitySettings.shadowCascades = 2;
                    QualitySettings.shadowDistance = 40;
                    QualitySettings.shadowResolution = ShadowResolution.Medium;
                    QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask;
                    QualitySettings.lodBias = 1;
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
                    QualitySettings.particleRaycastBudget = 512;
                    QualitySettings.antiAliasing = Mathf.Min(2, _bestAA.Value);
                    QualitySettings.realtimeReflectionProbes = false;
                    QualitySettings.billboardsFaceCameraPosition = true;
                    QualitySettings.skinWeights = SkinWeights.TwoBones;
                    break;

                case 3:
                    QualitySettings.pixelLightCount = 0;
                    QualitySettings.shadows = ShadowQuality.Disable;
                    QualitySettings.shadowProjection = ShadowProjection.CloseFit;
                    QualitySettings.shadowCascades = 0;
                    QualitySettings.shadowDistance = 20;
                    QualitySettings.shadowResolution = ShadowResolution.Low;
                    QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask;
                    QualitySettings.lodBias = 0.4f;
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                    QualitySettings.particleRaycastBudget = 16;
                    QualitySettings.antiAliasing = 0;
                    QualitySettings.realtimeReflectionProbes = false;
                    QualitySettings.billboardsFaceCameraPosition = false;
                    QualitySettings.skinWeights = SkinWeights.OneBone;
                    break;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FConfigData), nameof(FConfigData.Init))]
        private static void MaxQualityByDefault(ref FConfigData __instance)
        {
            // Override default to be best quality, user will likely lower it later if necessary.
            // Prevents people playing without anti-aliasing and texture filtering even if their hardware can handle it.
            __instance.Performance = 1;
        }
    }
}

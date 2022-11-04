using BepInEx;
using System.ComponentModel;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Utilla;
using HarmonyLib;

namespace NoFriction
{

    [Description("HauntedModMenu")]
    [ModdedGamemode]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")] // Make sure to add Utilla 1.5.0 as a dependency!
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }

        public bool inAllowedRoom = false;

        bool justPressedB = false;
        public bool sliding = false;

        public List<GorillaSurfaceOverride> gorillaSurfaceOverrides = new List<GorillaSurfaceOverride>();
        public List<int> overrideIndexs = new List<int>();

        void Start()
        {
            Instance = this;
            foreach (GorillaSurfaceOverride surfaceOverride in GameObject.Find("Level").GetComponentsInChildren<GorillaSurfaceOverride>())
            {
                gorillaSurfaceOverrides.Add(surfaceOverride);
                overrideIndexs.Add(surfaceOverride.overrideIndex);
            }
        }

        void Update()
        {
            if (inAllowedRoom && enabled)
            {
                bool eitherButtonDown;

                InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primaryButton, out bool isPressingPrimaryButton);
                InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.secondaryButton, out bool isPressingSecondaryButton);

                eitherButtonDown = isPressingPrimaryButton || isPressingSecondaryButton;

                if (eitherButtonDown)
                {
                    if (!justPressedB)
                    {
                        SetSlide(true);
                        justPressedB = true;
                    }
                }
                else
                {
                    if (justPressedB)
                    {
                        SetSlide(false);
                        justPressedB = false;
                    }
                }
            }
        }

        [ModdedGamemodeJoin] internal void RoomJoined(string gamemode) => inAllowedRoom = true;
        [ModdedGamemodeLeave]
        internal void RoomLeft(string gamemode)
        {
            inAllowedRoom = false;

            // Prevents the player from sliding when you leave a Modded lobby.
            SetSlide(false);
        }

        internal void SetSlide(bool value)
        {
            for (int i = 0; i < gorillaSurfaceOverrides.Count; i++)
            {
                gorillaSurfaceOverrides[i].overrideIndex = value == true ? 61 : overrideIndexs[i];
            }
            sliding = value;
        }

        void OnEnable()
        {
            HarmonyPatches.ApplyHarmonyPatches();
        }

        void OnDisable()
        {
            // Prevents the player from sliding when the mod is disabled.
            HarmonyPatches.RemoveHarmonyPatches();

            if (GorillaLocomotion.Player.Instance != null && sliding)
            {
                GorillaLocomotion.Player.Instance.currentMaterialIndex = 0;
                GorillaLocomotion.Player.Instance.currentOverride.overrideIndex = 0;
                GorillaLocomotion.Player.Instance.leftHandSurfaceOverride.overrideIndex = 0;
                GorillaLocomotion.Player.Instance.rightHandSurfaceOverride.overrideIndex = 0;
            }

            SetSlide(false);    
        }

        [HarmonyPatch(typeof(GorillaLocomotion.Player))]
        [HarmonyPatch("GetSlidePercentage", MethodType.Normal)]
        class slidepatch
        {
            static void Postfix(ref float __result)
            {
                if (Instance.inAllowedRoom && Instance.sliding)
                {
                    __result = 1f;
                }
            }
        }
    }
}

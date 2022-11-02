using BepInEx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.XR;
using Utilla;

namespace NoFriction
{
    [Description("HauntedModMenu")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")] // Make sure to add Utilla 1.5.0 as a dependency!
    [ModdedGamemode]

    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        bool inAllowedRoom = false;
        bool isPressingB = false;
        bool justPressedB = false;
        public RaycastHit[] hits;
        public List<GorillaSurfaceOverride> gorillaSurfaceOverrides = new List<GorillaSurfaceOverride>();
        bool fakestart = true;
        bool hauntedenabled = true;
        public List<int> overrideIndexs = new List<int>();


        void Awake()
        {
            HarmonyPatches.ApplyHarmonyPatches();
            Utilla.Events.GameInitialized += GameInitialized;
        }

        void Update()
        {
            if (inAllowedRoom && hauntedenabled)
            {

                if (fakestart)
                {
                    Collider[] gameObjects = GameObject.Find("Level").GetComponentsInChildren<Collider>();
                    for (int i = 0; i < gameObjects.Length; i++)
                    {
                        if (gameObjects[i].transform.gameObject.GetComponent<GorillaSurfaceOverride>())
                        {
                            overrideIndexs.Add(gameObjects[i].transform.gameObject.GetComponent<GorillaSurfaceOverride>().overrideIndex);
                            Destroy(gameObjects[i].transform.gameObject.GetComponent<GorillaSurfaceOverride>());
                        } else
                        {
                            overrideIndexs.Add(8);
                        }
                        gorillaSurfaceOverrides.Add(gameObjects[i].transform.gameObject.AddComponent<GorillaSurfaceOverride>());
                        gorillaSurfaceOverrides[i].overrideIndex = overrideIndexs[i];
                    }
                    fakestart = false;
                }

                List<InputDevice> list = new List<InputDevice>();
                InputDevices.GetDevices(list);

                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].characteristics.HasFlag(InputDeviceCharacteristics.Right))
                    {
                        list[i].TryGetFeatureValue(CommonUsages.secondaryButton, out isPressingB);
                    }
                }


                if (isPressingB)
                {
                    if (!justPressedB)
                    {
                        startSlide();
                        justPressedB = true;
                    }
                }
                else
                {
                    if (justPressedB)
                    {
                        stopSlide();
                        justPressedB = false;
                    }
                }
            }
        }
        [ModdedGamemodeJoin]
        public void RoomJoined(string gamemode)
        {
            
            inAllowedRoom = true;
        }

        [ModdedGamemodeLeave]
        public void RoomLeft(string gamemode)
        {
            // The room was left. Disable mod stuff.
            inAllowedRoom = false;
            stopSlide();
        }
        private void GameInitialized(object sender, EventArgs e)
        {
            
        }

        void startSlide()
        {
            for (int i = 0; i < gorillaSurfaceOverrides.Count; i++)
            {
                gorillaSurfaceOverrides[i].overrideIndex = 61;
            }
        }

        void stopSlide()
        {
            for (int i = 0; i < gorillaSurfaceOverrides.Count; i++)
            {
                gorillaSurfaceOverrides[i].overrideIndex = overrideIndexs[i];
            }
        }

        void OnEnable()
        {
            hauntedenabled = true;
        }

        void OnDisable()
        {
            hauntedenabled = false;
            stopSlide();
        }
    }
}

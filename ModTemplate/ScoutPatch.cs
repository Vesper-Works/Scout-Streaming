using OWML.ModHelper;
using OWML.Common;
using OWML.ModHelper.Events;
using HarmonyLib;
using UnityEngine;
using System.Reflection;

namespace ScoutStreaming
{
    public class ScoutPatch : ModBehaviour
    {
        public static ScoutPatch Instance { get; set; }
        private ProbeCamera probeCamera;
        private QuantumObject[] quantumObjects;
        private MethodInfo snapshotMethod;
        private void Start()
        {
            ModHelper.Console.WriteLine("Skipping splash screen...");
            var titleScreenAnimation = FindObjectOfType<TitleScreenAnimation>();
            titleScreenAnimation.SetValue("_fadeDuration", 0);
            titleScreenAnimation.SetValue("_gamepadSplash", false);
            titleScreenAnimation.SetValue("_introPan", false);
            titleScreenAnimation.Invoke("FadeInTitleLogo");
            ModHelper.Console.WriteLine("Done!");

   
            Instance = this;

            snapshotMethod = typeof(QuantumObject).GetMethod("OnProbeSnapshot",
                      BindingFlags.NonPublic | BindingFlags.Instance); 

            ModHelper.HarmonyHelper.AddPostfix<ProbeLauncher>("TakeSnapshotWithCamera", typeof(ScoutPatch), "SnapshotPatch");
            ModHelper.HarmonyHelper.AddPostfix<SatelliteSnapshotController>("OnPressInteract", typeof(ScoutPatch), "HearthianSatteliteOnPatch");
            ModHelper.HarmonyHelper.AddPostfix<SatelliteSnapshotController>("TurnOffProjector", typeof(ScoutPatch), "HearthianSatteliteOffPatch");
            ModHelper.Events.Scenes.OnCompleteSceneChange += OnCompleteSceneChange;
        }
        private void Update()
        {
            if (probeCamera != null && probeCamera.isActiveAndEnabled)
            {
                foreach (var thing in quantumObjects)
                {
                    snapshotMethod.Invoke(thing, new object[] { probeCamera });
                }
            }
        }
        private void OnCompleteSceneChange(OWScene oldScene, OWScene newScene)
        {
            if (newScene == OWScene.SolarSystem)
            {
                quantumObjects = FindObjectsOfType<QuantumObject>();
                OWML.Utils.TypeExtensions.SetValue(FindObjectOfType<ProbeLauncherUI>(), "s_takeSnapshotPrompt", new ScreenPrompt("Start Filming"));
            }

        }
        private static void SnapshotPatch(ProbeCamera camera)
        {
            Instance.probeCamera = camera;

            foreach (var probeCamera in FindObjectsOfType<ProbeCamera>())
            {
                probeCamera.GetComponent<OWCamera>().enabled = false;
            }


            camera.GetOWCamera().enabled = true;
            Instance.ModHelper.Console.WriteLine(camera.GetOWCamera().gameObject.name + " on");
        }

        private static void HearthianSatteliteOnPatch()
        {
            SatelliteSnapshotController museumProjector = FindObjectOfType<SatelliteSnapshotController>();
            var value = OWML.Utils.TypeExtensions.GetValue<OWCamera>(museumProjector, "_satelliteCamera");
            value.enabled = true;
        }   
        private static void HearthianSatteliteOffPatch()
        {
            SatelliteSnapshotController museumProjector = FindObjectOfType<SatelliteSnapshotController>();
            var value = OWML.Utils.TypeExtensions.GetValue<OWCamera>(museumProjector, "_satelliteCamera");
            value.enabled = false;
        }

    }
}

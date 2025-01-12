﻿using OWML.ModHelper;
using OWML.Common;

using HarmonyLib;
using UnityEngine;
using System.Reflection;
using UnityEngine.InputSystem;
using OWML.Utils;

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
            Instance = this;

            snapshotMethod = typeof(QuantumObject).GetMethod("OnProbeSnapshot",
                      BindingFlags.NonPublic | BindingFlags.Instance);

            ModHelper.HarmonyHelper.AddPostfix<ProbeLauncher>("TakeSnapshotWithCamera", typeof(ScoutPatch), "SnapshotPatch");
            //ModHelper.HarmonyHelper.AddPostfix<ProbeLauncher>("EquipTool", typeof(ScoutPatch), "EquipToolPatch");
            //ModHelper.HarmonyHelper.AddPostfix<ProbeLauncher>("LaunchProbe", typeof(ScoutPatch), "LaunchProbePatch");
            //ModHelper.HarmonyHelper.AddPostfix<ProbeLauncher>("RetrieveProbe", typeof(ScoutPatch), "RetrieveProbePatch");
            //ModHelper.HarmonyHelper.AddPostfix<SurveyorProbe>("OnAnchor", typeof(ScoutPatch), "OnAnchorPatch");
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
                    if(thing == null) { continue; }
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

        private static void EquipToolPatch()
        {
            foreach (var launcher in FindObjectsOfType<ProbeLauncher>())
            {
                if (launcher.IsEquipped())
                {
                    launcher.SendMessage("TakeSnapshotWithCamera", launcher.GetValue<ProbeCamera>("_preLaunchCamera"));
                    EnableCamera(launcher.GetValue<ProbeCamera>("_preLaunchCamera"));
                }
            }

        }

        private static void LaunchProbePatch()
        {
            foreach (var launcher in FindObjectsOfType<ProbeLauncher>())
            {
                if (launcher.IsEquipped())
                {
                    launcher.SendMessage("TakeSnapshotWithCamera", launcher.GetValue<SurveyorProbe>("_activeProbe").GetForwardCamera());
                    EnableCamera(launcher.GetValue<SurveyorProbe>("_activeProbe").GetForwardCamera());
                }
            }

        }

        private static void RetrieveProbePatch()
        {
            Instance.Invoke("RetakeSnapshotAfterRetrieval", 0.5f);
        }
        private static void OnAnchorPatch()
        {
            foreach (var launcher in FindObjectsOfType<ProbeLauncher>())
            {
                if (launcher.IsEquipped())
                {
                    launcher.SendMessage("TakeSnapshotWithCamera", launcher.GetValue<SurveyorProbe>("_activeProbe").GetRotatingCamera());
                    EnableCamera(launcher.GetValue<SurveyorProbe>("_activeProbe").GetRotatingCamera());
                }
            }
        }

        private static void SnapshotPatch(ProbeCamera camera)
        {
            Instance.probeCamera = camera;
            foreach (var probeCamera in FindObjectsOfType<ProbeCamera>())
            {
                probeCamera.GetOWCamera().enabled = false;
            }
            EnableCamera(camera);
        }

        private static void EnableCamera(ProbeCamera camera)
        {
            Instance.probeCamera = camera;
            foreach (var probeCamera in FindObjectsOfType<ProbeCamera>())
            {
                probeCamera.GetComponent<OWCamera>().enabled = false;
            }
            camera.GetOWCamera().enabled = true;
        }
        private void RetakeSnapshotAfterRetrieval()
        {
            foreach (var launcher in FindObjectsOfType<ProbeLauncher>())
            {
                if (launcher.IsEquipped())
                {
                    launcher.SendMessage("TakeSnapshotWithCamera", launcher.GetValue<ProbeCamera>("_preLaunchCamera"));
                    EnableCamera(launcher.GetValue<ProbeCamera>("_preLaunchCamera"));
                }
            }

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

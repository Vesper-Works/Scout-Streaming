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
            snapshotMethod = typeof(QuantumObject).GetMethod("OnProbeSnapshot",
                   BindingFlags.NonPublic | BindingFlags.Instance);
            Instance = this;

            ModHelper.HarmonyHelper.AddPostfix<ProbeLauncher>("TakeSnapshotWithCamera", typeof(ScoutPatch), "SnapshotPatch");
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
            }
        }
        private static void SnapshotPatch(ProbeCamera camera)
        {
            Instance.probeCamera = camera;

            switch (camera.GetOWCamera().gameObject.name)
            {
                case "RearCamera":
                    GameObject.Find("RotatingCamera").GetComponent<OWCamera>().enabled = false;
                    GameObject.Find("ForwardCamera").GetComponent<OWCamera>().enabled = false;
                    break;
                case "RotatingCamera":
                    GameObject.Find("RearCamera").GetComponent<OWCamera>().enabled = false;
                    GameObject.Find("ForwardCamera").GetComponent<OWCamera>().enabled = false;
                    break;
                case "ForwardCamera":
                    GameObject.Find("RotatingCamera").GetComponent<OWCamera>().enabled = false;
                    GameObject.Find("RearCamera").GetComponent<OWCamera>().enabled = false;
                    break;
            }

            camera.GetOWCamera().enabled = true;
            Instance.ModHelper.Console.WriteLine(camera.GetOWCamera().gameObject.name + " on");
        }


    }
}

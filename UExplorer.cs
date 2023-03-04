using Modding;
using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityExplorer;
using UnityExplorer.Inspectors;
using UnityExplorer.Inspectors.MouseInspectors;

namespace UExplorer
{
    public class UExplorer : Mod
    {
        public override string GetVersion() => typeof(ExplorerStandalone).Assembly.GetName().Version.ToString();

        public UExplorer() : base("Unity Explorer") { }

        private GameObject UEobject;
        internal static UExplorer Instance;
        private bool isInitialized = false;

        public override void Initialize()
        {
            Instance = this;
            if (UEobject == null)
            {
                UEobject = new GameObject("Unity Explorer Silent Object");
                GameObject.DontDestroyOnLoad(UEobject);
                UEobject.AddComponent<KeyboardMono>();
            }
        }

        internal void InitExplorer()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                ExplorerStandalone.CreateInstance();
                ExplorerStandalone.OnLog += OnLog;
                PatchWorldInspector();
            }
        }

        private static void PatchWorldInspector()
        {
  
            On.UnityExplorer.Inspectors.MouseInspectors.WorldInspector.UpdateMouseInspect += WorldInspector_UpdateMouseInspect;
        }

        private static void WorldInspector_UpdateMouseInspect(On.UnityExplorer.Inspectors.MouseInspectors.WorldInspector.orig_UpdateMouseInspect orig, 
            WorldInspector self, Vector2 _)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                MouseInspector.Instance.StopInspect();
                return;
            }
            var mousePos = Input.mousePosition;
            mousePos.z = cam.WorldToScreenPoint(Vector3.zero).z;
            var worldPos = cam.ScreenToWorldPoint(mousePos);
            var hits = Physics2D.OverlapPointAll(worldPos, Physics2D.AllLayers);
            var hit = hits.FirstOrDefault(x => x.transform.position.z > 0)
                ?? hits.LastOrDefault();
            if (hit == null)
            {
                MouseInspectorR.Instance.ClearHitData();
            }
            else
            {
                self.Reflect().OnHitGameObject(hit.gameObject);
            }
        }

        private void OnLog(string message, LogType logType)
        {
            string prefix = "[Explorer] ";
            switch (logType)
            {
                case LogType.Assert:
                    LogFine(prefix + message);
                    break;
                case LogType.Error:
                    LogError(prefix + message);
                    break;
                case LogType.Exception:
                    LogDebug(prefix + message);
                    break;
                case LogType.Log:
                    Log(prefix + message);
                    break;
                case LogType.Warning:
                    LogWarn(prefix + message);
                    break;
            }
        }
    }
}
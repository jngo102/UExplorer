using System;
using System.Linq;
using System.Reflection;
using Modding;
using UnityEngine;
using UnityExplorer;
using MonoMod.RuntimeDetour.HookGen;
using UnityExplorer.Inspectors;
using UnityExplorer.Inspectors.MouseInspectors;

namespace UExplorer
{
    public class UExplorer : Mod
    {
        public override string GetVersion() => "1.0.0.0";

        public UExplorer() : base("Unity Explorer") { }

        public override void Initialize()
        {
            ExplorerStandalone.CreateInstance();
            ExplorerStandalone.OnLog += OnLog;

            PatchWorldInspector();
        }

        //private static IDetour detour_UpdateMouseInspect;
        private static MethodInfo methodInfo_OnHitGameObject = typeof(WorldInspector)
            .GetMethod("OnHitGameObject", BindingFlags.Instance | BindingFlags.NonPublic);
        private static MethodInfo methodInfo_ClearHitData = typeof(MouseInspector)
            .GetMethod("ClearHitData", BindingFlags.Instance | BindingFlags.NonPublic);
        private static void PatchWorldInspector()
        {
            var MUpdateMouseInspect = typeof(WorldInspector)
                .GetRuntimeMethod("UpdateMouseInspect", new Type[] { typeof(Vector2) });
            HookEndpointManager.Add(MUpdateMouseInspect,
                new Action<Action<WorldInspector, Vector2>, WorldInspector, Vector2>(UpdateMouseInspectHook));
            /*detour_UpdateMouseInspect = new Hook(MUpdateMouseInspect,
                typeof(UExplorer).GetMethod("UpdateMouseInspectHook", BindingFlags.Static | BindingFlags.NonPublic));*/

        }
        private static void UpdateMouseInspectHook(Action<WorldInspector, Vector2> _,
            WorldInspector self, Vector2 _1)
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
                //if (ReflectionHelper.GetField<WorldInspector, GameObject>(self, "lastHitObject") != null)
                //{
                methodInfo_ClearHitData.Invoke(MouseInspector.Instance, Array.Empty<object>());
                //}
            }
            else
            {
                methodInfo_OnHitGameObject.Invoke(self, new object[] { hit.gameObject });
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
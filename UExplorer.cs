using Modding;
using UnityEngine;
using UnityExplorer;

namespace UExplorer
{
    public class UExplorer : Mod
    {
        public override string GetVersion() => "1.0.0.0";

        public UExplorer() : base("Unity Explorer") {}

        public override void Initialize()
        {
            ExplorerStandalone.CreateInstance();
            ExplorerStandalone.OnLog += OnLog;
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
using UnityEngine;

namespace UExplorer
{
    public class KeyboardMono : MonoBehaviour
    {
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F7))
            {
                UExplorer.Instance.InitExplorer();
            }
        }
    }
}
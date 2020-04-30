using UnityEngine;

namespace PrjCore.Util.Unity {
    public static class PApplicationUtil {

        public static void Quit(int exitCode) {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            if (exitCode == 0) {
                Debug.LogFormat("Application quit with code {0}", 0);
            } else {
                Debug.LogErrorFormat("Application quit with code {0}", exitCode);
            }
#else
            Application.Quit(exitCode);
#endif
        }
        
    }
}
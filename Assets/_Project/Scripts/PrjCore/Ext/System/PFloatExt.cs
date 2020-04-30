using UnityEngine;

namespace PrjCore.Util.System {
    public static class PFloatExt {
        public static float AngleToDegree(this float angle) {
            return Mathf.Rad2Deg * angle;
        }

        public static float AngleToRad(this float angle) {
            return Mathf.Deg2Rad * angle;
        }
        
    }
}